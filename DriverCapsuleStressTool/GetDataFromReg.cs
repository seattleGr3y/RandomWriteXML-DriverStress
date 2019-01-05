using Microsoft.HWSW.Test.Utilities;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace DriverCapsuleStressTool
{
    class GetDataFromReg
    {
        //internal static string infNameKeyValue;
        //internal static string origInfNameKey;
        internal static string infNameFromReg = null;
        //internal static string origInfName;
        internal static string result = null;
        internal static string attemptStatusValue;
        internal static string versionValue;
        internal static string oemInfPath = @"C:\Windows\INF\";

        /// <summary>
        /// check the registry to be sure firmware is installed
        /// GetDataFromReg.CheckRegIsInstalled(infName, hardwareID);
        /// </summary>
        /// <param name="infName"></param>
        /// <returns> value of LastAttempStatus or 0x12345678 if there was an error reading data </returns>
        internal static bool CheckRegIsInstalled(string infName, string hardwareID, string expectedDriverVersion, string line)
        {
            Logger.FunctionEnter();
            bool isInstalledREGcheck = false;
            string attemptVersionValue;
            bool result = false;
            //string versionInINF;
            string actualInfName = Path.GetFileNameWithoutExtension(infName);
            RegistryKey baseRk;
            baseRk = Registry.LocalMachine.OpenSubKey(@"HARDWARE\UEFI\ESRT");
            string[] subKeyList = baseRk.GetSubKeyNames();
            string[] textInput = File.ReadAllLines(line);

            try
            {
                foreach (string subKey in subKeyList)
                {
                    RegistryKey regkey = baseRk.OpenSubKey(subKey);
                    if (Regex.Match(subKey, hardwareID, RegexOptions.IgnoreCase).Success)
                    {
                        // get the "LastAttemptStatus"
                        attemptStatusValue = regkey.GetValue("LastAttemptStatus").ToString();
                        versionValue = regkey.GetValue("Version").ToString();
                        attemptVersionValue = regkey.GetValue("LastAttemptVersion").ToString();
                        int attemptStatusValueINT = Convert.ToInt32(attemptStatusValue);

                        //if (attemptVersionValue.Contains(expectedDriverVersion))
                        //{
                        //    if (attemptStatusValueINT == 0)
                        //    {
                        //        isInstalledREGcheck = true;
                        //    }
                        //}

                        foreach (string textLine in textInput)
                        {
                            if (Regex.Match(textLine, versionValue).Success)
                            {
                                if (attemptStatusValueINT == 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine("Matched the HW REV in the inf and Registry : " + isInstalledREGcheck);
                                    Console.ForegroundColor = ConsoleColor.White;
                                    isInstalledREGcheck = true;
                                }
                            }
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

        /// <summary>
        /// if exists find the oem##.inf file and delete it before attempting rollback
        /// GetDataFromReg.GetOEMinfNameFromReg(infName, hardwareID);
        /// </summary>
        /// <param name="infName"></param>
        /// <param name="hardwareID"></param>
        /// <returns></returns>
        internal static void GetOEMinfNameFromReg(string actualInfName, string hardwareID, string classGUID)
        {
            string infNameFromReg = string.Empty;
            Logger.FunctionEnter();
            try
            {
                RegistryKey baseRk2;
                baseRk2 = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Control\Class\");
                string[] subKeyList2 = baseRk2.GetSubKeyNames();
                //hardwareID = hardwareID.TrimStart('{').TrimEnd('}');
                foreach (string subKey2 in subKeyList2)
                {
                    if (string.IsNullOrEmpty(hardwareID)) { break; }
                    if (string.IsNullOrEmpty(subKey2)) { continue; }

                    RegistryKey regkey2 = baseRk2.OpenSubKey(subKey2);

                    if (regkey2.ToString().Contains(classGUID))
                    {
                        string[] keyValueNamesList2 = regkey2.GetSubKeyNames();

                        foreach (string keySubKeyValueName in keyValueNamesList2)
                        {
                            if (string.IsNullOrEmpty(keySubKeyValueName)) { continue; }
                            if (keySubKeyValueName.Equals("Properties")) { continue; }

                            RegistryKey SUBregkey2 = regkey2.OpenSubKey(keySubKeyValueName);

                            string[] subSubkeyValueNamesList2 = SUBregkey2.GetValueNames();

                            foreach (string subSUBsubKeyValueName in subSubkeyValueNamesList2)
                            {
                                if (string.IsNullOrEmpty(subSUBsubKeyValueName)) { continue; }

                                if (subSUBsubKeyValueName.Equals("MatchingDeviceId"))
                                {
                                    string matchDevIDkey = SUBregkey2.GetValue("MatchingDeviceId").ToString().ToLower();

                                    if (matchDevIDkey.Contains(hardwareID.ToLower()))
                                    {
                                        infNameFromReg = SUBregkey2.GetValue("InfPath").ToString();
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("infNameFromReg : " + infNameFromReg);
                                        Console.ForegroundColor = ConsoleColor.White;

                                        foreach (string infToCheck in Directory.EnumerateFiles(oemInfPath))
                                        {
                                            if (infToCheck.Contains(infNameFromReg))
                                            {
                                                Console.ForegroundColor = ConsoleColor.Yellow;
                                                Console.WriteLine("deleting the oem##.inf file : " + infNameFromReg);
                                                Console.ForegroundColor = ConsoleColor.White;
                                                Logger.Comment("deleting the oem##.inf file : " + infNameFromReg);
                                                File.Delete(oemInfPath + infNameFromReg);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Comment("exception thrown in GetDataFromReg checking if driver is installed...");
                GetData.GetExceptionMessage(ex);
            }
            Logger.FunctionLeave();
        }

        /// <summary>
        /// if exists find the oem##.inf file and delete it before attempting rollback
        /// GetDataFromReg.GetOEMinfNameFromReg(infName, hardwareID);
        /// </summary>
        /// <param name="infName"></param>
        /// <param name="hardwareID"></param>
        /// <returns></returns>
        internal static string GetOEMinfNameFromRegSTR(string actualInfName, string hardwareID, string classGUID)
        {
            string result = string.Empty;
            string infNameFromReg = string.Empty;
            Logger.FunctionEnter();
            try
            {
                RegistryKey baseRk2;
                baseRk2 = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Control\Class\");
                string[] subKeyList2 = baseRk2.GetSubKeyNames();
                //hardwareID = hardwareID.TrimStart('{').TrimEnd('}');
                foreach (string subKey2 in subKeyList2)
                {
                    if (string.IsNullOrEmpty(subKey2)) { continue; }

                    RegistryKey regkey2 = baseRk2.OpenSubKey(subKey2);

                    if (regkey2.ToString().Contains(classGUID))
                    {
                        string[] keyValueNamesList2 = regkey2.GetSubKeyNames();

                        foreach (string keySubKeyValueName in keyValueNamesList2)
                        {
                            if (string.IsNullOrEmpty(keySubKeyValueName)) { continue; }
                            if (keySubKeyValueName.Equals("Properties")) { continue; }

                            RegistryKey SUBregkey2 = regkey2.OpenSubKey(keySubKeyValueName);

                            string[] subSubkeyValueNamesList2 = SUBregkey2.GetValueNames();

                            foreach (string subSUBsubKeyValueName in subSubkeyValueNamesList2)
                            {
                                if (string.IsNullOrEmpty(subSUBsubKeyValueName)) { continue; }

                                if (subSUBsubKeyValueName.Equals("MatchingDeviceId"))
                                {
                                    string matchDevIDkey = SUBregkey2.GetValue("MatchingDeviceId").ToString().ToLower();

                                    if (matchDevIDkey.Contains(hardwareID.ToLower()))
                                    {
                                        infNameFromReg = SUBregkey2.GetValue("InfPath").ToString();
                                        result = infNameFromReg;
                                        Console.WriteLine(infNameFromReg);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Comment("exception thrown in GetDataFromReg checking if driver is installed...");
                GetData.GetExceptionMessage(ex);
            }
            Logger.FunctionLeave();
            return result;
        }
    }
}