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

namespace Flow.Workers.DelayWorker
{
    class Program
    {
        static void Main()
        {
            var worker = new DelayWorker();
            worker.Main();
        }
    }

    class DelayWorker : WorkerBase.WorkerBase
    {
       // The base DoTask implements a default 50 Second delay with a 5 Second heartbeat and cancellation
       // That is exactly what we want :) 
    }
}
