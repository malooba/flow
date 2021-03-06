﻿//Copyright 2016 Malooba Ltd

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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowShared.Json
{
    /// <summary>
    /// The structure sent to a worker to start a task
    /// </summary>
    public class ActivityTask
    {
        [JsonProperty(PropertyName = "activityId")]
        public string ActivityId { get; set; }

        [JsonProperty(PropertyName = "activityName")]
        public string ActivityName { get; set; }

        [JsonProperty(PropertyName = "activityVersion")]
        public string ActivityVersion { get; set; }

        [JsonProperty(PropertyName = "asyncSignal")]
        public string AsyncSignal { get; set; }

        [JsonProperty(PropertyName = "input")]
        public JObject Input { get; set; }

        [JsonProperty(PropertyName = "startedEventId")]
        public long StartedEventId { get; set; }

        [JsonProperty(PropertyName = "taskToken")]
        public Guid TaskToken { get; set; }

        [JsonProperty(PropertyName = "executionId")]
        public Guid ExecutionId { get; set; }

        [JsonProperty(PropertyName = "jobId")]
        public string JobId { get; set; }
    }
}
