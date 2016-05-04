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
using Newtonsoft.Json.Linq;

namespace Flow.Workers.GenericUpdaterWorker
{
    class GenericUpdaterWorker : WorkerBase.WorkerBase
    {
        protected override void DoTask(WorkerBase.ActivityTask task)
        {
            string message;
            string destination;

            switch(task.ActivityName)
            {
                case "jobComplete":
                    destination = GetInput<string>(task, "destination");
                    Console.WriteLine($"Job {task.JobId} completed with destination = {destination}");
                    RespondSuccess(task);
                    break;

                case "jobFailed":
                    message = GetInput<string>(task, "message") ?? "";
                    Console.WriteLine($"Job {task.JobId} failed with message {message}");
                    RespondSuccess(task);
                    break;

                case "jobCancelled":
                    Console.WriteLine($"Job {task.JobId} cancelled");
                    RespondSuccess(task);
                    break;

                default:
                    RespondFailure(task, $"Unknown activity name - {task.ActivityName}");
                    break;
            }
        }

        protected override void DoNotification(WorkerBase.ActivityTask task)
        {
            var notificationType = GetInput<string>(task, "type");
            string reason;
            int progress;
            JObject progressData;

            switch(notificationType)
            {
                case "ActivityTaskHeartbeat":
                    progress = GetInput<int>(task, "progress");
                    progressData = GetInput<JObject>(task, "progressData", null);
                    var overallProgress = -1;
                    if(progressData != null)
                    {
                        var stage = (int)progressData.GetValue("stage");
                        var stages = (int)progressData.GetValue("stages");
                        overallProgress = ((stage - 1) * 100 + progress) / stages;
                    }
                    Console.WriteLine($"Job {task.JobId} progress = {progress} overall progress = {overallProgress}");
                    break;

                case "WorkflowExecutionCancelledEvent":
                    reason = GetInput<string>(task, "reason") ?? "";
                    Console.WriteLine($"Job {task.JobId} errored with reason {reason}");
                    break;

                case "WorkflowExecutionFailedEvent":
                    reason = GetInput<string>(task, "reason") ?? "";
                    Console.WriteLine($"Job {task.JobId} errored with reason {reason}");
                    break;

                default:
                    RespondFailure(task, $"Unknown notification type - {notificationType}");
                    break;
            }

        }
    }
}
