//Copyright 2016 Malooba Ltd

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using FlowShared.Json;
using FlowShared.Db;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowDecider
{
    public sealed class Decider : ServiceBase
    {
        private const string DEFAULT_DECISION_LIST = "decider";
        private const int DECIDER_RETRY_DELAY = 5000;
        private static ILog log;
        private static Task loopTask;
        private static CancellationTokenSource cts;
        private static CancellationToken token;


        public Decider()
        {
            log = LogManager.GetLogger(typeof(Database));

            if(Environment.UserInteractive)
            {
                log.Info("Starting in console");
#if DEBUG
                log.Info("Cleaning database");
                using(var db = new Database())
                {
                    db.CleanDatabase();
                }
                log.Info("Starting test workflow");
                StartTestWorkflow();
#endif
                var exit = new ManualResetEvent(false);
                OnStart(null);
                Console.CancelKeyPress += delegate {
                    OnStop();
                    exit.Set();
                };
                exit.WaitOne();
            }
            else
            {
                log.Info("Starting as service");
                Run(this);
            }
        }

        protected override void OnStart(string[] args)
        {
            cts = new CancellationTokenSource();
            token = cts.Token;
            log.Info("Starting loop");
            loopTask = Task.Factory.StartNew(DeciderLoop, cts.Token);
            log.Info("Loop started");
        }

        protected override void OnStop()
        {
            log.Info("Got OnStop() event");
            cts.Cancel();
            log.Info("Token cancelled");
            loopTask.Wait();
            log.Info("Decider loop completed");
        }

        /// <summary>
        /// Loop over available tasks and process new history items for each one
        /// </summary>
        private static void DeciderLoop()
        {
            log.Info("In decider loop");
            while(!token.IsCancellationRequested)
            {
                try
                {
                    Execution execution;
                    bool updatedOk;

                    using(var db = new Database())
                    {
                        execution = GetDeciderTask(db, DEFAULT_DECISION_LIST);

                        if(execution == null)
                        {
                            Task.Delay(DECIDER_RETRY_DELAY, token).Wait();
                            continue;
                        }

                        int historySeen;
                        var decisions = ProcessHistoryItems(db, execution, out historySeen);

                        updatedOk = StoreDecisions(db, execution, decisions, historySeen);
                    }
                    if(!updatedOk)
                    {
                        // Dump the failed context and flag the execution as requiring attention again
                        using(var db = new Database())
                            RetryDeciderTask(db, execution.ExecutionId);
                    }
                }
                catch(Exception ex)
                {
                    log.Error("Uncaught exception in DeciderLoop", ex);
                }
            }
        }

        /// <summary>
        /// Debugging function to start a simple workflow
        /// </summary>
        private static void StartTestWorkflow()
        {
            var r = new Random();
            var jobId = r.Next(999999);
            var json = $"{{'workflowName':'adderasynch', 'workflowVersion':'2.0.0.0', 'input':{{ '_job': {{'x':5, 'y':3}}, _jobType: 'test', _jobId: '{jobId}' }}}}";
            // var json = $"{{'workflowName':'testHeartbeat', 'workflowVersion':'1.0.0.0', 'input':{{ '_job': {{'x':5, 'y':3}}, _jobType: 'test', _jobId: '{jobId}' }}}}";
            using(var db = new Database())
                db.StartWorkflow(json);
        }

        /// <summary>
        /// Fetch the next running execution that has work to be done (AwaitingDecision == true)
        /// priority is given to tasks that have not been seen for the longest time
        /// </summary>
        /// <param name="db"></param>
        /// <param name="decisionQueue"></param>
        /// <returns></returns>
        private static Execution GetDeciderTask(Database db, string decisionQueue)
        {
            while(true)
            {
                var exec =
                    (from ex in db.Executions
                     join st in db.ExecutionStates 
                     on ex.ExecutionId equals st.ExecutionId
                     where ex.DecisionList == decisionQueue && ex.AwaitingDecision && (st.State == ExState.Running || st.State == ExState.Cleanup)
                     orderby ex.LastSeen
                     select ex).FirstOrDefault();

                // Nothing to do
                if(exec == null)
                {
                    log.Info("Nothing to do");
                    return null;
                }

                try
                {
                    exec.AwaitingDecision = false;
                    exec.DeciderToken = Guid.NewGuid();
                    db.SubmitChanges(ConflictMode.FailOnFirstConflict);
                }
                catch(ChangeConflictException ex)
                {
                    log.Info("Ignoring ChangeConflictException in GetDeciderTask()", ex);
                    db.Refresh(RefreshMode.OverwriteCurrentValues, exec);
                    continue;
                }
                return exec;
            }
        }

        /// <summary>
        /// For each history item that requires attention, process it and accumulate the resulting decisions.
        /// If there is no new history then historySeen will be 0.
        /// Otherwise, historySeen will be the Id of the last history event processed.
        /// Any execution marked as AwaitingDecision *should* have new history to process so we *should* never see 0 in historySeen.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="execution"></param>
        /// <param name="historySeen">The Id of the last history item seen by the decider</param>
        /// <returns></returns>
        private static IEnumerable<History> ProcessHistoryItems(Database db, Execution execution, out int historySeen)
        {
            var decisions = new List<History>();
            var workflow = JsonConvert.DeserializeObject<WorkflowObj>(execution.Workflow.Json);
            var history = from h in execution.Histories
                          where h.Id > execution.HistorySeen
                          orderby h.Id
                          select h;

            historySeen = 0;

            log.Info($"Processing events for execution {execution.ExecutionId}");
            foreach (var h in history)
            {
                log.Info($"Processing {h.EventType}");
                switch (h.EventType)
                {
                    case "WorkflowExecutionStartedEvent":
                        ProcessWorkflowExecutionStartedEvent(db, workflow, execution, h, decisions);
                        break;

                    case "WorkflowExecutionCancelledEvent":
                        db.CancelActivityTasks(execution);
                        AttemptCleanup(db, execution, workflow, decisions);
                        break;

                    case "ActivityTaskCompletedEvent":
                        ProcessActivityTaskCompletedEvent(db, workflow, execution, h, decisions);
                        break;

                    case "ActivityTaskFailedEvent":
                        ProcessActivityTaskFailedEvent(db, workflow, execution, h, decisions);
                        break;

                    case "ActivityTaskCancelledEvent":
                        //ProcessActivityTaskCancelledEvent(db, workflow, execution, h, decisions);
                        break;

                    case "WorkflowSignalledEvent":
                        //
                        break;

                    // A workflow can be stopeed from the REST interface so we need to action it here
                    case "WorkflowStoppedEvent":
                        execution.ExecutionState.State = ExState.Stopped;
                        return decisions;

                }
                historySeen = h.Id;
            }
            log.Info($"Finished Processing events for execution {execution.ExecutionId}");

            return decisions;
        }

        /// <summary>
        /// Two stage workflow execution start.
        /// First create and initialise the workflow variables
        /// Then schedule the first task
        /// If this process is interrupted then the variables
        /// </summary>
        /// <param name="db"></param>
        /// <param name="workflow"></param>
        /// <param name="execution"></param>
        /// <param name="history"></param>
        /// <param name="decisions"></param>
        private static void ProcessWorkflowExecutionStartedEvent(Database db, WorkflowObj workflow, Execution execution, History history, List<History> decisions)
        {
            var evt = WorkflowExecutionStartedEvent.Create(history);
            CreateVariables(db, evt, execution, workflow);
            // Ensure that the variables are set up for the first task
            // This should never fail due to conflict
            db.SubmitChanges();

            // Scheduling the first task
            var startTaskId = workflow.Tasks.Single(t => t.ActivityName == "start").Outflows.Single(o => o.Name == "Out").Target;
            var startTask = workflow.Tasks.Single(t => t.TaskId == startTaskId);
            CreateTaskScheduledEvent(db, execution, startTask, decisions);
        }

        /// <summary>
        /// Process task completion by scheduling the next task or completing the workflow
        /// </summary>
        /// <param name="db"></param>
        /// <param name="workflow"></param>
        /// <param name="execution"></param>
        /// <param name="history"></param>
        /// <param name="decisions"></param>
        private static void ProcessActivityTaskCompletedEvent(Database db, WorkflowObj workflow, Execution execution, History history, List<History> decisions)
        {
            // There should(!) be no contention for the data modified in this process
            var evt = ActivityTaskCompletedEvent.Create(history);
            var se = ActivityTaskScheduledEvent.Create(db.GetHistoryEvent(execution.ExecutionId, evt.SchedulingEventId));
            var completedTask = workflow.Tasks.Single(t => t.TaskId == se.TaskId);

            // Default task outflow
            var outflow = "Out";
            // Update variables
            if (evt.Result != null)
            {

                // Update the variables if there are any normal results (not prefixed with "$")
                if (evt.Result.Properties().Any(p => !p.Name.StartsWith("$")))
                {
                    var variables = db.Variables.Where(v => v.Execution == execution).ToArray();
                    foreach (var o in completedTask.Outputs.Where(o => o.Value.Var != null))
                    {
                        // If the activity has not returned a value for an output then we don't update the mapped variable
                        // In general it is probably best if activities return values for all outputs to avoid confusion
                        JToken value;
                        if (evt.Result.TryGetValue(o.Key, out value))
                            variables.Single(v => v.Name == o.Value.Var).Json = value.ToString(Formatting.None);
                    }
                }
                // Get the correct outflow
                JToken outflowToken;
                if (evt.Result.TryGetValue("$outflow", out outflowToken))
                {
                    if (outflowToken.Type == JTokenType.String)
                        outflow = (string)outflowToken;
                    else
                        throw new ApplicationException("Task outflow identifier must be a string");
                }
            }

            var nextTaskId = completedTask.Outflows.Single(o => o.Name == outflow).Target;
            var nextTask = workflow.Tasks.Single(t => t.TaskId == nextTaskId);

            // A task with no outflows is an end
            if(nextTask.Outflows.Length == 0)
            {
                Console.WriteLine($"Execution state = {execution.ExecutionState.State}");
                if(ExState.Create(execution.ExecutionState.State) != ExState.Cleanup)
                    CreateWorkflowCompletedEvent(execution, decisions);
                AttemptCleanup(db, execution, workflow, decisions);
            }
            else
            {
                CreateTaskScheduledEvent(db, execution, nextTask, decisions);
            }
        }

        /// <summary>
        /// When a task fails, follow the error outflow if one exists else fail the workflow
        /// </summary>
        /// <param name="db"></param>
        /// <param name="workflow"></param>
        /// <param name="execution"></param>
        /// <param name="history"></param>
        /// <param name="decisions"></param>
        private static void ProcessActivityTaskFailedEvent(Database db, WorkflowObj workflow, Execution execution, History history, List<History> decisions)
        {
            // There should(!) be no contention for the data modified in this process
            var evt = ActivityTaskFailedEvent.Create(history);
            var se = ActivityTaskScheduledEvent.Create(db.GetHistoryEvent(execution.ExecutionId, evt.SchedulingEventId));
            var failedTask = workflow.Tasks.Single(t => t.TaskId == se.TaskId);

            var nextTaskId = failedTask.FailOutflow?.Target;
            if (!string.IsNullOrEmpty(nextTaskId))
            {
                var nextTask = workflow.Tasks.Single(t => t.TaskId == nextTaskId);
                CreateTaskScheduledEvent(db, execution, nextTask, decisions);
            }
            decisions.Add(new WorkflowExecutionFailedEvent
            {
                Reason = $"Task {se.TaskId} failed with no recovery action defined"
            });

            AttemptCleanup(db, execution, workflow, decisions);
        }

        /// <summary>
        /// Store decisions and create a Tasklist item for each ActivityTaskScheduledEvents
        /// </summary>
        /// <param name="db"></param>
        /// <param name="execution"></param>
        /// <param name="decisions"></param>
        /// <param name="historySeen"></param>
        /// <returns></returns>
        private static bool StoreDecisions(Database db, Execution execution, IEnumerable<History> decisions, int historySeen)
        {
            foreach(var decision in decisions)
            {
                // Grab the history 
                var historyId = Database.InsertHistory(execution, decision);

                TaskList qt;
                if(ActivityTaskScheduledEvent.CanCreate(decision))
                {
                    var atse = ActivityTaskScheduledEvent.Create(decision);

                    const int YEAR = 3600 * 12 * 365;

                    qt = new TaskList
                    {
                        ExecutionId = execution.ExecutionId,
                        JobId = execution.JobId,
                        ListName = atse.TaskList,
                        TaskToken = Guid.NewGuid(),
                        TaskScheduledEventId = historyId,
                        Priority = atse.TaskPriority,
                        HeartbeatTimeout = atse.HeartbeatTimeout,
                        TaskAlarm = DateTime.UtcNow.AddSeconds(Math.Min(atse.ScheduleToCloseTimeout ?? YEAR, atse.ScheduleToStartTimeout ?? YEAR)),
                        ScheduledAt = DateTime.UtcNow,
                        TaskSheduleToCloseTimeout = atse.ScheduleToCloseTimeout,
                        TaskStartToCloseTimeout = atse.StartToCloseTimeout,
                        ProgressData = (atse.Input.SelectToken("progressData") ?? JValue.CreateNull()).ToString(Formatting.None)
                    };
                    db.TaskLists.InsertOnSubmit(qt);
                }

                // Notify updater
                else if(WorkflowExecutionFailedEvent.CanCreate(decision))
                {
                    var wefe = WorkflowExecutionFailedEvent.Create(decision);
                    var data = new JObject(
                        new JProperty("type", wefe.EventType),
                        new JProperty("reason", wefe.Reason));
                    qt = Database.CreateUpdaterNotification(execution, data);
                    db.TaskLists.InsertOnSubmit(qt);
                }
                else if(WorkflowCleanupStartedEvent.CanCreate(decision))
                {
                    execution.ExecutionState.State = ExState.Cleanup;
                }
            }
            try
            {
                if(historySeen != 0)
                    execution.HistorySeen = historySeen;

                execution.DeciderToken = null;
                execution.LastSeen = DateTime.UtcNow;           
                db.SubmitChanges();
            }
            catch (ChangeConflictException)
            {
                db.Refresh(RefreshMode.KeepCurrentValues, execution);
                db.SubmitChanges();
            }

            return true;
        }

        private static void AttemptCleanup(Database db, Execution execution, WorkflowObj workflow, List<History> decisions)
        {
            if(ExState.Create(execution.ExecutionState.State) == ExState.Cleanup)
            {
                StopWorkflow(execution, decisions);
                return;
            }
            try
            {
                var cleanupStartTask = workflow.Tasks.SingleOrDefault(t => t.ActivityName == "cleanup");
                if(cleanupStartTask == null)
                {
                    StopWorkflow(execution, decisions);
                    return;
                }
                var cleanupTaskId = cleanupStartTask.Outflows.Single(o => o.Name == "Out").Target;
                var cleanupTask = workflow.Tasks.Single(t => t.TaskId == cleanupTaskId);
                decisions.Add(new WorkflowCleanupStartedEvent());
                CreateTaskScheduledEvent(db, execution, cleanupTask, decisions);
            }
            catch(Exception ex)
            {
                log.Error("Exception thrown from cleanup", ex);
                StopWorkflow(execution, decisions);
            }
        }

        private static void StopWorkflow(Execution execution, List<History> decisions)
        {
            execution.ExecutionState.State = ExState.Stopped;
            decisions.Add(new WorkflowStoppedEvent());
        }

        /// <summary>
        /// If a decider update has failed then clear the token and set the AwaitingDecision flag so we can try again
        /// </summary>
        /// <param name="db"></param>
        /// <param name="executionId"></param>
        private static void RetryDeciderTask(Database db, Guid executionId)
        {
            var exec = db.Executions.SingleOrDefault(e => e.ExecutionId == executionId);
            if (exec != null)
            {
                exec.AwaitingDecision = true;
                exec.DeciderToken = null;
                try
                {
                    db.SubmitChanges();
                }
                catch (ChangeConflictException ex)
                {
                    // We tried.. Let the decider timeout pick this execution up later 
                    log.Error("Failed to retry decider task", ex);
                }
            }
        }

        /// <summary>
        /// Create all workflow variables and initialise them from the workflow input data
        /// </summary>
        /// <param name="db"></param>
        /// <param name="evt"></param>
        /// <param name="execution"></param>
        /// <param name="workflow"></param>
        private static void CreateVariables(Database db, WorkflowExecutionStartedEvent evt, Execution execution, WorkflowObj workflow)
        {
            foreach (var vdefn in workflow.Variables)
            {
                // Get the initial value of the variable
                JToken value = null;

                if(!string.IsNullOrWhiteSpace(vdefn.Value.Path))
                {
                    value = evt.Input.SelectToken(vdefn.Value.Path);

                    if(value == null)  // The path expression failed
                    {
                        if(vdefn.Value.Required)
                            throw new ApplicationException("Uninitialised required variable - " + vdefn.Key);
                    }

                    // Empty default is equivalent to an explicit null
                    if(IsNullOrUndefined(value) && !string.IsNullOrWhiteSpace(vdefn.Value.Default))
                        value = JToken.Parse(vdefn.Value.Default);
                }
                // Empty literal is equivalent to an explicit null
                else if(!string.IsNullOrWhiteSpace(vdefn.Value.Lit))
                    value = JToken.Parse(vdefn.Value.Lit);

                // TODO: Check datatypes for validity

                db.Variables.InsertOnSubmit(new Variable
                {
                    ExecutionId = execution.ExecutionId,
                    Name = vdefn.Key,
                    Json = JsonConvert.SerializeObject(value)
                });
            }
        }

        /// <summary>
        /// Create a workflow completed event and add it to the decision list
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="decisions"></param>
        private static void CreateWorkflowCompletedEvent(Execution execution, List<History> decisions)
        {
            var wfce = new WorkflowExecutionCompletedEvent
            {
                ExecutionId = execution.ExecutionId
            };
            decisions.Add(wfce);
        }

        /// <summary>
        /// Create a task scheduled event and add it to the decision list
        /// </summary>
        /// <param name="db"></param>
        /// <param name="execution"></param>
        /// <param name="task"></param>
        /// <param name="decisions"></param>
        private static void CreateTaskScheduledEvent(Database db, Execution execution, TaskObj task, ICollection<History> decisions)
        {
            var act = db.GetActivity(task.ActivityName, task.ActivityVersion);

            var input = GetTaskInput(execution, task);
            var activity = JsonConvert.DeserializeObject<ActivityObj>(act.Json);
            var atse = new ActivityTaskScheduledEvent
            {
                ActivityId = Guid.NewGuid().ToString(),
                ActivityName = task.ActivityName,
                ActivityVersion = task.ActivityVersion,
                TaskList = task.TaskList ?? activity.DefaultTaskList, // TODO: Should this be overridable in the task definition? Probably!
                TaskId = task.TaskId,
                TaskPriority = task.TaskPriority ?? activity.DefaultPriority ?? 0,
                HeartbeatTimeout = (int?)(task.HeartbeatTimeout ?? activity.DefaultTaskHeartbeatTimeout),
                ScheduleToCloseTimeout = (int?)(task.ScheduleToCloseTimeout ?? activity.DefaultTaskScheduleToCloseTimeout),
                ScheduleToStartTimeout = (int?)(task.ScheduleToStartTimeout ?? activity.DefaultTaskScheduleToStartTimeout),
                StartToCloseTimeout = (int?)task.StartToCloseTimeout,
                Input = input
            };
            decisions.Add(atse);
        }

        /// <summary>
        /// Generate JSON for activity inputs and outflows
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        private static JObject GetTaskInput(Execution execution, TaskObj task)
        {
            // Add outflows from task definition
            var inputs = new JObject(new JProperty("$outflows", new JArray(from o in task.Outflows select o.Name)));

            if(task.Inputs == null) return inputs;

            // Lookup input values and add them
            foreach(var input in task.Inputs)
            {
                JToken value = null;

                // Is the input from a variable?
                if(!string.IsNullOrWhiteSpace(input.Value.Var))
                {
                    var variable = execution.Variables.Single(v => v.Name == input.Value.Var);

                    // Does the variable have some content?
                    if(!string.IsNullOrWhiteSpace(variable?.Json))
                    {
                        value = JToken.Parse(variable.Json);
                        if(!string.IsNullOrWhiteSpace(input.Value.Path))
                        {
                            value = value.SelectToken(input.Value.Path);

                            // If required and the path target does not exist
                            // There is no error if the path target exists and is explicitly null - JValue.CreateNull()
                            if(value == null)
                            {
                                if(input.Value.Required)
                                    throw new ApplicationException($"Uninitialised required input - {input.Value.Var}[{input.Value.Path}] for task: {task.TaskId}");

                                value = input.Value.Default;
                            }
                        }
                    }
                    else if(!string.IsNullOrWhiteSpace(input.Value.Path))
                    {
                        if(input.Value.Required)
                            throw new ApplicationException($"Uninitialised required input - {input.Value.Var}[{input.Value.Path}] for task: {task.TaskId}");

                        value = input.Value.Default;
                    }
                    else
                    {
                        value = JValue.CreateNull();
                    }
                }
                else if(input.Value.Lit != null)
                {
                    value = input.Value.Lit;
                }

                // TODO: Check datatypes for validity

                inputs.Add(input.Key, value);
            }
            return inputs;
        }

        /// <summary>
        /// Utility function to detect empty JSON inputs
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool IsNullOrUndefined(JToken value)
        {
            return
                value == null ||
                value.Type == JTokenType.Null ||
                value.Type == JTokenType.Undefined;
        }
    }
}
