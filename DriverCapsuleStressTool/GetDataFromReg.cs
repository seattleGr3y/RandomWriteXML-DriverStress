using Microsoft.HWSW.Test.Utilities;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DriverCapsuleStressTool
{
    class GetDataFromReg
    {
        internal static string attemptStatusValue;

        /// <summary>
        /// check the registry to be sure firmware is installed
        /// GetDataFromReg.CheckRegIsInstalled(infName, hardwareID);
        /// </summary>
        /// <param name="infName"></param>
        /// <returns> value of LastAttempStatus or 0x12345678 if there was an error reading data </returns>
        internal static bool CheckRegIsInstalled(string infName, string hardwareID)
        {
            Logger.FunctionEnter();
            bool isInstalledREGcheck = false;
            bool result = false;
            string actualInfName = Path.GetFileNameWithoutExtension(infName);
            RegistryKey baseRk;
            baseRk = Registry.LocalMachine.OpenSubKey(@"HARDWARE\UEFI\ESRT");
            string[] subKeyList = baseRk.GetSubKeyNames();

            try
            {
                foreach (string subKey in subKeyList)
                {
                    RegistryKey regkey = baseRk.OpenSubKey(subKey);
                    if (Regex.Match(subKey, hardwareID, RegexOptions.IgnoreCase).Success)
                    {
                        // get the "LastAttemptStatus"
                        attemptStatusValue = regkey.GetValue("LastAttemptStatus").ToString();
                        int attemptStatusValueINT = Convert.ToInt32(attemptStatusValue);

                        if (attemptStatusValueINT == 0 )
                        {
                            isInstalledREGcheck = true;
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Comment("result from the registry isInstalledREGcheck : " + isInstalledREGcheck);
                        Console.ForegroundColor = ConsoleColor.White;
                        result = isInstalledREGcheck;

                    }
                    else { continue; }
                    //}
                }
            }
            catch (Exception ex)
            {
                Logger.Comment("exception thrown in GetDataFromReg checking if driver is installed...");
                GetData.GetExceptionMessage(ex);
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Logger.Comment("result from the registry isInstalledREGcheck 1 : " + result);
            Console.ForegroundColor = ConsoleColor.White;
            return result;
        }
    }
}
