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

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowShared.Json
{
    [JsonObject]
    public class WorkflowExecutionStartedEvent : EventConverter<WorkflowExecutionStartedEvent>
    {
        [JsonProperty(PropertyName = "tagList")]
        public List<string> TagList = new List<string>();

        [JsonProperty(PropertyName = "workflowName")]
        public string WorkflowName;

        [JsonProperty(PropertyName = "workflowVersion")]
        public string WorkflowVersion;

        [JsonProperty(PropertyName = "input")]
        public JObject Input;

        [JsonProperty(PropertyName = "decisionList")]
        public string DecisionList;

        [JsonProperty(PropertyName = "taskPriority")]
        public uint? TaskPriority;

        [JsonProperty(PropertyName = "executionStartToCloseTimeout")]
        public uint? ExecutionStartToCloseTimeout;

        [JsonProperty(PropertyName = "taskStartToCloseTimeout")]
        public uint? TaskStartToCloseTimeout;
    }
}
