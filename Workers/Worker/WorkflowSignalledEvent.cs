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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.Workers.WorkerBase
{
    /// <summary>
    /// When a signal is added to the execution history there is no immediate effect
    /// The signal must be explicitly waited for in the workflow to take effect.
    /// If the signal is received by a matching wait then the wait task will terminate with the appropriate event type 
    /// determined by the signal status e.g. status == "success" will post an activity task completed event.
    /// </summary>
    [JsonObject]
    public class WorkflowSignalledEvent 
    {
        [JsonProperty(PropertyName = "signalName")]
        public string SignalName;

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        // Set when status == "success"
        [JsonProperty(PropertyName = "result")]
        public JObject Result { get; set; }

        // Set when status == "failure"
        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; set; }

        // Optionally set when status == "failure"
        [JsonProperty(PropertyName = "details")]
        public JObject Details { get; set; }
    }
}
