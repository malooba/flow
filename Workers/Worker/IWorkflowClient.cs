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
using Newtonsoft.Json.Linq;

namespace Flow.Workers.WorkerBase
{
    /// <summary>
    /// Client used by worker processes to communicate with the workflow engine over the REST interface
    /// </summary>
    public interface IWorkflowClient
    {
        /// <summary>
        /// Poll the workflow engine for a task
        /// </summary>
        /// <returns></returns>
        ActivityTask Poll(string taskList, string workerId, CancellationTokenSource cts);

        /// <summary>
        /// Send task heartbeat and progress percentage
        /// If there is a failure then log it and return null
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="progress">Optional progress percentage</param>
        /// <param name="message">Optional progress message</param>
        /// <returns>ActivityTaskStatus with CancelRequested flag</returns>
        JObject Heartbeat(ActivityTask task, int? progress = null, string message = null);

        /// <summary>
        /// Return a response from an activity
        /// </summary>
        /// <param name="task"></param>
        /// <param name="json"></param>
        void Respond(ActivityTask task, string json);

        /// <summary>
        /// Send an asynchronous signal to a workflow execution
        /// </summary>
        /// <param name="task"></param>
        /// <param name="signal"></param>
        /// <returns></returns>
        void SendSignal(ActivityTask task, WorkflowSignalledEvent signal);

        /// <summary>
        /// Poll for signals on an execution
        /// </summary>
        /// <param name="executionId"></param>
        /// <param name="signalName"></param>
        /// <returns>Array of signals that match</returns>
        JArray PollSignal(Guid executionId, string signalName);
    }
}