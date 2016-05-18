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
    public class ActivityTaskScheduledEvent: EventConverter<ActivityTaskScheduledEvent>
    {
        /// <summary>
        /// Unique ID of activity task
        /// </summary>
        [JsonProperty(PropertyName = "activityId")]
        public string ActivityId;
        [JsonProperty(PropertyName = "activityName")]
        public string ActivityName;
        [JsonProperty(PropertyName = "activityVersion")]
        public string ActivityVersion;
        [JsonProperty(PropertyName = "taskId")]
        public string TaskId;
        [JsonProperty(PropertyName = "asyncSignal")]
        public string AsyncSignal;
        [JsonProperty(PropertyName = "input")]
        public JObject Input;
        [JsonProperty(PropertyName = "taskList")]
        public string TaskList;
        [JsonProperty(PropertyName = "taskPriority")]
        public int TaskPriority;
        [JsonProperty(PropertyName = "heartbeatTimeout")]
        public int? HeartbeatTimeout;
        [JsonProperty(PropertyName = "scheduleToCloseTimeout")]
        public int? ScheduleToCloseTimeout;
        [JsonProperty(PropertyName = "scheduleToStartTimeout")]
        public int? ScheduleToStartTimeout;
        [JsonProperty(PropertyName = "startToCloseTimeout")]
        public int? StartToCloseTimeout;
    }
}
