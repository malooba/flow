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
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Workers.WaitWorker
{
    public class WaitWorker : WorkerBase.WorkerBase
    {
        private static int defaultPollRate = 10;

        protected override void DoTask(WorkerBase.ActivityTask task)
        {
            if(task.ActivityName == "wait")
            {
                var signalName = (string)task.Input.SelectToken("signalName");
                var pollRate = (int?)task.Input.SelectToken("pollRate") ?? defaultPollRate;

                if(string.IsNullOrWhiteSpace(signalName))
                    RespondFailure(task, "Missing required signal name");
                else
                    Task.Run(() => WaitForSignal(task, signalName, pollRate));
            }
        }

        private void WaitForSignal(WorkerBase.ActivityTask task, string signalName, int pollRate)
        {
            try
            {
                while(true)
                {
                    var signals = workflowClient.PollSignal(task.ExecutionId, signalName);
                    if(signals.Count == 0)
                    {
                        Thread.Sleep(pollRate * 1000);
                        continue;
                    }

                    if(signals.Count > 1)
                        logger.Warn("Wait task found more than one matching signal, ignoring all but the first");
                    var evt = signals[0]["attributes"];

                    switch((string)evt["status"])
                    {
                        case "success":
                            RespondSuccess(task, evt["result"]);
                            return;

                        case "failure":
                            RespondFailure(task, (string)evt["reason"], evt["details"]);
                            return;

                        case "cancelled":
                            RespondCancelled(task);
                            return;

                        default:
                            RespondFailure(task, $"Invalid signal status - {evt["status"]}", evt);
                            return;
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error($"Poll error - {ex.Message}  stack = {ex.StackTrace}");
                RespondFailure(task, $"Exception caught - {ex.Message}", ex.StackTrace);
            }
        }
    }
}
