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
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace FlowCore
{
    class PeriodicTaskScheduler
    {
        private readonly List<PeriodicTask> ptasks;
        private const int LOOP_INTERVAL = 10; // seconds
        private readonly ILog log;
        private CancellationToken token;

        public PeriodicTaskScheduler(CancellationToken token)
        {
            this.token = token;
            log = LogManager.GetLogger(typeof(PeriodicTaskScheduler));
            ptasks = new List<PeriodicTask>();
            Task.Run((Action)SchedulerLoop, token);
        }

        private void SchedulerLoop()
        {
            while(true)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    lock(ptasks)
                    {
                        foreach(var pt in ptasks.Where(p => !p.Running && p.Due < now))
                        {
                            pt.Running = true;
                            Task.Factory.StartNew(pt.RunTask, token, token);
                            if(token.IsCancellationRequested)
                                goto die;
                        }
                    }
                }
                catch(Exception ex)
                {
                    log.Error("Exception thrown from Scheduler loop", ex);
                    ptasks.Clear();
                }
                Thread.Sleep(LOOP_INTERVAL * 1000);
            }
            die:
            ;
        }

        public void AddTask(PeriodicTask task)
        {
            lock(ptasks)
                ptasks.Add(task);
        }

    }
}
