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
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ActivityTask = Flow.Workers.WorkerBase.ActivityTask;

namespace Flow.Workers.AsynchWorker
{
    /// <summary>
    /// A dummy asynchronous activity that waits for a fiven number of milliseconds
    /// The asynchronous twin of the delay worker
    /// </summary>
    public class AsynchWorker : WorkerBase.WorkerBase
    {
        protected override void DoTask(ActivityTask task)
        {
            if (task.ActivityName == "asynch")
            {
                foreach (var prop in task.Input.Properties())
                {
                    logger.Info($"{prop.Name} = {prop.Value}");
                }
                
                int delay;
                int.TryParse((string)task.Input.SelectToken("delay"), out delay);
                var signalName = (string)task.Input.SelectToken("signalName");

                var url = $"/executions/{task.ExecutionId}/signal";

                Task.Run(async ()  =>
                {
                    try
                    {
                        logger.Info($"Pre-delay at {DateTime.Now} for {delay}mS");
                        await Task.Delay(delay);
                        logger.Info($"Post-delay at {DateTime.Now}");

                        SignalSuccess(task, signalName, new JObject(new JProperty("output", "value")));
                    }

                    catch(Exception ex)
                    {
                        logger.ErrorFormat("Poll error - {0}  stack = {1}", ex.Message, ex.StackTrace);
                    }
                });
                logger.Info("Returning success");
                RespondSuccess(task);
            }
            else
            {
                RespondFailure(task, $"No such activity name - {task.ActivityName}");
            }
        }
    }
}
