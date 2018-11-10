using Microsoft.HWSW.Test.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace RandomWriteXML
{
    class RegCheck
    {
        /// <summary>
        /// setting the 'policy' to 1 for the firmware allowing it to 'rollback'
        /// </summary>
        /// <param name="hardwareID"></param>
        /// <param name="rebootRequired"></param>
        /// <returns></returns>
        internal static bool CreatePolicyRegKeyAndSetValue(string hardwareID, bool rebootRequired)
        {
            try
            {
                Logger.FunctionEnter();
                string regKeyPath = @"SYSTEM\CurrentControlSet\Control\FirmwareResources\" + hardwareID;
                var rs = new RegistrySecurity();
                string user = Environment.UserName;
                rs.AddAccessRule(new RegistryAccessRule(user,
                                                    RegistryRights.WriteKey | RegistryRights.SetValue | RegistryRights.Delete,
                                                    InheritanceFlags.ContainerInherit,
                                                    PropagationFlags.None,
                                                    AccessControlType.Allow));
                RegistryKey key;
                key = Registry.LocalMachine.CreateSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, rs);
                key.SetAccessControl(rs);

                //Check if Reg-Dword exist beore creating it
                object regDwordVal = key.GetValue("Policy");
                if (regDwordVal == null)
                {
                    key.SetValue("Policy", 1, RegistryValueKind.DWord);
                    rebootRequired = true;
                }

                key.Close();
                Logger.FunctionLeave();
                return true;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
                return false;
            }
        }

        /// <summary>
        /// Determine if a reboot is needed to apply updates
        /// </summary>
        /// <returns></returns>
        internal static bool IsRebootPending()
        {
            Logger.FunctionEnter();
            const string psFile = @".\CheckRebootState.ps1";
            bool rc = false;

            // next requires wu service to be running
            //WUApiLib.UpdateInstaller inst = new UpdateInstaller();
            //rc = inst.RebootRequiredBeforeInstallation;

            if (!rc && File.Exists(psFile) && !CloudUtilities.IsCloudOS())
            {
                if (PS.RunPSCommand(@"-File " + psFile, out string outstr, true))
                {
                    Logger.Debug("output string from PS: " + outstr);
                    rc = outstr.ToLower().Contains("true");
                }
            }

            if (!rc)
            {
                Logger.Debug("Com lookup returned false, checking registry to confirm");

                // check registry keys for potential pending updates
                const string _rebootKey1 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\";
                const string _rebootKey2 = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\";
                const string _rebootValue = "RebootRequired";
                const string _sessionMgrKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\";
                const string _pendingRenameValue = "PendingFileRenameOperations";

                Logger.Debug("Checking for {0}", _sessionMgrKey);
                RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                RegistryKey subKey = baseKey.OpenSubKey(_sessionMgrKey);
                if (subKey != null)
                {
                    Logger.Debug("Getting Subkey {0}", _pendingRenameValue);
                    var value = subKey.GetValue(_pendingRenameValue);
                    if (value != null)
                    {
                        var stringList = new List<string>(value as string[]);
                        if (stringList.Count > 0)
                        {
                            Logger.Comment("Pending Reboot Detected: Pending rename registry key setting found");
                            rc = true;
                        }
                    }
                }

                // NOTE: the next may fail on domain-joined machines due to MS domain admin policies.
                //       this should run ok for WTT machines which is the intended target.
                Logger.Debug("Checking {0}", _rebootKey1);
                subKey = baseKey.OpenSubKey(_rebootKey1);

                if (subKey != null)
                {
                    string[] subKeys = subKey.GetSubKeyNames();
                    if (subKeys.Contains(_rebootValue))
                    {
                        Logger.Comment("Pending Reboot Detected: Pending reboot required registry key found");
                        rc = true;
                    }
                }

                Logger.Debug("Checking {0}", _rebootKey2);
                subKey = baseKey.OpenSubKey(_rebootKey2);
                if (subKey != null)
                {
                    string[] subKeys = subKey.GetSubKeyNames();
                    if (subKeys.Contains(_rebootValue))
                    {
                        Logger.Comment("Pending Reboot Detected: Pending reboot required registry key found");
                        rc = true;
                    }
                }
            }

            Logger.Debug("Reboot Pending Query Returning {0}", rc);
            Logger.FunctionLeave();

            return rc;
        }
    }
}
