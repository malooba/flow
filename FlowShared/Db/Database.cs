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
using FlowShared.Json;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowShared.Db
{
    public class Database : DatabaseDataContext
    {
        private static readonly ILog log;
        private const int MAX_EVENTS = 1000;
        private const int RETRIES = 10;
        private const int NOTIFICATION_PRIORITY = 100;
        private const string UPDATER_TASKLIST = "updater";

        static Database()
        {
            log = LogManager.GetLogger(typeof(Database)); 
        }

        public void CleanDatabase()
        {
            ExecuteCommand("DELETE FROM Variables");
            ExecuteCommand("DELETE FROM Histories");
            ExecuteCommand("DELETE FROM TaskLists");
            ExecuteCommand("DELETE FROM ExecutionStates");
            ExecuteCommand("DELETE FROM Executions");
        }

        public IEnumerable<WorkflowId> GetWorkflows()
        {
            return from w in Workflows select new WorkflowId { Name = w.Name, Version = ConvertVersion(w.Version) };
        }

        public IEnumerable<object> GetWorkflows(string name)
        {
            return
                from w in Workflows
                where w.Name == name
                select new { Name = w.Name, Version = ConvertVersion(w.Version) };
        }

        public Workflow GetWorkflow(string name, string version = null)
        {
            if(version == null)
            {
                return
                    (from w in Workflows
                     where w.Name == name 
                     orderby w.Version descending 
                     select w).FirstOrDefault();
            }

            return
                (from w in Workflows
                 where w.Name == name && w.Version == ConvertVersion(version)
                 select w).SingleOrDefault();
        }

        public string StoreWorkflow(WorkflowObj wfObj)
        {
            try
            {
                var wf =
                    (from w in Workflows
                        where w.Name == wfObj.Name && w.Version == ConvertVersion(wfObj.Version)
                        select w).SingleOrDefault();

                if(wf == null)
                {
                    wf = new Workflow
                    {
                        Name = wfObj.Name,
                        Version = ConvertVersion(wfObj.Version),
                        // Description = acObj.Description,
                        Json = JsonConvert.SerializeObject(wfObj)
                    };
                    Workflows.InsertOnSubmit(wf);
                }

                wf.Json = JsonConvert.SerializeObject(wfObj);
                SubmitChanges(ConflictMode.FailOnFirstConflict);
                return "\"OK\"";
            }
            catch(Exception ex)
            {
                return "\"" + ex.Message + "\"";
            }
        }

        public bool DeleteWorkflows(string name)
        {
            var wfs =
                from w in Workflows
                where w.Name == name
                select w;

            if(!wfs.Any())
                return false;

            Workflows.DeleteAllOnSubmit(wfs);
            SubmitChanges();
            return true;
        }

        public bool DeleteWorkflow(string name, string version)
        {
            var wf = 
               (from w in Workflows
                where w.Name == name && w.Version == ConvertVersion(version)
                select w).SingleOrDefault();
            if(wf == null)
                return false;

            Workflows.DeleteOnSubmit(wf);
            SubmitChanges();
            return true;
        }


        public IEnumerable<object> GetActivities()
        {
            return from a in Activities select new { Name = a.Name, Version = ConvertVersion(a.Version) };
        }

        public IEnumerable<object> GetActivities(string name)
        {
            return
                from a in Activities
                where a.Name == name
                select new { Name = a.Name, Version = ConvertVersion(a.Version) };
        }


        public Activity GetActivity(string name, string version)
        {
            return
                (from a in Activities
                 where a.Name == name && a.Version == ConvertVersion(version)
                 select a).SingleOrDefault();
        }

        public bool StoreActivity(ActivityObj acObj)
        {
            try
            {
                var ac =
                    (from a in Activities
                     where a.Name == acObj.Name && a.Version == ConvertVersion(acObj.Version)
                     select a).SingleOrDefault();

                if(ac == null)
                {
                    ac = new Activity
                    {
                        Name = acObj.Name,
                        Version = ConvertVersion(acObj.Version),
                        Description = acObj.Description,
                        Json = JsonConvert.SerializeObject(acObj)
                    };
                    Activities.InsertOnSubmit(ac);
                }

                ac.Json = JsonConvert.SerializeObject(acObj);
                SubmitChanges(ConflictMode.FailOnFirstConflict);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public IEnumerable<History> GetExecutionHistory(Guid executionId, int start, int count)
        {
            if(count > MAX_EVENTS)
                count = MAX_EVENTS;
            return
                (from h in Histories
                 where h.ExecutionId == executionId
                 select h).Skip(start).Take(count);
        }

        public History GetHistoryEvent(Guid executionId, int eventId)
        {
            return Histories.Single(h => h.ExecutionId == executionId && h.Id == eventId);
        }

        /// <summary>
        /// Insert a history request info a workflow
        /// Returns the Id of the newly created history event
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="ev"></param>
        public static int InsertHistory(Execution execution, History ev)
        {
            using(var db = new Database())
            {
                InsertHistory(db, execution, ev);
                // This should never fail. Don't catch any exceptions so we quickly find out if this is not the case
                db.SubmitChanges();
                return ev.Id;
            }
        }

        /// <summary>
        /// Insert history using an existing Database instance and do not submit
        /// The new event Id is not made directly available
        /// </summary>
        /// <param name="db"></param>
        /// <param name="execution"></param>
        /// <param name="ev"></param>
        public static void InsertHistory(Database db, Execution execution, History ev)
        {
            ev.ExecutionId = execution.ExecutionId;
            ev.Timestamp = DateTime.UtcNow;
            db.Histories.InsertOnSubmit(ev);
        }


        /// <summary>
        /// Start a new workflow
        /// </summary>
        /// <param name="json">The entire message body from the request</param>
        /// <returns></returns>
        public string StartWorkflow(string json)
        {
            // This presently uses the Event as the body of the start message because it's convenient
            //
            var workflowName = "";
            var workflowVersion = ""; 
            var executionId = Guid.NewGuid();
            try
            {
                // Neither of these operation should ever fail as they cannot be conflicted
                var wfStartData = JsonConvert.DeserializeObject<WorkflowExecutionStartedEvent>(json);
                workflowName = wfStartData.WorkflowName;
                workflowVersion = wfStartData.WorkflowVersion;
                var wf = GetWorkflow(workflowName, workflowVersion);
                if(wf == null)
                {
                    log.Error($"Unknown workflow definition - {workflowName} v {workflowVersion} ");
                    throw new ApplicationException($"Unknown workflow definition - {workflowName} v {workflowVersion} ");
                }
                var wfDefinition = JsonConvert.DeserializeObject<WorkflowObj>(wf.Json);

                Executions.InsertOnSubmit(new Execution
                {
                    ExecutionId = executionId,
                    JobId = (string)wfStartData.Input.SelectToken("_jobId"),
                    Workflow = wf,
                    DecisionList = wfStartData.DecisionList ?? wfDefinition.DecisionList ?? "decider",
                    ExecutionStartToCloseTimeout = (int?)(wfStartData.ExecutionStartToCloseTimeout ?? wfDefinition.DefaultExecutionStartToCloseTimeout),
                    TaskStartToCloseTimeout = (int?)(wfStartData.TaskStartToCloseTimeout ?? wfDefinition.DefaultTaskStartToCloseTimeout),
                    TaskScheduleToCloseTimeout = null, // What goes here?
                    TaskScheduleToStartTimeout = null, // and here?
                    HistorySeen = 0,
                    AwaitingDecision = true,
                    LastSeen = DateTime.UtcNow,
                    ExecutionState = new ExecutionState {State = ExState.Running}
                });

                wfStartData.ExecutionId = executionId;
                wfStartData.Id = 0;
                wfStartData.Timestamp = DateTime.UtcNow;

                Histories.InsertOnSubmit(wfStartData);

                SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
            catch(ChangeConflictException ex)
            {
                log.Error("Failed to create new workflow execution", ex);
                throw new ApplicationException($"Failed to create new workflow execution - {workflowName} v {workflowVersion} ");
            }
            return new JObject(new JProperty("executionId", executionId.ToString())).ToString(Formatting.None);
        }

        /// <summary>
        /// Respond to a request for work by checking for a suitable waiting task
        /// If one is found, set up the timeout alarms, add the task started event and return the task to the worker
        /// </summary>
        /// <param name="workerId">The worker polling</param>
        /// <param name="taskListName">The tasklist to poll</param>
        /// <returns>The task, if one is available</returns>
        public string SelectAndStartTask(string workerId, string taskListName)
        {
            var task =
                (from t in TaskLists
                 where t.ListName == taskListName && t.WorkerId == null
                 orderby t.Priority, t.ScheduledAt  
                 select t).FirstOrDefault();

            // Nothing to do
            if(task == null) return null;

            // Check for a notification
            if(task.TaskScheduledEventId == 0)
            {
                var notifyTask = new ActivityTask
                {
                    ActivityId = null,
                    ActivityName = "$notify",
                    ActivityVersion = "1.0.0.0",
                    ExecutionId = task.ExecutionId,
                    JobId = task.JobId,
                    AsyncSignal = null,
                    Input = JObject.Parse(task.NotificationData),
                    TaskToken = task.TaskToken,
                    StartedEventId = 0
                };
                TaskLists.DeleteOnSubmit(task);
                SubmitChanges();
                return JsonConvert.SerializeObject(notifyTask);
            }

            task.WorkerId = workerId;
            task.StartedAt = DateTime.UtcNow;

            if(task.HeartbeatTimeout.HasValue)
                task.HeartbeatAlarm = DateTime.UtcNow.AddSeconds(task.HeartbeatTimeout.Value);

            var scheduleToCloseAlarm = DateTime.MinValue;
            if(task.TaskSheduleToCloseTimeout.HasValue)
                scheduleToCloseAlarm = task.ScheduledAt.AddSeconds(task.TaskSheduleToCloseTimeout.Value);

            var startToCloseAlarm = DateTime.MinValue;
            if(task.TaskStartToCloseTimeout.HasValue)
                startToCloseAlarm = task.ScheduledAt.AddSeconds(task.TaskStartToCloseTimeout.Value);

            // Select the earliest alarm time
            var alarm = scheduleToCloseAlarm < startToCloseAlarm ? scheduleToCloseAlarm : startToCloseAlarm;

            if(alarm > DateTime.MinValue)
                task.TaskAlarm = alarm;
            try
            {
                SubmitChanges();
            }
            catch(ChangeConflictException ex)
            {
                // The likely suspects are:
                // 1) Task timeout - OK, we were just too late
                // 2) Cancellation - OK we were just late enough
                // 3) Task taken by other worker - No problem
                // In all cases there is no error so we just bail
                log.Info("ChangeConflictException in SelectAndStartTask", ex);
                return null;
            }

            var atse = new ActivityTaskStartedEvent
            {
                WorkerId = workerId,
                ScheduledEventId = task.TaskScheduledEventId,
            };

            InsertHistory(this, task.Execution, atse);
            SubmitChanges();

            // Get the scheduling event
            var evt = ActivityTaskScheduledEvent.Create(Histories.Single(h => (h.ExecutionId == task.ExecutionId) && (h.Id == task.TaskScheduledEventId)));

            var at = new ActivityTask
            {
                ActivityId = evt.ActivityId,
                ActivityName = evt.ActivityName,
                ActivityVersion = evt.ActivityVersion,
                ExecutionId = evt.ExecutionId,
                JobId = task.JobId,
                AsyncSignal = evt.AsyncSignal,
                Input = evt.Input,
                TaskToken = task.TaskToken,
                StartedEventId = atse.Id
            };

            return JsonConvert.SerializeObject(at);
        }

        /// <summary>
        /// Handle response for a task including heartbeats
        /// The returned status property identifies the type of response
        /// </summary>
        /// <param name="taskToken"></param>
        /// <param name="json"></param>
        public string ProcessTaskResponse(Guid taskToken, string json)
        {
            try
            {
                var taskResponse = JsonConvert.DeserializeObject<ActivityTaskResponse>(json);
                // Note that the task may have been deleted by the ActivityTaskTimeoutChecker 
                var task = TaskLists.SingleOrDefault(q => q.TaskToken == taskToken);
                string response = null;

                int i;
                for(i = 0; i < RETRIES; i++)
                {
                    switch(taskResponse.Status.ToLower())
                    {
                        case "success":
                            if(task == null)
                            {
                                log.Info($"Ignoring success response for deleted task with token: {taskToken}");
                                break;
                            }
                            var result = taskResponse.Result;

                            var atce = new ActivityTaskCompletedEvent
                            {
                                Result = result,
                                SchedulingEventId = task.TaskScheduledEventId,
                            };

                            InsertHistory(this, task.Execution, atce);
                            task.Execution.AwaitingDecision = true;
                            TaskLists.DeleteOnSubmit(task);
                            break;

                        case "failure":
                            if(task == null)
                            {
                                log.Info($"Ignoring failure response for deleted task with token: {taskToken}");
                                break;
                            }
                            var atfe = new ActivityTaskFailedEvent
                            {
                                Reason = taskResponse.Reason,
                                Details = taskResponse.Details,
                                SchedulingEventId = task.TaskScheduledEventId
                            };
                            InsertHistory(this, task.Execution, atfe);
                            task.Execution.AwaitingDecision = true;
                            TaskLists.DeleteOnSubmit(task);
                            break;

                        case "cancelled":
                            if(task == null)
                            {
                                log.Info($"Ignoring cancellation response for deleted task with token: {taskToken}");
                                break;
                            }
                            var atxe = new ActivityTaskCancelledEvent
                            {
                                SchedulingEventId = task.TaskScheduledEventId
                            };
                            InsertHistory(this, task.Execution, atxe);
                            task.Execution.AwaitingDecision = true;
                            TaskLists.DeleteOnSubmit(task);
                            break;

                        case "heartbeat":
                            if(task != null)
                            {
                                if(taskResponse.Progress.HasValue)
                                    task.Progress = taskResponse.Progress.Value;

                                if(taskResponse.ProgressMessage != null)
                                    task.ProgressMessage = taskResponse.ProgressMessage;

                                if(task.HeartbeatTimeout.HasValue)
                                    task.HeartbeatAlarm = DateTime.UtcNow.AddSeconds(task.HeartbeatTimeout.Value);

                                
                                // Send a progress notification to the updater
                                var data = new JObject(
                                    new JProperty("type", "ActivityTaskHeartbeat"),
                                    new JProperty("progress", task.Progress ?? -1),
                                    new JProperty("message", task.ProgressMessage ?? ""),
                                    new JProperty("progressData", JToken.Parse(task.ProgressData)));

                                var qt = CreateUpdaterNotification(task.Execution, data);
                                TaskLists.InsertOnSubmit(qt);
                                SubmitChanges();
                            }
                            else
                            {
                                log.Info($"Received heartbeat for deleted task with token: {taskToken}, cancelling");
                            }
                            // If the task has been cancelled or deleted then send a cancellation request
                            response = new JObject { new JProperty("cancellationRequested", task == null || task.Cancelling) }.ToString(Formatting.None);
                            break;

                        case "rescheduled":
                            // Rescheduled tasks are left in the tasklist with an updated scheduling time
                            // effectively pushing them to the back of the queue 
                            if (task != null)
                            {
                                task.ScheduledAt = DateTime.UtcNow;
                                task.WorkerId = null;
                            }
                            break;

                        default:
                            log.ErrorFormat("Invalid activity task response status: {0}", taskResponse.Status);
                            break;
                    }

                    // Nothing to update if the task has been deleted
                    if(task == null)
                        return response;

                    try
                    {
                        SubmitChanges();
                        break;
                    }
                    catch(ChangeConflictException)
                    {
                        Refresh(RefreshMode.KeepCurrentValues, task.Execution);
                    }
                }
                if(i == RETRIES)
                {
                    log.Error($"Failed to process {taskResponse.Status} response from task {taskToken}");
                    return null;
                }
                return response;
            }
            catch(Exception ex)
            {
                log.Error("Failed to process activity task response", ex);
                return null;
            }
        }

        public static TaskList CreateUpdaterNotification(Execution execution, JObject data)
        {
            return new TaskList
            {
                ExecutionId = execution.ExecutionId,
                JobId = execution.JobId,
                ListName = UPDATER_TASKLIST,
                TaskToken = Guid.NewGuid(),
                TaskScheduledEventId = 0,
                Priority = NOTIFICATION_PRIORITY,
                HeartbeatTimeout = int.MaxValue,
                TaskAlarm = DateTime.MaxValue,
                ScheduledAt = DateTime.UtcNow,
                TaskSheduleToCloseTimeout = null,
                TaskStartToCloseTimeout = null,
                NotificationData = data.ToString(Formatting.None)
            };
        }
        /// <summary>
        /// Convert a version string to a long integer.
        /// version strings may have up to 4 segments where each segment may
        /// be 0-9999.
        /// missing segements are assumed to be zero (1.0 == 1.0.0.0)
        /// </summary>
        /// <param name="version">version string</param>
        /// <returns>long integer version</returns>
        public static long ConvertVersion(string version)
        {
            var vs = version.Split('.');

            if(vs.Length > 4)
                throw new ApplicationException("Too many version numbers");

            return vs
                .Select(long.Parse)
                .Concat(Enumerable.Repeat(0L, 4 - vs.Length))
                .Aggregate((acc, v) =>
                {
                    if(v > 9999) throw new ApplicationException("version number >= 10000");
                    return acc * 10000 + v;
                });
        }

        /// <summary>
        /// Convert a long integer to a version string.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string ConvertVersion(long version)
        {
            var vv = version;
            var parts = new List<string>(4);
            for(var i = 0; i < 4; i++)
            {
                var v = vv % 10000;
                parts.Add(v.ToString());
                vv /= 10000;
            }
            if(vv != 0)
                throw new ApplicationException("Invalid version number");
            parts.Reverse();
            return string.Join(".", parts);
        }

        /// <summary>
        /// Normalise a version string by converting twice
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string NormaliseVersion(string version)
        {
            return ConvertVersion(ConvertVersion(version));
        }

        /// <summary>
        ///  Cancel an execution by cancelling its running task(s) and deleting the others
        /// </summary>
        /// <param name="executionId"></param>
        public void CancelActivityTasks(Execution execution)
        {
            foreach(var task in TaskLists.Where(t => t.Execution == execution))
            {
                if(task.WorkerId == null)
                    TaskLists.DeleteOnSubmit(task);
                else
                    task.Cancelling = true;
            }
        }

        /// <summary>
        /// Insert an asynchronous signal into an execution's history
        /// </summary>
        /// <param name="executionid"></param>
        /// <param name="signal"></param>
        public void SignalExecution(Guid executionid, WorkflowSignalledEvent signal)
        {
            try
            {
                var execution = Executions.SingleOrDefault(e => e.ExecutionId == executionid);

                if (execution == null)
                {
                    log.Warn($"Signal for unknown execution with id {executionid} ignored");
                    return;
                }
                InsertHistory(this, execution, signal);
                SubmitChanges();
            }
            catch (Exception ex)
            {
                log.Error($"Failed to signal execution with id {executionid}", ex);
            }
        }
    }
}
