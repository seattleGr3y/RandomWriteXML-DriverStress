using Microsoft.HWSW.Test.Utilities;
using System;
using System.Management;
using System.ServiceProcess;

namespace DriverCapsuleStressTool
{
    class StartStopServices
    {
        /// <summary>
        /// Stop a service if it is currently running
        /// StartStopServices.StopService(serviceName);
        /// </summary>
        /// <param name="serviceName"></param>
        internal static void StopService(string serviceName)
        {
            // is the WTT service started.
            string startMode = "Disabled";
            ServiceController sc = new ServiceController
            {
                ServiceName = serviceName
            };
            Console.WriteLine("The " + serviceName + " service status is currently set to {0}",
                                   sc.Status.ToString());
            Logger.Comment("The " + serviceName + " service status is currently set to {0}",
                               sc.Status.ToString());

            if (!sc.StartType.Equals(ServiceStartMode.Disabled))
                ServiceStartModeUpdate(serviceName, startMode);

            if (sc.Status == ServiceControllerStatus.Running)
            {
                // Stop it if it is started.
                Console.WriteLine("Stopping the " + serviceName + " service...");
                Logger.Comment("Stopping the " + serviceName + " service...");
                try
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);

                    Console.WriteLine("The " + serviceName + " service status is now set to {0}.",
                                       sc.Status.ToString());
                    Logger.Comment("The " + serviceName + " service status is now set to {0}.",
                                       sc.Status.ToString());
                }
                catch (Exception ex)
                {
                    GetData.GetExceptionMessage(ex);
                }
            }
        }

        /// <summary>
        /// Start a service if it is currently stopped
        /// StartStopServices.StartService(serviceName);
        /// </summary>
        /// <param name="serviceName"></param>
        internal static void StartService(string serviceName)
        {
            // is the service stopped.
            string startMode = "Automatic";
            ServiceController sc2 = new ServiceController
            {
                ServiceName = serviceName
            };
            Console.WriteLine("The " + serviceName + " service status is currently set to {0}",
                               sc2.Status.ToString());
            Logger.Comment("The " + serviceName + " service status is currently set to {0}",
                               sc2.Status.ToString());

            if (!sc2.StartType.Equals(startMode))
                ServiceStartModeUpdate(serviceName, startMode);

            if (sc2.Status == ServiceControllerStatus.Stopped)
            {
                // Start it if it is stopped.
                Console.WriteLine("Stopping the " + serviceName + " service...");
                Logger.Comment("Stopping the " + serviceName + " service...");
                try
                {
                    sc2.Start();
                    sc2.WaitForStatus(ServiceControllerStatus.Running);

                    Console.WriteLine("The " + serviceName + " service status is now set to {0}.",
                                       sc2.Status.ToString());
                    Logger.Comment("The " + serviceName + " service status is now set to {0}.",
                                       sc2.Status.ToString());
                }
                catch (Exception ex)
                {
                    GetData.GetExceptionMessage(ex);
                }
            }
        }

        /// <summary>
        /// set the start mode for a service
        /// StartStopServices.ServiceStartModeUpdate(serviceName, startMode);
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="startMode"></param>
        /// <returns></returns>
        internal static bool ServiceStartModeUpdate(string serviceName, string startMode)
        {
            uint successReturn = 1;
            string filterService = String.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", serviceName);
            ManagementObjectSearcher querySearch = new ManagementObjectSearcher(filterService);
            
            if (querySearch == null)
            {
                return false;
            }
            else
            {
                try
                {
                    ManagementObjectCollection services = querySearch.Get();//get that service
                    foreach (ManagementObject service in services)
                    {
                        if (Convert.ToString(service.GetPropertyValue("StartMode")) != startMode)//if startup type is Diasabled then change it
                        {
                            ManagementBaseObject inParams = service.GetMethodParameters("ChangeStartMode");
                            inParams["startmode"] = startMode;
                            ManagementBaseObject outParams =
                            service.InvokeMethod("ChangeStartMode", inParams, null);
                            successReturn = Convert.ToUInt16(outParams.Properties["ReturnValue"].Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GetData.GetExceptionMessage(ex);
                }
            }
            return (successReturn == 0);
        }
    }
}
