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
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.Workers.WorkerBase
{
    public class WorkerBase : ServiceBase
    {
        protected readonly ILog logger;

        /// <summary>
        /// Polling interval in milliseconds
        /// </summary>
        protected int pollInterval = 5000;

        /// <summary>
        /// Retry interval in milliseconds
        /// Retry is probably because HTTP server is not running
        /// </summary>
        protected int retryInterval = 5000;

        protected readonly IWorkflowClient workflowClient;

        protected readonly string taskList;
        protected readonly string workerId;

        private CancellationTokenSource cts;
        private ManualResetEvent exit;

        protected WorkerBase(IWorkflowClient workflowClient = null)
        {
            // This will log under the derived class name
            logger = LogManager.GetLogger(GetType());

            // If running as a console application then log to the console
            if(Environment.UserInteractive)
            {
                var layout = new PatternLayout("%utcdate %-5level - %message%newline");
                layout.ActivateOptions();
                var consoleAppender = new ConsoleAppender
                {
                    Layout = layout
                };
                consoleAppender.ActivateOptions();
                BasicConfigurator.Configure(consoleAppender);
                ((log4net.Repository.Hierarchy.Logger)(logger.Logger)).AddAppender(consoleAppender);
            }

            var appSettings = ConfigurationManager.AppSettings;
            ServiceName = appSettings["serviceName"];
            taskList = taskList ?? appSettings["TaskList"];
            workerId = appSettings["WorkerId"];
            var baseUrl = appSettings["BaseUrl"];
            logger.Info($"QueueName = {taskList}");
            logger.InfoFormat($"BaseUrl = {baseUrl}");

            this.workflowClient = workflowClient ?? new WorkflowClient(baseUrl);
        }

        public void Main(params string[] args)
        {
            // Create the token source.
            cts = new CancellationTokenSource();
            if(Environment.UserInteractive)
            {
                OnStart(args);
                Console.CancelKeyPress += (sender, eventArgs) => OnStop();
                exit = new ManualResetEvent(false);
                exit.WaitOne();
            }
            else
            {
                Run(this);
            }
        }

        protected override void OnStart(string[] args)
        {
            Task.Run((Action)PollTask);
        }

        protected override void OnStop()
        {
            cts.Cancel();
        }

        /// <summary>
        /// (Semi)Infinite loop to poll for work
        /// </summary>
        private async void PollTask()
        {
            logger.Info("Service starting");
            while(true)
            {
                if(cts.IsCancellationRequested)
                    break;
                try
                {

                    while(true)
                    {
                        var task = workflowClient.Poll(taskList, workerId, cts);
                        if(task != null)
                        {
                            if(task.ActivityName == "$notify")
                                DoNotification(task);
                            else
                                DoTask(task);
                        }
                        else
                            await Task.Delay(pollInterval);
                        //Thread.Sleep(pollInterval);
                    }
                }
                catch(Exception ex)
                {
                    logger.Error($"Error polling for work: {ex.Message} {ex.StackTrace}");
                    Thread.Sleep(retryInterval);
                }
            }
            logger.Info("Service shutting down");
            exit?.Set();
        }

        /// <summary>
        /// Override this to do real work
        /// </summary>
        /// <param name="task">Task</param>
        protected virtual void DoTask(ActivityTask task)
        {
            Task.Run(() =>
            {
                // Sample code for simple delaying activity with heartbeat and cancellation
                Console.WriteLine($"Starting activity, id = {task.ActivityId}");
                var delay = (int?)GetInput(task, "delay") ?? 50000;
                for(var progress = 0; progress < 100; progress += 10)
                {
                    Thread.Sleep(delay / 10);
                    var hb = workflowClient.Heartbeat(task, progress);
                    if(hb == null)
                    {
                        RespondFailure(task, "Heartbeat failed");
                        return;
                    }
                    if((bool?)hb.SelectToken("cancellationRequested") == true)
                    {
                        RespondCancelled(task);
                        return;
                    }
                }
                var result = new JObject {["output"] = 42};
                RespondSuccess(task, result);
            });
        }

        /// <summary>
        /// Override this to do real work
        /// </summary>
        /// <param name="task">Task</param>
        protected virtual void DoNotification(ActivityTask task)
        {
            // The dafault behaviour for notifications is to log and ignore them
            var type = GetInput(task, "type", "(Unknown type)");
            Console.WriteLine($"Notification, type = {type}");
        }

        /// <summary>
        /// Fetch the value of a single task input variable
        /// returns null if not present
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="name">Input name</param>
        /// <returns></returns>
        protected JToken GetInput(ActivityTask task, string name)
        {
            JToken input;
            task.Input.TryGetValue(name, out input);
            return input;
        }

        /// <summary>
        /// Attempt to extract the required type of value from a variable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected T GetInput<T>(ActivityTask task, string name)
        {
            var value = GetInput(task, name);
            if(value == null)
                throw new ApplicationException($"Missing required value of input parameter: {name}");
            return value.ToObject<T>();
        }

        /// <summary>
        /// Attempt to extract the required type of value from a variable with a default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected T GetInput<T>(ActivityTask task, string name, T defaultValue)
        {
            var value = GetInput(task, name);
            if(value == null)
                return defaultValue;
            return value.ToObject<T>();
        }

        /// <summary>
        /// Complete task sucessfully
        /// Use the reponse object to create the outputs
        /// The response object will usually be an anonymous object
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="result">Result object</param>
        protected void RespondSuccess(ActivityTask task, object result)
        {
            string json;
            try
            {
                json = JsonConvert.SerializeObject(result, Formatting.None);
            }
            catch(Exception)
            {
                RespondFailure(task, "Invalid JSON response object");
                return;
            }
            RespondSuccess(task, json);
        }

        /// <summary>
        /// Complete task sucessfully with no result
        /// </summary>
        /// <param name="task"></param>
        protected void RespondSuccess(ActivityTask task)
        {
            var successObj = new JObject();
            RespondSuccess(task, successObj);
        }

        /// <summary>
        /// Complete task sucessfully
        /// Use the supplied text as the result
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="jsonObjectResult">Result text</param>
        protected void RespondSuccess(ActivityTask task, string jsonObjectResult)
        {
            RespondSuccess(task, JObject.Parse(jsonObjectResult));
        }

        /// <summary>
        /// Complete task sucessfully
        /// Use the JObject to create the task outputs
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="result">Result JSON object</param>
        protected void RespondSuccess(ActivityTask task, JObject result)
        {
            var successObj = new JObject(new JProperty("status", "success"),
                new JProperty("result", result));
            workflowClient.Respond(task, JsonConvert.SerializeObject(successObj, Formatting.None));
        }

        /// <summary>
        /// Complete task with failure
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="reason">Reason string</param>
        /// <param name="details">JSON details object</param>
        protected void RespondFailure(ActivityTask task, string reason, JToken details = null)
        {
            var failObj = new {status = "failure", reason, details};
            workflowClient.Respond(task, JsonConvert.SerializeObject(failObj, Formatting.None));
        }

        /// <summary>
        /// Complete task with cancellation
        /// </summary>
        /// <param name="task">Task</param>
        protected void RespondCancelled(ActivityTask task)
        {
            var cancelObj = new {status = "cancelled"};
            workflowClient.Respond(task, JsonConvert.SerializeObject(cancelObj, Formatting.None));
        }

        /// <summary>
        /// Reschedule task
        /// </summary>
        /// <param name="task">Task</param>
        [Obsolete("Use asynchronous workers instead of resheduling")]
        protected void RespondRescheduled(ActivityTask task)
        {
            var cancelObj = new {status = "rescheduled"};
            workflowClient.Respond(task, JsonConvert.SerializeObject(cancelObj, Formatting.None));
        }

        /// <summary>
        /// Signal for response from an asynchronous task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="signalName"></param>
        /// <param name="result"></param>
        protected void SignalSuccess(ActivityTask task, string signalName, JObject result)
        {
            var signal = new WorkflowSignalledEvent
            {
                SignalName = signalName,
                Status = "success",
                Result = result
            };
            workflowClient.SendSignal(task, signal);
        }

        /// <summary>
        /// Signal for response from an asynchronous task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="signalName"></param>
        /// <param name="reason"></param>
        /// <param name="details"></param>
        protected void SignalFailure(ActivityTask task, string signalName, string reason, JObject details = null)
        {
            var signal = new WorkflowSignalledEvent
            {
                SignalName = signalName,
                Status = "failure",
                Reason = reason,
                Details = details
            };
            workflowClient.SendSignal(task, signal);
        }

        /// <summary>
        /// Signal for response from an asynchronous task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="signalName"></param>
        protected void SignalCancelled(ActivityTask task, string signalName)
        {
            var signal = new WorkflowSignalledEvent
            {
                SignalName = signalName,
                Status = "cancelled"
            };
            workflowClient.SendSignal(task, signal);
        }

        /// <summary>
        /// Responds with success synchronously or asynchronously as appropriate
        /// </summary>
        /// <param name="task"></param>
        /// <param name="result"></param>
        protected void Success(ActivityTask task, JObject result)
        {
            var signalName = GetInput<string>(task, "signalName");
            if(string.IsNullOrWhiteSpace(signalName))
                RespondSuccess(task, result);
            else
                SignalSuccess(task, signalName, result);
        }

        /// <summary>
        /// Responds with failure synchronously or asynchronously as appropriate
        /// </summary>
        /// <param name="task"></param>
        /// <param name="reason"></param>
        /// <param name="details"></param>
        protected void Failure(ActivityTask task, string reason, JObject details = null)
        {
            var signalName = GetInput<string>(task, "signalName");
            if(string.IsNullOrWhiteSpace(signalName))
                RespondFailure(task, reason, details);
            else
                SignalFailure(task, signalName, reason, details);
        }

        /// <summary>
        /// Responds with cancelled synchronously or asynchronously as appropriate
        /// </summary>
        /// <param name="task"></param>
        protected void Cancelled(ActivityTask task)
        {
            var signalName = GetInput<string>(task, "signalName");
            if(string.IsNullOrWhiteSpace(signalName))
                RespondCancelled(task);
            else
                SignalCancelled(task, signalName);
        }
    }
}
