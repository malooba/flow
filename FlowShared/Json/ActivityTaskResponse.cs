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

namespace FlowShared.Json
{
    [JsonObject]
    public class ActivityTaskResponse
    {
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

        // Optionally set when status == "heartbeat"
        [JsonProperty(PropertyName = "progress")]
        public int? Progress { get; set; }

        // Optionally set when status == "heartbeat"
        [JsonProperty(PropertyName = "progressMessage")]
        public string ProgressMessage { get; set; }

        // Optionally set when status == "heartbeat"
        [JsonProperty(PropertyName = "jobId")]
        public JToken JobId { get; set; }
    }
}
