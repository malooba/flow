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
using log4net;

namespace FlowCore
{
    /// <summary>
    /// Periodic task base class
    /// Runs the action at intervals defined in seconds
    /// </summary>
    public class PeriodicTask
    {
        private readonly string name;
        private readonly Action<CancellationToken> action;
        private readonly int periodInSeconds;
        protected static ILog log;

        internal DateTime Due;
        internal bool Running;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of task (and logger)</param>
        /// <param name="action">Action to be performed periodically</param>
        /// <param name="periodInSeconds">Task period</param>
        /// <param name="runNow">True if task is to be executed immediately</param>
        protected PeriodicTask(string name, Action<CancellationToken> action, int periodInSeconds, bool runNow = false)
        {
            this.name = name;
            this.action = action;
            this.periodInSeconds = periodInSeconds;
            Due = runNow ? DateTime.UtcNow : DateTime.UtcNow.AddSeconds(periodInSeconds);
            log = LogManager.GetLogger(name);
        }

        /// <summary>
        /// Execute the action and reschedule
        /// </summary>
        internal void RunTask(object ct)
        {
            if(!(ct is CancellationToken))
                throw new ArgumentException("Invalid argument to RunTask", nameof(ct));
            var token = (CancellationToken)ct;
            try
            {
                action.Invoke(token);
            }
            catch(Exception ex)
            {
                log.Error($"Caught exception in periodic task {name}: {ex.Message}", ex);
                Console.WriteLine($"Caught exception in periodic task {name}: {ex.Message}");
            }
            if(token.IsCancellationRequested)
                return;
            // Reschedule task skipping any missed schedule(s)
            while(Due < DateTime.UtcNow)
                Due = Due.AddSeconds(periodInSeconds);
            Running = false;
        }
    }
}