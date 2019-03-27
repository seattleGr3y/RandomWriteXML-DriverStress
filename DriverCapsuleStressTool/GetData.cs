using Microsoft.HWSW.Test.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DriverCapsuleStressTool
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
            try
            {
                foreach (string textLine in textInput)
                {
                    if (Regex.IsMatch(textLine, getHardwareID, RegexOptions.IgnoreCase).Equals(true))
                    {
                        int begingIndex = textLine.IndexOf('{');
                        int endingIndex = textLine.IndexOf('}');

                        if ((begingIndex >= 0) && (endingIndex >= 0))
                        {
                            hardwareID = textLine.Substring(begingIndex, (endingIndex + 1) - begingIndex).TrimStart('{').TrimEnd('}');
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }

            Logger.FunctionLeave();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("hardwareID in the GetHID method : " + hardwareID);
            Console.ForegroundColor = ConsoleColor.White;
            return hardwareID;
        }

        /// <summary>
        /// just getting the driver version for install to check installation is good etc.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static string GetDriverVersion(string line)
        {
            string result = string.Empty;
            try
            {
                Logger.FunctionEnter();
                string getVersion = "DriverVer";
                string[] textInput;
                string expectedDriverVersion = string.Empty;
                textInput = File.ReadAllLines(line);

                foreach (string textLine in textInput)
                {
                    if (Regex.IsMatch(textLine, getVersion).Equals(true))
                    {
                        if (textLine.Contains("="))
                        {
                            expectedDriverVersion = textLine.Split(',')[1];
                            Logger.Comment("the following should be the driver version");
                            Logger.Comment(expectedDriverVersion);
                            result = expectedDriverVersion;
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
        /// just getting the classGUID to check installation is good etc.
        /// GetData.GetClassGUID(line);
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static string GetClassGUID(string line)
        {
            string result = string.Empty;
            try
            {
                Logger.FunctionEnter();
                string getClassGuid = "ClassGUID=";
                string getClassGuid2 = "ClassGuid=";
                string classGUID = string.Empty;
                string[] textInput;
                textInput = File.ReadAllLines(line);

                foreach (string textLine in textInput)
                {
                    if (Regex.Match(textLine, getClassGuid).Success)
                    {
                        classGUID = textLine.Split('=')[1];
                        Logger.Comment("the following should be the classGUID");
                        Logger.Comment(classGUID);
                        result = classGUID;
                    }
                    else if (Regex.Match(textLine, getClassGuid2).Success)
                    {
                        classGUID = textLine.Split('=')[1];
                        Logger.Comment("the following should be the classGUID");
                        Logger.Comment(classGUID);
                        result = classGUID;
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
        /// just getting the driver version for install to check installation is good etc.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static string GetDriverDate(string line)
        {
            string result = string.Empty;
            try
            {
                string expectedDriverDate;
                string getVersion = "DriverVer";
                string[] textInput;
                textInput = File.ReadAllLines(line);

                foreach (string textLine in textInput)
                {
                    if (Regex.Match(textLine, getVersion).Success)
                    {
                        if (textLine.Contains("="))
                        {
                            expectedDriverDate = textLine.Split(',')[0].Split('=')[1];
                            Logger.Comment("the following should be the driver date");
                            Logger.Comment(expectedDriverDate);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Expected Driver Date : " + expectedDriverDate);
                            Console.ForegroundColor = ConsoleColor.White;
                            result = expectedDriverDate;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }
            return result;
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
            string result = string.Empty;
            try
            {
                Logger.FunctionEnter();
                Logger.Comment("Install_Uninstall exit code values should print out here: ");
                Logger.Comment("installationExitCode" + installExitCode);
                Logger.Debug("install stdOutput : " + stdOutput);
                Logger.Debug("install errorMessage : " + errorMessage);
                Logger.FunctionLeave();
                result = installExitCode;
            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            return result;
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
            Logger.FunctionEnter();
            string getIsFirmware = "Class";
            bool result = false;

            string[] infFileContent = File.ReadAllLines(line);

            try
            {
                foreach (string infLine in infFileContent)
                {
                    if (infLine.Contains(getIsFirmware).Equals(true))
                    {
                        if (infLine.Contains("Firmware").Equals(true))
                        {
                            Logger.Comment(infLine);
                            return true;
                        }
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
            }

            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("CheckDriverIsFirmware result : " + result);
            Console.ForegroundColor = ConsoleColor.White;
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
        /// Check if directory exists if not create it
        /// GetData.CreateIfMissing(path);
        /// </summary>
        /// <param name="path"></param>
        internal static void CreateIfMissing(string path)
        {
            bool folderExists = Directory.Exists(path);
            if (!folderExists)
            {
                Directory.CreateDirectory(path);
            }
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
        internal static void IfWillNeedRollBack(string line)
        {
            try
            {
                CreateIfMissing(Program.rollbackLine);
                Logger.FunctionEnter();
                string infName = Path.GetFileNameWithoutExtension(line);
                infName = Regex.Replace(infName, @"[\d-]", string.Empty);
                string rollbackINFnameDIR = @"\" + infName;
                string fullRollBackDir = Program.rollbackLine + rollbackINFnameDIR.ToLower();
                string rollbackExists = CheckRollbacksExist(line, infName);

                if (string.IsNullOrEmpty(rollbackExists))
                {
                    Console.WriteLine("No Rollbacks exist...create-copy now...");
                    string dirToCopy = @"C:\Windows\System32\DriverStore\FileRepository\";
                    foreach (string dirToTest in Directory.EnumerateDirectories(dirToCopy))
                    {
                        if (dirToTest.Contains(infName))
                        {
                            // Get the subdirectories for the specified directory.
                            DirectoryInfo dir = new DirectoryInfo(dirToTest);
                            // Get the files in the directory and copy them to the new location.
                            FileInfo[] files = dir.GetFiles();
                            foreach (FileInfo file in files)
                            {
                                string temppath = Path.Combine(fullRollBackDir, file.Name);
                                if (File.Exists(temppath))
                                {
                                    continue;
                                }
                                else
                                {
                                    CreateIfMissing(fullRollBackDir);
                                    string itemToCopy = dir + @"\" + file;
                                    Utilities.CopyFile(itemToCopy, temppath);
                                    Console.WriteLine("should be copying backups next...");
                                }
                                { continue; }
                            }
                            Logger.Comment("copied stuff to Rollbacks folder...");
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(fullRollBackDir + " fullRollBackDir already exists...move on and don't copy anything");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Logger.FunctionLeave();
            }
            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }
        }

        internal static string CheckRollbacksExist(string line, string infName)
        {
            CreateIfMissing(Program.rollbackLine);
            string result = string.Empty;
            infName = Path.GetFileNameWithoutExtension(infName);
            infName = infName.Split('.')[0].ToLower();
            infName = infName.Replace("surface", "");
            infName = Regex.Replace(infName, @"[\d-]", string.Empty);
            try
            {
                var infFiles = Directory.EnumerateFiles(Program.rollbackLine, "*.inf", System.IO.SearchOption.AllDirectories);
                foreach (string file in infFiles)
                {
                    if (Regex.Match(file, infName, RegexOptions.IgnoreCase).Success)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(file + " rollback file already exists somewhere so...move on and don't copy anything");
                        Console.ForegroundColor = ConsoleColor.White;
                        result = file;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }
            return result;
        }

        /// <summary>
        /// determine what from the list is to be installed first
        /// </summary>
        /// <param name="InputTestFilePath"></param>
        /// <returns></returns>
        internal static string GetTestFirst(string InputTestFilePath)
        {
            Logger.FunctionEnter();
            string startChoice = string.Empty;
            try
            {
                var testInputData = XDocument.Load(InputTestFilePath);
                string testFirst = testInputData.XPathSelectElement("/Tests/TestChoices/StartChoice").Value;
                startChoice = testFirst;
            }
            catch (Exception ex)
            {
                GetExceptionMessage(ex);
                return null;
            }
            Logger.FunctionLeave();
            return startChoice;
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
        /// GetData.SetTestFirst(InputTestFilePathBAK, InputTestFilePath, infName);
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
            try
            {
                string crashDumpPath = Program.dirName + @"\CrashDumps\";
                string timeStamp = string.Empty;
                string newMemoryDumpFIle = string.Empty;
                string[] miniDumpFiles = null;

                string SystemRoot = Environment.ExpandEnvironmentVariables("%systemroot%");
                string memoryDumpFile = SystemRoot + "\\MEMORY.DMP";
                string miniDumpPath = SystemRoot + "\\MINIDUMP";
                string miniDumpArchiveDir = @"\MiniDumpArchive";
                string miniDumpArchive = Program.dirName + miniDumpArchiveDir;

                if (File.Exists(memoryDumpFile))
                {
                    crashDumpOccurred = true;

                    //Rename crash dump file
                    memoryDumpFile = memoryDumpFile.Split('.')[0];
                    timeStamp = DateTime.Now.ToLocalTime().ToString();
                    newMemoryDumpFIle = memoryDumpFile.Replace(".DMP", memoryDumpFile + "_" + timeStamp + ".DMP");
                    string newMemDumpFileDIR = crashDumpPath + newMemoryDumpFIle;
                    CreateIfMissing(crashDumpPath);
                    Utilities.CopyFile(memoryDumpFile, newMemDumpFileDIR);
                }
                else if (Directory.Exists(miniDumpPath))
                {
                    miniDumpFiles = Directory.GetFiles(miniDumpPath, "*.DMP");
                    CreateIfMissing(miniDumpPath);

                    if (miniDumpFiles.Count() > 0)
                    {
                        crashDumpOccurred = true;
                        foreach (string fileName in Directory.EnumerateFiles(miniDumpPath))
                        {
                            timeStamp = DateTime.Now.ToLocalTime().ToString();
                            string archivedMinidumpFile = fileName.ToUpper().Replace(".DMP", String.Empty) + "_" + timeStamp + ".DMP";
                            newMemoryDumpFIle = miniDumpArchive + "\\" + archivedMinidumpFile;
                            string copyMiniDumpPath = miniDumpPath + fileName;
                            Utilities.CopyFile(copyMiniDumpPath, newMemoryDumpFIle);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }

            Logger.FunctionLeave();
            return crashDumpOccurred;
        }

        /// <summary>
        /// get the list of infs from the ZIPs present in the 'support folder' where the executable
        /// will be running from these are used to add to the XML
        /// </summary>
        /// <param name="supportFolderLocation"></param>
        /// <returns></returns>
        internal static List<string> GetInfPathsList(string supportFolderLocation)
        {
            List<string> infPathList = new List<string>();

            try
            {
                var infFiles = Directory.EnumerateFiles(supportFolderLocation, "*.inf", System.IO.SearchOption.AllDirectories);
                foreach (string infFile in infFiles)
                {
                    string infDir = Path.GetDirectoryName(infFile);
                    string infRealName = Path.GetFileNameWithoutExtension(infFile);
                    string infPathTMP = Path.GetFullPath(infFile);

                    if (Regex.Match(infFile, "rollbacks", RegexOptions.IgnoreCase).Success)
                    {
                        continue;
                    }

                    else //if (infFile.Contains("Surface"))
                    {
                        infPathList.Add(infFile);
                    }
                }
            }

            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }

            return infPathList;
        }


        /// <summary>
        /// get count of drivers to be installed
        /// int infsPathListCount = GetPathListCount(supportFolderLocation);
        /// </summary>
        /// <param name="supportFolderLocation"></param>
        /// <returns></returns>
        internal static int GetPathListCount(string supportFolderLocation)
        {
            List<string> infPathList = new List<string>();
            int driversPathListCount = 0;

            try
            {
                if (Program.custom.Equals("none"))
                {
                    var infFiles = Directory.EnumerateFiles(Program.dirName, "*.inf", System.IO.SearchOption.AllDirectories);
                    foreach (string infFile in infFiles)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(driversPathListCount);
                        Console.ForegroundColor = ConsoleColor.White;

                        string infDir = Path.GetDirectoryName(infFile);
                        string infRealName = Path.GetFileNameWithoutExtension(infFile);
                        string infPathTMP = Path.GetFullPath(infFile);

                        if (infPathTMP.Contains("rollbacks"))
                        {
                            continue;
                        }
                        if (infPathList.Contains(infRealName))
                        {
                            driversPathListCount++;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(driversPathListCount);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }

                else
                {
                    List<string> orderToRun = Program.custom.Split(',').ToList();
                    driversPathListCount = orderToRun.Count;
                }
            }

            catch (Exception ex)
            {
                GetExceptionMessage(ex);
            }
            return driversPathListCount;
        }

        /// <summary>
        /// just another check to be sure driver is installed
        /// this will run after there is a reboot so is mostly for validation of firmware installs
        /// GetData.IsInstalledAfterReboot();
        /// </summary>
        internal static string IsInstalledAfterReboot(string line)
        {
            string result = "pass";
            string returnCode = "pass";
            try
            {
                // get the file attributes for file or directory
                FileAttributes attr = File.GetAttributes(line);
                // detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    foreach (string tmpPath in Directory.EnumerateFiles(line))
                    {
                        if (tmpPath.EndsWith(".inf"))
                        {
                            line = tmpPath;
                        }
                        else { continue; }
                    }
                }

                string TMPexpectedVersion = GetDriverVersion(line);
                string TMPinfName = Path.GetFileNameWithoutExtension(line);
                string TMPhardwareID = FirmwareInstallGetHID(line);
                string classGUID = GetClassGUID(line);
                int TMPinfListCount = XMLReader.GetInfsPathListCount(Program.InputTestFilePathBAK);
                int executionCount = XMLReader.GetExecutionCount(Program.InputTestFilePath);
                bool TMPisCapsule = CheckDriverIsFirmware(line, executionCount, TMPinfListCount);
                bool stopOnError = XMLReader.GetStopOnError(Program.InputTestFilePathBAK);

                if (TMPisCapsule)
                {
                    string CapsuleDidInstall = GetDataFromReg.CheckRegCapsuleIsInstalled(TMPinfName, TMPhardwareID, TMPexpectedVersion, line);
                    if (stopOnError.Equals(true))
                    {
                        switch (returnCode)
                        {
                            case string failed when CapsuleDidInstall.Equals("unsuccessful"):
                                returnCode = "unsuccessful registry code is 1 ";
                                XMLErrorMessage(returnCode, line);
                                break;

                            case string InsufficientResources when CapsuleDidInstall.Equals("InsufficientResources"):
                                returnCode = "InsufficientResources registry code is 2 ";
                                XMLErrorMessage(returnCode, line);
                                break;

                            case string IncorrectVersion when CapsuleDidInstall.Equals("IncorrectVersion"):
                                returnCode = "IncorrectVersion registry code is 3 ";
                                XMLErrorMessage(returnCode, line);
                                break;

                            case string invalidImage when CapsuleDidInstall.Equals("invalidImage"):
                                returnCode = "invalidImage registry code is 4 ";
                                XMLErrorMessage(returnCode, line);
                                break;

                            case string authenticationERR when CapsuleDidInstall.Equals("authenticationERR"):
                                returnCode = "authenticationERR registry code is 5 ";
                                XMLErrorMessage(returnCode, line);
                                break;

                            case string ACnotConnected when CapsuleDidInstall.Equals("ACnotConnected"):
                                returnCode = "ACnotConnected registry code is 6 ";
                                XMLErrorMessage(returnCode, line);
                                break;

                            case string insufficientPower when CapsuleDidInstall.Equals("insufficientPower"):
                                returnCode = "insufficientBatteryPower registry code is 7 ";
                                XMLErrorMessage(returnCode, line);
                                break;

                            default:
                                returnCode = "pass";
                                result = returnCode;
                                Logger.Comment("checked the registry after reboot but this firmware installed correctly " + returnCode + " " + line);
                                Console.WriteLine("checked the registry after reboot but this firmware installed correctly " + returnCode + " " + line);
                                break;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                GetExceptionMessage(ex);
                return result;
            }
        }

        /// <summary>
        /// just to not repeat myself 7 times i am putting this here
        /// GetData.XMLErrorMessage(returnCode, line);
        /// </summary>
        /// <param name="returnCode"></param>
        /// <param name="line"></param>
        internal static void XMLErrorMessage(string returnCode, string line)
        {
            string result;
            Logger.Comment("checked the registry after reboot but this firmware seems to have failed to install due to " + returnCode + line);
            Console.WriteLine("checked the registry after reboot but this firmware seems to have failed to install due to " + returnCode + line);
            Logger.Comment("Copy the the driverstress log and DPINST.LOG to our folder...");
            Utilities.CopyFile(@"C:\Windows\DPINST.LOG", Program.dpinstLog);
            Utilities.CopyFile(Program.dirName + @"\DriverCapsuleStressLog.txt", Program.resultsLogDir + @"\DriverCapsuleStressLog.txt");
            XMLWriter.SetRegErrCode(returnCode, line);
            Utilities.CopyFile(Program.InputTestFilePathBAK, Program.resultsLogDir + @"\DriverCapsuleStress.xml");
            result = returnCode;
        }
    }
}
