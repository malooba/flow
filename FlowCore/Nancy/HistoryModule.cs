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
using System.Collections.Generic;
using System.Linq;
using FlowShared.Db;
using FlowShared.Json;
using Nancy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FlowCore.Nancy
{
    public class HistoryModule : NancyModule
    {
        private const int HISTORY_MAX = 1000;

        /// <summary>
        /// Get execution history
        /// 
        /// NOTE: It is possible, but very unlikely, that event timestamps are not monotonic
        /// This is not an error.  Events are ordered by ID and the timestamps are provided purely for reporting purposes.
        /// Any non-monotonicity should be of the order of a few milliseconds in any case.
        /// 
        /// executionid is a required parameter and must be a GUID.
        /// signal is the name of a signal to search for.  This parameter overrides fromid and for.
        /// fromid and for permit the selection of a restricted range of the unfiltered events 
        /// </summary>
        public HistoryModule()
        {
            Get["/history"] = parameters =>
            {
                Response response;
                var qexecutionId = Request.Query["executionid"];    // The executionId to get history for (required)
                var qfrom = Request.Query["fromid"];                // The Id of the event prior to those returned (use 0 or omit to get from the start)
                var qfor = Request.Query["for"];                    // The number of events to fetch (maximum and default are HISTORY_MAX)
                var signal = (string)Request.Query["signal"];       // Search for a named asynchronous signal in entire history (ignore fromid, for)


                var f = 0;
                var n = HISTORY_MAX;

                if(qfrom != null) f = int.Parse((string)qfrom);
                if(qfor != null)  n = Math.Max(int.Parse((string)qfor), HISTORY_MAX);

                Guid executionId;
                if(!Guid.TryParse(qexecutionId, out executionId))
                {
                    response = new Response().WithStatusCode(HttpStatusCode.BadRequest);
                }
                else
                {
                    using(var db = new Database())
                    {
                        IEnumerable<string> history;
                        if (signal != null)
                        {
                            // Search for WorkflowSignalledEvents with the signalName or
                            // ActivityTaskScheduledEvents with the asynchSignal parameter matching the signalName
                            var fullHistory =
                                from h in db.Histories
                                where h.ExecutionId == executionId &&
                                      ((h.EventType == "WorkflowSignalledEvent") || (h.EventType == "ActivityTaskScheduledEvent"))
                                select h;

                            history =
                                from h in fullHistory.ToList()
                                where ((h.EventType == "WorkflowSignalledEvent" && (string)(JObject.Parse(h.Json).SelectToken("signalName")) == signal) ||
                                       (h.EventType == "ActivityTaskScheduledEvent" && (string)(JObject.Parse(h.Json).SelectToken("asyncSignal")) == signal))
                                select JsonConvert.SerializeObject(new HistoryJson(h));
                        }
                        else
                        {
                            history =
                                (from h in db.Histories
                                 where h.ExecutionId == executionId && h.Id > f
                                 select JsonConvert.SerializeObject(new HistoryJson(h))
                                 ).Take(n);
                        }
                        var responseJson = "[" + string.Join(",", history) + "]";
                        response =
                            ((Response)responseJson)
                            .WithHeader("X-HistoryCount", db.Histories.Count(h => h.ExecutionId == executionId).ToString())
                            .WithContentType("application/json");
                    }
                }
                return response;
            };
        }
    }

    public class HistoryJson
    {
        public HistoryJson(History h)
        {
            EventType = h.EventType;
            ExecutionId = h.ExecutionId;
            Id = h.Id;
            Timestamp = h.Timestamp;
            Attributes = h.Json;
        }

        [JsonProperty(PropertyName = "eventType", Order = 1)]
        public string EventType;

        [JsonProperty(PropertyName = "executionId", Order = 2)]
        public Guid ExecutionId;

        [JsonProperty(PropertyName = "id", Order = 3)]
        public int Id;

        [JsonProperty(PropertyName = "timestamp", Order = 4)]
        public DateTime Timestamp;

        [JsonProperty(PropertyName = "attributes", Order = 5)]
        [JsonConverter(typeof(JsonLiteralTextConverter))]
        public string Attributes;
    }
}
