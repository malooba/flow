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
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using Nancy.Hosting.Self;

namespace FlowCore
{
    internal sealed class FlowCore : ServiceBase
    {
        private NancyHost host;
        private readonly CancellationTokenSource cts;
        private static string baseUrl;

        public FlowCore()
        {
            var appSettings = ConfigurationManager.AppSettings;
            baseUrl = appSettings["BaseUrl"];


            // Create the token source.
            cts = new CancellationTokenSource();
            if(Environment.UserInteractive)
            {
                var exit = new ManualResetEvent(false);
                OnStart(null);
                Console.CancelKeyPress += delegate {
                    OnStop();
                    exit.Set();
                };
                exit.WaitOne();
            }
            else
            {
                Run(this);
            }
        }

        protected override void OnStart(string[] args)
        {
            var pts = new PeriodicTaskScheduler(cts.Token);
            // Create periodic timeout/cleanup tasks
            pts.AddTask(new ActivityTaskTimeoutChecker());
            pts.AddTask(new DeciderTimeoutChecker());
            pts.AddTask(new ExecutionDeleter());

            var config = new HostConfiguration
            {
                // Allow Nancy to automatically reserve URL endpoints from the OS
                UrlReservations = new UrlReservations
                {
                    CreateAutomatically = true
                }
            };

            host = new NancyHost(config, new Uri(baseUrl));
            // Start the fans, please!
            host.Start();
        }

        protected override void OnStop()
        {
            cts.Cancel();
            host.Stop();
            host.Dispose();
        }
    }
}
