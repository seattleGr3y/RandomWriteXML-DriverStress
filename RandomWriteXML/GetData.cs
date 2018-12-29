﻿using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RandomWriteXML
{
    class GetData
    {
        private static string hardwareID;
        /// <summary>
        /// just getting the hardware ID for firmware install to pass it to create\update a regkey
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static string FirmwareInstallGetHID(string line)
        {
            Logger.FunctionEnter();
            string getHardwareID = "FirmwareId";
            string[] textInput;
            textInput = File.ReadAllLines(line);

            foreach (string textLine in textInput)
            {
                if (Regex.IsMatch(textLine, getHardwareID, RegexOptions.IgnoreCase).Equals(true))
                {
                    int begingIndex = textLine.IndexOf('{');
                    int endingIndex = textLine.IndexOf('}');

                    if ((begingIndex >= 0) && (endingIndex >= 0))
                        hardwareID = textLine.Substring(begingIndex, (endingIndex + 1) - begingIndex).TrimStart('{').TrimEnd('}');
                }
            }
            Logger.FunctionLeave();
            return hardwareID;
        }

        /// <summary>
        /// just getting the driver version for install to check installation is good etc.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static string GetDriverVersion(string line)
        {
            Logger.FunctionEnter();
            string getVersion = "DriverVer=";
            string getVersion2 = "DriverVer = ";
            string[] textInput;
            string expectedDriverVersion = string.Empty;
            string result = string.Empty;
            textInput = File.ReadAllLines(line);

            foreach (string textLine in textInput)
            {
                if (Regex.IsMatch(textLine, getVersion).Equals(true))
                {
                    expectedDriverVersion = textLine.Split(',')[1];
                    Logger.Comment("the following should be the driver version");
                    Logger.Comment(expectedDriverVersion);
                    result = expectedDriverVersion;
                }
                if (Regex.IsMatch(textLine, getVersion2).Equals(true))
                {
                    expectedDriverVersion = textLine.Split(',')[1];
                    Logger.Comment("the following should be the driver version");
                    Logger.Comment(expectedDriverVersion);
                    result = expectedDriverVersion;
                }
            }
            Logger.FunctionLeave();
            return result;
        }

        /// <summary>
        /// just getting the driver version for install to check installation is good etc.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static string GetDriverDate(string line)
        {
            //
            string getVersion = "DriverVer=";
            string getVersion2 = "DriverVer = ";
            string[] textInput;
            string expectedDriverDate = string.Empty;
            textInput = File.ReadAllLines(line);

            foreach (string textLine in textInput)
            {
                if (Regex.Match(textLine, getVersion).Success)
                {
                    expectedDriverDate = textLine.Split(',')[0].Split('=')[1];
                    Logger.Comment("the following should be the driver date");
                    Logger.Comment(expectedDriverDate);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Expected Driver Date : " + expectedDriverDate);
                    Console.ForegroundColor = ConsoleColor.White;
                    //Console.ReadKey();
                    return expectedDriverDate;
                }
                else if (Regex.Match(textLine, getVersion2).Success)
                {
                    expectedDriverDate = textLine.Split(',')[0].Split('=')[1]; ;
                    Logger.Comment("the following should be the driver date");
                    Logger.Comment(expectedDriverDate);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Expected Driver Date : " + expectedDriverDate);
                    Console.ForegroundColor = ConsoleColor.White;
                    //Console.ReadKey();
                    return expectedDriverDate;
                }
            }
            return expectedDriverDate;
        }

        /// <summary>
        /// as the name implies getting the exception message to pass to try\catch
        /// </summary>
        /// <param name="ex"></param>
        internal static void GetExceptionMessage(Exception ex)
        {
            Logger.Error(ex.ToString());
            Console.ReadKey();
        }

        /// <summary>
        /// just a way to seperately print out exit code from where ever it may be used
        /// currently only in one spot in Install_Uninstall.cs
        /// </summary>
        /// <param name="installExitCode"></param>
        /// <param name="stdOutput"></param>
        internal static string GetExitCode(string installExitCode, string stdOutput, string errorMessage)
        {
            Logger.FunctionEnter();
            Logger.Comment("Install_Uninstall exit code values should print out here: ");
            Logger.Comment("installationExitCode" + installExitCode);
            Logger.Debug("install stdOutput : " + stdOutput);
            Logger.Debug("install errorMessage : " + errorMessage);
            Logger.FunctionLeave();
            return installExitCode;
        }

        /// <summary>
        /// will check if driver is a capsule (firmware)
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static bool CheckDriverIsFirmware(string line, int executionCount, int infListCount)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("CheckDriverIsFirmware method : " + line);
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(500);
            Logger.FunctionEnter();
            string getIsFirmware = "Class=Firmware";
            bool result = false;
            string[] infFileContent = File.ReadAllLines(line);

            foreach (string infLine in infFileContent)
            {
                if (infLine.Contains(getIsFirmware).Equals(true))
                {
                    //Logger.Comment(infLine);
                    return true;
                }
                else if (infLine.Contains(getIsFirmware).Equals(false))
                {
                    result = false;
                }
                else
                {
                    continue;
                }
            }
            Logger.FunctionLeave();
            return result;
        }

        /// <summary>
        /// looks for the string to identify if the INF is UEFI
        /// </summary>
        /// <param name="line"></param>
        /// <param name="startChoice"></param>
        /// <returns></returns>
        internal static bool IsLikeChoice(string line, string startChoice)
        {
            Logger.FunctionEnter();
            bool result = false;
            try
            {
                string GetFirmwareName = "CatalogFile=";
                string[] textInput;
                string expectedDriverVersion = string.Empty;
                textInput = File.ReadAllLines(line);

                foreach (string textLine in textInput)
                {
                    if (textLine.Contains(GetFirmwareName))
                    {
                        string textLineStartChoice = textLine.Split('=')[1];
                        if (Regex.Match(textLineStartChoice, startChoice, RegexOptions.IgnoreCase).Success)
                        {
                            Logger.Comment("this is textLineToTest " + textLineStartChoice);
                            result = true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }
            Logger.FunctionLeave();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="needRollBack"></param>
        /// <param name="infName"></param>
        /// <param name="rollBackLine"></param>
        /// trying to check if this is firmware that has been installed during the test
        /// that now needs to be 'installed' via the rollback folder so the old version is 
        /// now installed again (rollback)
        /// <returns></returns>
        internal static void IfWillNeedRollBack(string line, bool needRollBack, string infName)
        {
            try
            {
                Logger.FunctionEnter();
                string rbInfName = Path.GetFileNameWithoutExtension(line);
                string rollbackINFnameDIR = @"\" + rbInfName;
                string fullRollBackDir = Program.rollBackDir + rollbackINFnameDIR;
                if (!Directory.Exists(fullRollBackDir))
                {
                    Directory.CreateDirectory(fullRollBackDir);
                }
                string dirToCopy = @"C:\Windows\System32\DriverStore\FileRepository\";
                foreach (string dirToTest in Directory.EnumerateDirectories(dirToCopy))
                {
                    if (Directory.Exists(fullRollBackDir)) { break; }
                    if (Regex.Match(dirToTest, infName, RegexOptions.IgnoreCase).Success)
                    {
                        // Get the subdirectories for the specified directory.
                        DirectoryInfo dir = new DirectoryInfo(dirToTest);
                        // Get the files in the directory and copy them to the new location.
                        FileInfo[] files = dir.GetFiles();
                        foreach (FileInfo file in files)
                        {
                            string temppath = Path.Combine(fullRollBackDir, file.Name);

                            if (file.Exists)
                            {
                                continue;
                            }
                            else
                            {
                                file.CopyTo(temppath, false);
                            }
                        }
                        Utilities.DirectoryCopy(dirToTest, fullRollBackDir, true);
                        Logger.Comment("copy stuff to RollBacks folder...");
                    }
                    else
                    {
                        continue;
                    }
                }
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }
        }

        /// <summary>
        /// determine what from the list is to be installed first
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static string GetTestFirst(string InputTestFilePath)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePath);
                string testFirst = testInputData.XPathSelectElement("/Tests/TestChoices/StartChoice").Value;
                string startChoice = testFirst;
                Logger.Comment("testing this first each loop around : " + startChoice);
                Logger.FunctionLeave();
                return startChoice;
            }
            catch (Exception ex)
            {
                GetExceptionMessage(ex);
                return null;
            }
        }

        /// <summary>
        /// determine what from the list is to be installed first
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static bool GetRandomChoice(string InputTestFilePath)
        {
            bool result = false;
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePath);
                string randomizeChoiceSTR = testInputData.XPathSelectElement("/Tests/TestChoices/randomizeList").Value;
                bool randomizeChoice = Convert.ToBoolean(randomizeChoiceSTR);
                Logger.Comment("entered for randomize or not choice : " + randomizeChoiceSTR);
                Logger.FunctionLeave();
                if (randomizeChoice.Equals(null)) { randomizeChoice = false; }
                result = randomizeChoice;
            }
            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }
            return result;
        }

        #region TESTING THIS METHOD 10/17/2018
        /// <summary>
        /// if no choice is set and there is firmware set highest pri firmware to install first
        /// for now this will default to UEFI
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <param name="infName"></param>
        /// <returns></returns>
        internal static void SetTestFirst(string InputTestFilePathBAK, string InputTestFilePath, string infName)
        {
            try
            {
                Logger.FunctionEnter();
                var testInputData = XDocument.Load(InputTestFilePath);
                Logger.Comment("setting this to test first as there was no choice made : " + infName);
                testInputData.XPathSelectElement("/DriverTests/Test/TestFirst").Value = infName;
                testInputData.Save(InputTestFilePath);
                Utilities.CopyFile(InputTestFilePath, InputTestFilePathBAK);
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }
        }
        #endregion

        /// <summary>
        /// this will check for crash dump and just copy it to our reults folder
        /// </summary>
        /// <returns></returns>
        internal static bool CheckCrashDumpOccurred()
        {
            Logger.FunctionEnter();
            bool crashDumpOccurred = false;
            string crashDumpPath = string.Empty;
            string timeStamp = string.Empty;
            string newMemoryDumpFIle = string.Empty;
            string[] miniDumpFiles = null;

            string SystemRoot = Environment.ExpandEnvironmentVariables("%systemroot%");
            string memoryDumpFile = SystemRoot + "\\MEMORY.DMP";
            string miniDumpPath = SystemRoot + "\\MINIDUMP";

            if (File.Exists(memoryDumpFile))
            {
                crashDumpOccurred = true;
                crashDumpPath = memoryDumpFile;

                //Rename crash dump file
                timeStamp = DateTime.Now.ToLocalTime().ToString();
                newMemoryDumpFIle = crashDumpPath.ToUpper().Replace(".DMP", String.Empty) + "_" + timeStamp + ".DMP";

                File.Move(crashDumpPath, newMemoryDumpFIle);
            }
            else if (Directory.Exists(miniDumpPath))
            {
                string miniDumpArchiveDir = "MiniDumpArchive";
                miniDumpFiles = Directory.GetFiles(miniDumpPath, "*.DMP");

                if (Directory.Exists(miniDumpPath) && (!Directory.Exists(miniDumpPath + "\\" + miniDumpArchiveDir)))
                {
                    Directory.CreateDirectory(miniDumpPath + "\\" + miniDumpArchiveDir);
                }

                if (miniDumpFiles.Count() > 0)
                {

                    crashDumpOccurred = true;
                    int fileStartIndex = miniDumpFiles[0].LastIndexOf('\\') + 1;
                    string fileNameName = miniDumpFiles[0].Substring(fileStartIndex);

                    crashDumpPath = miniDumpFiles[0];

                    //Rename crash dump file
                    timeStamp = DateTime.Now.ToLocalTime().ToString();
                    string archivedMinidumpFile = fileNameName.ToUpper().Replace(".DMP", String.Empty) + "_" + timeStamp + ".DMP";
                    newMemoryDumpFIle = Program.dirName + miniDumpArchiveDir + "\\" + archivedMinidumpFile;
                    File.Move(crashDumpPath, newMemoryDumpFIle);

                }
            }
            Logger.FunctionLeave();
            return crashDumpOccurred;
        }


        internal static List<string> GetInfPathsList(string supportFolderLocation)
        {
            List<string> infPathList = new List<string>();

            var infFiles = Directory.EnumerateFiles(supportFolderLocation, "*.inf", SearchOption.AllDirectories);
            foreach (string infFile in infFiles)
            {
                string infDir = Path.GetDirectoryName(infFile);
                string infRealName = Path.GetFileNameWithoutExtension(infFile);
                string infPathTMP = Path.GetFullPath(infFile);
                if (infPathTMP.Contains("RollBacks"))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("RollBacks...don't add this to the damn list...");
                    Console.ForegroundColor = ConsoleColor.White;
                    continue;
                }
                else if (infDir.Contains(infRealName))
                {
                    infPathList.Add(infFile);
                }
            }
            return infPathList;
        }
    }
}
