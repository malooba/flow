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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flow.Workers.WorkerBase
{
    internal class WorkflowClient : IWorkflowClient
    {
        private ILog logger;
        private HttpClient client;

        public WorkflowClient(string baseUrl)
        {
            logger = LogManager.GetLogger(this.GetType());
            client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public ActivityTask Poll(string taskList, string workerId, CancellationTokenSource cts)
        {
            var url = "/tasks/poll?list=" + taskList + "&worker=" + workerId;
            var response = client.GetAsync(url, cts.Token).Result;
            switch(response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var json = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ActivityTask>(json);

                case HttpStatusCode.NoContent:
                    logger.Info("Nothing to do");
                    return null;

                default:
                    logger.WarnFormat("HTTP Status code = {0} ({1})", response.ReasonPhrase, (int)response.StatusCode);
                    throw new ApplicationException($"Bad HTTP status on Poll {response.StatusCode}");
            }  
        }

        public JObject Heartbeat(ActivityTask task, int? progress = null, string message = null)
        {
            JToken progressData;
            task.Input.TryGetValue("progressData", out progressData);
            var heartbeatRequest = new
            {
                status = "heartbeat",
                progress,
                message,
                progressData
            };
            var json = JsonConvert.SerializeObject(heartbeatRequest, Formatting.None);
            var content = new StringContent(json, Encoding.UTF8);
            var response = client.PostAsync("tasks/" + task.TaskToken, content).Result;
            if(response.StatusCode != HttpStatusCode.OK)
            {
                logger.WarnFormat("HTTP Status code = {0} ({1})", Enum.GetName(typeof(HttpStatusCode), response.StatusCode), (int)response.StatusCode);
                return null;
            }

            json = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(json);
        }

        public void Respond(ActivityTask task, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            logger.Info($"Responding with content - {json}");
            var response = client.PostAsync("/tasks/" + task.TaskToken, content).Result;
            if(response.StatusCode != HttpStatusCode.OK)
                logger.WarnFormat("HTTP Status code = {0} ({1})", Enum.GetName(typeof(HttpStatusCode), response.StatusCode), (int)response.StatusCode);
        }

        public void SendSignal(ActivityTask task, WorkflowSignalledEvent signal)
        {
            var signalJson = JsonConvert.SerializeObject(signal);
            var content = new StringContent(signalJson, Encoding.UTF8, "application/json");
            var url = $"/executions/{task.ExecutionId}/signal";
            client.PostAsync(url, content);
        }

        public JArray PollSignal(Guid executionId, string signalName)
        {
            var url = $"/history?executionid={executionId}&signal={signalName}";
            var response = client.GetAsync(url).Result;
            if(response.StatusCode == HttpStatusCode.OK)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                return JArray.Parse(json);
            }
            throw new ApplicationException($"PollSignal returned status = {response.StatusCode}");
        }
    }
}
