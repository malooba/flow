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

using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace Flow.SvcInstaller
{
    [RunInstaller(true)]
    public class WindowsServiceInstaller : Installer
    {
        /// <summary>
        /// Public Constructor for WindowsServiceInstaller.
        /// - Put all of your Initialization code here.
        /// </summary>
        public WindowsServiceInstaller()
        {
            var serviceProcessInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            // Need to explicitly fetch the configuration because this class is not conventionally executed
            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetAssembly(typeof(WindowsServiceInstaller)).Location);
            var serviceName = config.AppSettings.Settings["ServiceName"].Value;
            var serviceDisplayName = config.AppSettings.Settings["ServiceDisplayName"].Value;
            var user = config.AppSettings.Settings["User"].Value;
            var password = config.AppSettings.Settings["Password"].Value;

            //# Service Account Information
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;
            switch(user)
            {
                case "LocalService":
                    serviceProcessInstaller.Account = ServiceAccount.LocalService;
                    break;

                case "LocalSystem":
                    serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
                    break;

                case "NetworkService":
                    serviceProcessInstaller.Account = ServiceAccount.NetworkService;
                    break;

                default:
                    serviceProcessInstaller.Username = user;
                    serviceProcessInstaller.Password = password;
                    break;
            }

            //# Service Information
            
            serviceInstaller.DisplayName = serviceDisplayName;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = serviceName;

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}