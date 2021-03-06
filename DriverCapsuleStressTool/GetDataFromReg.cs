﻿using Microsoft.HWSW.Test.Utilities;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace DriverCapsuleStressTool
{
    class GetDataFromReg
    {
        internal static string infNameFromReg = null;
        internal static string result = null;
        internal static string attemptStatusValue;
        internal static string versionValue;
        internal static string oemInfPath = @"C:\Windows\INF\";

        #region  -- STILL NOT WORKING CORRECTLY FOR PnP DRIVERS COME BACK TO THIS -- 
        ///// <summary>
        ///// check the registry to be sure firmware is installed
        ///// still testing this
        ///// GetDataFromReg.CheckRegDriverIsInstalled(classGUID, infName, hardwareID, expectedDriverVersion, line);
        ///// </summary>
        ///// <param name="infName"></param>
        ///// <returns> value of LastAttempStatus or 0x12345678 if there was an error reading data </returns>
        //internal static bool CheckRegDriverIsInstalled(string classGUID, string infName, string hardwareID, string expectedDriverVersion, string line)
        //{
        //    Logger.FunctionEnter();
        //    bool isInstalledREGcheck = false;
        //    string attemptVersionValue;
        //    bool result = false;
        //    string actualInfName = Path.GetFileNameWithoutExtension(infName);
        //    RegistryKey baseRk;
        //    baseRk = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Control\Class\");
        //    string[] subKeyList = baseRk.GetSubKeyNames();
        //    string[] textInput = File.ReadAllLines(line);

        //    try
        //    {
        //        if (string.IsNullOrEmpty(hardwareID))
        //        {
        //            foreach (string subKey in subKeyList)
        //            {
        //                RegistryKey regkey = baseRk.OpenSubKey(subKey);
        //                if (Regex.Match(subKey, classGUID, RegexOptions.IgnoreCase).Success)
        //                {
        //                    // get the "LastAttemptStatus" "LastAttemptVersion" "Version"
        //                    // using what I need to when needed may use them all eventually
        //                    // for now just gathering Version ...likely all I need

        //                    if (!subKey.Contains("Version")) { continue; }
        //                    attemptStatusValue = regkey.GetValue("LastAttemptStatus").ToString();
        //                    versionValue = regkey.GetValue("Version").ToString();
        //                    attemptVersionValue = regkey.GetValue("LastAttemptVersion").ToString();
        //                    // int attemptStatusValueINT = Convert.ToInt32(attemptStatusValue);

        //                    foreach (string textLine in textInput)
        //                    {
        //                        if (Regex.Match(textLine, "Class=System", RegexOptions.IgnorePatternWhitespace).Success)
        //                        {
        //                            break;
        //                        }
        //                        if (Regex.Match(textLine, versionValue).Success)
        //                        {
        //                            if (attemptStatusValue.Equals("0"))
        //                            {
        //                                isInstalledREGcheck = true;
        //                                Logger.Comment("Matched the HW REV in the inf and Registry : " + attemptVersionValue);
        //                            }
        //                        }
        //                    }
        //                    Logger.Comment("result from the registry isInstalledREGcheck : " + attemptVersionValue);
        //                    result = isInstalledREGcheck;
        //                }
        //                else { continue; }
        //            }
        //        }
        //        else if (isInstalledREGcheck == false)
        //        {
        //            foreach (string subKey in subKeyList)
        //            {
        //                RegistryKey regkey = baseRk.OpenSubKey(subKey);
        //                if (Regex.Match(subKey, hardwareID, RegexOptions.IgnoreCase).Success)
        //                {
        //                    attemptStatusValue = regkey.GetValue("LastAttemptStatus").ToString();
        //                    versionValue = regkey.GetValue("Version").ToString();
        //                    attemptVersionValue = regkey.GetValue("LastAttemptVersion").ToString();
        //                    //int attemptStatusValueINT = Convert.ToInt32(attemptStatusValue);

        //                    foreach (string textLine in textInput)
        //                    {
        //                        if (Regex.Match(textLine, "Class=System", RegexOptions.IgnorePatternWhitespace).Success)
        //                        {
        //                            break;
        //                        }
        //                        if (Regex.Match(textLine, versionValue).Success)
        //                        {
        //                            if (attemptStatusValue.Equals("0"))
        //                            {
        //                                isInstalledREGcheck = true;
        //                                Logger.Comment("Matched the HW REV in the inf and Registry : " + attemptVersionValue);
        //                            }
        //                        }
        //                    }
        //                    Logger.Comment("result from the registry isInstalledREGcheck : " + attemptVersionValue);
        //                    result = isInstalledREGcheck;
        //                }
        //                else { continue; }
        //            }
        //        }
        //        // -----------
        //        else
        //        {
        //            RegistryKey baseRk2;
        //            baseRk2 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\WIndows\Current\CurrentVersion\Uninstall\");
        //            string[] subKeyList2 = baseRk2.GetSubKeyNames();

        //            foreach (string subKey2 in subKeyList2)
        //            {
        //                RegistryKey regkey = baseRk.OpenSubKey(subKey2);
        //                if (Regex.Match(subKey2, hardwareID, RegexOptions.IgnoreCase).Success)
        //                {
        //                    string uninstallSTR = regkey.GetValue("UninstallString").ToString();
        //                    versionValue = regkey.GetValue("DisplayVersion").ToString();
        //                    //attemptVersionValue = regkey.GetValue("LastAttemptVersion").ToString();
        //                    int attemptStatusValueINT = Convert.ToInt32(attemptStatusValue);

        //                    foreach (string textLine in textInput)
        //                    {
        //                        if (Regex.Match(textLine, "Class=System", RegexOptions.IgnorePatternWhitespace).Success)
        //                        {
        //                            break;
        //                        }
        //                        if (textLine.Contains(versionValue))
        //                        {
        //                            if (uninstallSTR.Contains(infName))
        //                            {
        //                                isInstalledREGcheck = true;
        //                            }
        //                        }
        //                    }
        //                    Logger.Comment("result from the registry isInstalledREGcheck : " + isInstalledREGcheck);
        //                    result = isInstalledREGcheck;
        //                }
        //                else { continue; }
        //            }
        //        }
        //        //------------
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Comment("exception thrown in GetDataFromReg checking if driver is installed...");
        //        GetData.GetExceptionMessage(ex);
        //    }
        //    Logger.Comment("result from the registry isInstalledREGcheck : " + result);
        //    return result;
        //}
        #endregion

        // 0: Success
        // 1: Unsuccessful
        // 2: Insufficient resources
        // 3: Incorrect version
        // 4: Invalid image format
        // 5: Authentication error
        // 6: Power event - AC not connected
        // 7: Power event - Insufficient battery
        /// <summary>
        /// check the registry to be sure firmware is installed
        /// GetDataFromReg.CheckRegIsInstalled(infName, hardwareID);
        /// </summary>
        /// <param name="infName"></param>
        /// <returns> value of LastAttempStatus or 0x12345678 if there was an error reading data </returns>
        internal static string CheckRegCapsuleIsInstalled(string infName, string hardwareID, string expectedDriverVersion, string line)
        {
            Logger.FunctionEnter();
            string isInstalledREGcheck = string.Empty;
            string attemptVersionValue;
            string result = string.Empty;
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
                        attemptStatusValue = regkey.GetValue("LastAttemptStatus").ToString();
                        int attemptStatusValueINT = Convert.ToInt32(attemptStatusValue);
                        versionValue = regkey.GetValue("Version").ToString();
                        int versionValueINT = Convert.ToInt32(versionValue);
                        var regValHex = versionValueINT.ToString("X");
                        string regValueHex = "0x" + regValHex.ToLower();
                        string searchString = "HKR,,FirmwareVersion,%REG_DWORD%,"; 
                        attemptVersionValue = regkey.GetValue("LastAttemptVersion").ToString();
                        string isInstalled = string.Empty;                        

                        foreach (string textLine in textInput)
                        {
                            if (textLine.StartsWith(searchString))
                            {
                                string matchedValue = textLine.Split(',')[4].ToLower();
                                if (regValueHex.Equals(matchedValue))
                                {
                                    switch (isInstalled)
                                    {
                                        case string failed when (attemptStatusValueINT == 1):
                                            isInstalledREGcheck = "unsuccessful";
                                            break;

                                        case string InsufficientResources when (attemptStatusValueINT == 2): 
                                            isInstalledREGcheck = "InsufficientResources";
                                            break;

                                        case string IncorrectVersion when (attemptStatusValueINT == 3):
                                            isInstalledREGcheck = "IncorrectVersion";
                                            break;

                                        case string invalidImage when (attemptStatusValueINT == 4):
                                            isInstalledREGcheck = "invalidImage";
                                            break;

                                        case string authenticationERR when (attemptStatusValueINT == 5):
                                            isInstalledREGcheck = "authenticationERR";
                                            break;

                                        case string ACnotConnected when (attemptStatusValueINT == 6):
                                            isInstalledREGcheck = "ACnotConnected";
                                            break;

                                        case string insufficientPower when (attemptStatusValueINT == 7):
                                            isInstalledREGcheck = "insufficientPower";
                                            break;

                                        default:
                                            isInstalledREGcheck = "pass";
                                            Logger.Comment("Matched the HW REV in the inf and Registry : " + isInstalledREGcheck);
                                            Logger.Comment("which means it definitely installed...");
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine("Matched the HW REV in the inf and Registry : " + isInstalledREGcheck);
                                            Console.WriteLine("which means it definitely installed...");
                                            Console.WriteLine("regValueHex : " + regValueHex);
                                            Console.ForegroundColor = ConsoleColor.White;
                                            break;
                                    }
                                }
                            }
                        }
                        result = isInstalledREGcheck;
                    }
                    else { continue; }
                }
            }
            catch (Exception ex)
            {
                Logger.Comment("exception thrown in GetDataFromReg checking if firmware is installed...");
                GetData.GetExceptionMessage(ex);
            }
            Logger.Comment("result from the registry isInstalledREGcheck : " + result);
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
                Logger.Comment("exception thrown in GetDataFromReg getting oem inf name...");
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
                Logger.Comment("exception thrown in GetDataFromReg getting oem inf name string...");
                GetData.GetExceptionMessage(ex);
            }
            Logger.FunctionLeave();
            return result;
        }
    }
}