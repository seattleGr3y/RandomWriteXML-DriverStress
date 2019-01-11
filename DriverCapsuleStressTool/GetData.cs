using Microsoft.HWSW.Test.Utilities;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO.Compression;

namespace DriverCapsuleStressTool
{
    class GetData
    {
        /// <summary>
        /// uses a list of known good driver names to use as friendly name to put in the XML
        /// which is later used to check if the driver is installed or not
        /// </summary>
        /// <param name="infName"></param>
        /// <returns></returns>
        internal static string FindFriendlyNameInCSV(string infName)
        {
            string result = string.Empty;
            string friendlyDriverName;
            // get data from CSV file to associate friendly name to inf name 
            // to correctly find if is installed or not
            var path = Program.dirName + @"\DeviceName-InfName.csv"; // Habeeb, "Dubai Media City, Dubai"
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                //csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    string DeviceName = fields[0];
                    string CsvInfName = fields[1].Split('.')[0];

                    if (CsvInfName.Equals(infName))
                    {
                        friendlyDriverName = DeviceName;
                        result = friendlyDriverName;
                        Logger.Comment("this is the matching friendlyName from the CSV file : " + result);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// get list of INFs that we might expect so we don't try installing each INF from a
        /// package that could have many but only require one to be installed
        /// List<string> infsList = GetInfNameFromCSV(dirName);
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        internal static List<string> GetInfNameFromCSV(string dirName)
        {
            List<string> infsList = new List<string>();

            var path = dirName + @"\DeviceName-InfName.csv";
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    string DeviceName = fields[0];
                    string CsvInfName = fields[1].Split('.')[0];
                    infsList.Add(CsvInfName);
                }
                return infsList;
            }
        }
        
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
            Logger.FunctionEnter();
            string getVersion = "DriverVer";
            //string getVersion2 = "DriverVer = ";
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
            Logger.FunctionEnter();
            string getClassGuid = "ClassGUID=";
            string getClassGuid2 = "ClassGuid=";
            string classGUID = string.Empty;
            string[] textInput;
            string result = string.Empty;
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
            string getVersion = "DriverVer";
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
            string getIsFirmware = "Class";
            bool result = false;
            string[] infFileContent = File.ReadAllLines(line);

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
                infName = infName.Replace("Surface", "").ToLower();
                string rbInfName = Path.GetFileNameWithoutExtension(line).ToLower();
                string rollbackINFnameDIR = @"\" + rbInfName;
                string fullRollBackDir = (Program.rollbackLine + rollbackINFnameDIR).ToLower();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Does the Rollbacks directory exist?");
                Console.ForegroundColor = ConsoleColor.White;
                //Console.ReadKey();
                string rollbackExists = CheckRollbacksExist(line, infName);

                if(rollbackExists.Equals(null))
                {
                    Console.WriteLine("No Rollbacks exist...create-copy now...");
                    //Console.ReadKey();
                    Directory.CreateDirectory(fullRollBackDir);
                    string dirToCopy = @"C:\Windows\System32\DriverStore\FileRepository\";
                    foreach (string dirToTest in Directory.EnumerateDirectories(dirToCopy))
                    {
                        //if (Directory.Exists(fullRollBackDir)) { break; }
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
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("copying files : " + file);
                                    Console.ForegroundColor = ConsoleColor.White;
                                    file.CopyTo(temppath, false);
                                    Console.WriteLine("should be copying backups next...");
                                    //Console.ReadKey();
                                }
                            }
                            //Utilities.DirectoryCopy(dirToTest, fullRollBackDir, true);
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
            string result = string.Empty;
           // string infName = "surfaceuefi1010";
            infName = infName.Split('.')[0].ToLower();
            infName = infName.Replace("surface", "");
            infName = Regex.Replace(infName, @"[\d-]", string.Empty);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("infName check if rollbackexists : " + infName);
            Console.WriteLine("line : " + line);
            Console.WriteLine("Program.rollbackLine : " + Program.rollbackLine);
            Console.ForegroundColor = ConsoleColor.White;
            
            var infFiles = Directory.EnumerateFiles(Program.rollbackLine, "*.inf", System.IO.SearchOption.AllDirectories);
            foreach (string file in infFiles)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("file check if rollback exists : " + file);
                Console.WriteLine("infName check if rollback exists : " + infName);
                Console.ForegroundColor = ConsoleColor.White;

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
                if (!Directory.Exists(crashDumpPath)) { Directory.CreateDirectory(crashDumpPath); }
                Utilities.CopyFile(memoryDumpFile, newMemDumpFileDIR);
            }
            else if (Directory.Exists(miniDumpPath)) 
            {
                miniDumpFiles = Directory.GetFiles(miniDumpPath, "*.DMP");

                if (Directory.Exists(miniDumpPath))
                {
                    Directory.CreateDirectory(miniDumpArchive);
                }

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
            Logger.FunctionLeave();
            return crashDumpOccurred;
        }

        /// <summary>
        /// get the list of infs from the folders present in the 'support folder' where the executable
        /// will be running from these are used to add to the XML
        /// </summary>
        /// <param name="supportFolderLocation"></param>
        /// <returns></returns>
        internal static List<string> GetInfPathsList(string supportFolderLocation)
        {
            List<string> infPathList = new List<string>();
            List<string> infsList = GetInfNameFromCSV(Program.dirName);
            #region TRIED TO IMPLEMENT ENUMERATE ZIP FILES...NOT WORKING FOR SOME REASON 
            //var checkForZIP = Directory.EnumerateFileSystemEntries(Program.dirName);
            //foreach (string checkedEntry in checkForZIP)
            //{
            //    if (checkedEntry.EndsWith(".zip"))
            //    {
            //        ZipFile.ExtractToDirectory(checkedEntry, Program.dirName);
            //    }
            //}
            #endregion
            var infFiles = Directory.EnumerateFiles(supportFolderLocation, "*.inf", System.IO.SearchOption.AllDirectories);
            foreach (string infFile in infFiles)
            {
                string infDir = Path.GetDirectoryName(infFile);
                string infRealName = Path.GetFileNameWithoutExtension(infFile);
                string infPathTMP = Path.GetFullPath(infFile);

                if (infPathTMP.Contains("Rollbacks"))
                {
                    continue;
                }
                if (infsList.Contains(infRealName))
                {
                    infPathList.Add(infFile);
                }
                else if (infFile.Contains("Surface"))
                {
                    infPathList.Add(infFile);
                }
            }
            return infPathList;
        }

        /// <summary>
        /// get count of drivers to be installed
        /// int driversPathListCount = GetPathListCount(supportFolderLocation);
        /// </summary>
        /// <param name="supportFolderLocation"></param>
        /// <returns></returns>
        internal static int GetPathListCount(string supportFolderLocation)
        {
            List<string> infPathList = new List<string>();
            int driversPathListCount = 0;

            var infFiles = Directory.EnumerateFiles(supportFolderLocation, "*.inf", System.IO.SearchOption.AllDirectories);
            foreach (string infFile in infFiles)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(driversPathListCount);
                Console.ForegroundColor = ConsoleColor.White;

                string infDir = Path.GetDirectoryName(infFile);
                string infRealName = Path.GetFileNameWithoutExtension(infFile);
                string infPathTMP = Path.GetFullPath(infFile);

                if (infPathTMP.Contains("Rollbacks"))
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
            return driversPathListCount;
        }
    }
}
