using Microsoft.HWSW.Test.Utilities;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;

namespace RandomWriteXML
{
    /// <summary>
    /// create regkey to enable the stress test to continue running after a reboot
    /// </summary>
    class RebootAndContinue
    {
        #region SETTING REGISTRY KEY TO RESTART STRESS POST REBOOT
        internal static void SetStartUpRegistry(string stressAppPath)
        {
            try
            {
                Logger.FunctionEnter();
                Logger.Comment("trying to set reboot regKey...");
                var rs = new RegistrySecurity();
                string user = Environment.UserName;
                rs.AddAccessRule(new RegistryAccessRule(user,
                                                        RegistryRights.WriteKey | RegistryRights.SetValue | RegistryRights.Delete,
                                                        InheritanceFlags.ContainerInherit,
                                                        PropagationFlags.None,
                                                        AccessControlType.Allow));

                RegistryKey regkey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", true);
                regkey.SetAccessControl(rs);
                regkey.SetValue("DriverStressRebootKey", stressAppPath, RegistryValueKind.String);
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
        }
        #endregion

        internal static void RebootCmd(bool rebootCmd)
        {
            try
            {
                Logger.FunctionEnter();
                Logger.Comment("re-add the reg key to start post reboot...");;
                SetStartUpRegistry(Program.stressAppPath);
                Logger.Comment("I should reboot next...");
                Thread.Sleep(3000);
                Logger.FunctionLeave();
                StartShutDown("-f -r -t 5");
                // Utilities.Reboot(true);
            }
            catch (Exception)
            {
                Logger.Comment("re-add the reg key to start post reboot...");
                //SetStartUpRegistry(Program.stressAppPath);
                Thread.Sleep(3000);
                StartShutDown("-f -r -t 5");
            }
        }

        private static void StartShutDown(string param)
        {
            Logger.FunctionEnter();
            ProcessStartInfo proc = new ProcessStartInfo
            {
                FileName = "cmd",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = "/C shutdown " + param
            };
            Logger.FunctionLeave();
            Process.Start(proc);
        }
        /// <summary>
        ///    Enable debug mode for dpinst to attempt to get more verbose output from dpinst as 
        ///    the default gives very little and there are not any built in options for better output
        /// </summary>
        internal static void EnableWinDebugMode()
        {
            Logger.FunctionEnter();
            Logger.Comment("First we will enable kernel debug mode which will require a reboot...");
            Utilities.RunCommand("bcdedit", "/debug on", true);
            File.Create(Program.dirName + @"debugEnabled.txt");
            Logger.Comment("re-add the reg key to start post reboot...");
            //string stressAppPath = Program.dirName + @"\DriverStress-2.exe";
            //SetStartUpRegistry(stressAppPath);
            Logger.FunctionLeave();
            Thread.Sleep(3000);
            RebootCmd(true);
        }
    }
}


