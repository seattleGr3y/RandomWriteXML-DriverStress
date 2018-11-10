using Microsoft.HWSW.Test.Utilities;
using System;
using System.IO;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace RandomWriteXML
{
    class CheckWhatInstalled
    {
        private static readonly string csvFileName = @".\InstalledDrivers.csv";
        private static readonly string extension = Path.GetExtension(csvFileName);

        /// <summary>
        /// check for existing csv so we can increment to the post update process version
        /// this way we don't overwrite and will be able to compare pre and post for existing drivers
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetNextFileName(string fileName)
        {
            Logger.FunctionEnter();
            string extension = Path.GetExtension(csvFileName);

            if (File.Exists(fileName))
            {
                fileName = fileName.Replace(extension, "(PostUpdate)" + extension);
            }
            else
            {
                Logger.Comment("file does not yet exist to increment name...continue");
                fileName = csvFileName;
            }
            Logger.FunctionLeave();
            return fileName;
        }

        /// <summary>
        /// wmi calls to see what is installed on the device and write a CSV
        /// </summary>
        internal static void CheckInstalledCSV()
        {
            Logger.FunctionEnter();
            try
            {
                StringBuilder installedDrivers = new StringBuilder();

                /// write to a CSV
                /// headers for csv file
                var header = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"",
                                           "InfName",
                                           "DeviceName",
                                           "DriverVersion",
                                           "DriverDate",
                                          "HardWareID"
                                          );
                installedDrivers.AppendLine(header);

                using (ManagementObjectSearcher s =
                        new ManagementObjectSearcher(
                        "root\\CIMV2",
                        "SELECT * FROM Win32_PnPSignedDriver"))

                    foreach (ManagementObject WmiObject in s.Get())
                    {
                        var listResults = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"",
                                              WmiObject["InfName"],
                                              WmiObject["DeviceName"],
                                              WmiObject["DriverVersion"],
                                              WmiObject["DriverDate"],
                                              WmiObject["HardWareID"]
                                             );
                        installedDrivers.AppendLine(listResults);
                    }
                var newFileName = GetNextFileName(csvFileName);
                File.WriteAllText(newFileName, installedDrivers.ToString());

            }
            catch (Exception ex)
            {
                GetData.GetExceptionMessage(ex);
            }
            Logger.FunctionLeave();
        }

        /// <summary>
        /// check if the driver we are about to install has already been installed
        /// if it has and is firmware then rollback if is not then just uninstall
        /// </summary>
        /// <param name="infNameToTest"></param>
        /// <param name="expectedDriverVersion"></param>
        /// <returns></returns>
        internal static bool CheckInstalled(string friendlyDriverName, string infNameToTest, string expectedDriverVersion, string expectedDriverDate)
        {
            string infName = string.Empty;
            string installedDriverVersion = string.Empty;
            string installedDeviceName = string.Empty;
            DateTime installedDriverDate;
            DateTime expDriverDate;
            string driverDate = string.Empty;
            bool result = false;
            Logger.FunctionEnter();
            using (ManagementObjectSearcher s2 =
                    new ManagementObjectSearcher(
                    "root\\CIMV2",
                    "SELECT * FROM Win32_PnPSignedDriver"))


                foreach (ManagementObject WmiObject in s2.Get())
                {
                    if (string.IsNullOrEmpty(infName = WmiObject["InfName"].ToString())) { continue; }
                    if (string.IsNullOrEmpty(installedDriverVersion = WmiObject["DriverVersion"].ToString())) { continue; }
                    if (string.IsNullOrEmpty(installedDeviceName = WmiObject["DeviceName"].ToString())) { continue; }
                    if (string.IsNullOrEmpty(driverDate = WmiObject["DriverDate"].ToString())) { continue; }
                    try
                    {
                        // there are zeros in driver versions in inf files that are not in the devMgr
                        // and viceVersa will have to remove\replace all zeros with ""
                        if (expectedDriverVersion.Contains("0"))
                        {
                            expectedDriverVersion = expectedDriverVersion.Replace("0", "");
                            expectedDriverVersion = expectedDriverVersion.TrimEnd('.');
                        }

                        if (installedDriverVersion.Contains("0"))
                        {
                            installedDriverVersion = installedDriverVersion.Replace("0", "");
                            installedDriverVersion = installedDriverVersion.TrimEnd('.');
                        }

                        if (installedDeviceName.Contains(" ")) { installedDeviceName = installedDeviceName.Replace(" ", ""); }
                        if (infName.Contains(" ")) { infName = infName.Replace(" ", ""); }
                        if (infNameToTest.Contains(" ")) { infNameToTest = infNameToTest.Replace(" ", ""); }
                        if (friendlyDriverName.Contains(" ")) { friendlyDriverName = friendlyDriverName.Replace(" ", ""); }
                        infNameToTest = infNameToTest.Split('.')[0];
                        if (infNameToTest.Equals("SurfaceEC")) { infNameToTest = "SurfaceEmbeddedControllerFirmware"; }

                        string currentDriverDate = driverDate.Split('.')[0].Insert(4, "/").Insert(7, "/").TrimEnd('0');
                        try
                        {
                            installedDriverDate = Convert.ToDateTime(currentDriverDate);
                            expDriverDate = Convert.ToDateTime(expectedDriverDate);
                        }
                        catch
                        {
                            Console.WriteLine("this date is formatted badly : " + currentDriverDate);
                            Console.WriteLine("or is this date formatted badly? : " + expectedDriverDate);
                            Console.WriteLine("....for this driver : " + infNameToTest);
                            //Console.ReadKey();
                            continue;
                        }

                        if (installedDriverDate == expDriverDate)
                        {
                            Console.WriteLine("the dates are equal is this the right driver?");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("the infNameToTest : " + infNameToTest);
                            Console.WriteLine("friendlyDriverName : " + friendlyDriverName);
                            Console.WriteLine("infName : " + infName);
                            Console.WriteLine("installedDeviceName : " + installedDeviceName);
                            Console.ForegroundColor = ConsoleColor.White;
                            //Console.ReadKey();

                            if (infName.Equals(infNameToTest))
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE by date/name");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by date/name");
                                Console.ForegroundColor = ConsoleColor.White;
                                //Console.ReadKey();
                                break;
                            }
                            else if (Regex.Match(installedDeviceName, infNameToTest, RegexOptions.IgnoreCase).Success)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE by date/name");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by date/name");
                                Console.ForegroundColor = ConsoleColor.White;
                                //Console.ReadKey();
                                break;
                            }
                        }

                        else if (Regex.Match(infName, friendlyDriverName).Success)
                        {
                            if (expectedDriverVersion.Equals(installedDriverVersion))
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE friendlyName/version : ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by friendlyName/version");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                break;
                            }
                            else if (installedDriverDate == expDriverDate)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE friendlyName/date : ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by friendlyName/version");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                break;
                            }
                            else { continue; }
                        }

                        else if (Regex.Match(infName, infNameToTest).Success)
                        {
                            if (expectedDriverVersion.Equals(installedDriverVersion))
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE name/version : ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by name/version");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                break;
                            }
                            else if (installedDriverDate == expDriverDate)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE name/date : ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by name/date");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                break;
                            }
                            else { continue; }
                        }

                        else if (Regex.Match(friendlyDriverName, infNameToTest).Success)
                        {
                            if (expectedDriverVersion.Equals(installedDriverVersion))
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE name/version : ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by name/version");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                break;
                            }
                            else if (installedDriverDate == expDriverDate)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE name/date : ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by name/date");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                break;
                            }
                            else { continue; }
                        }
                        else if (installedDeviceName.Equals(infNameToTest))
                        {
                            if (expectedDriverVersion.Equals(installedDriverVersion))
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE deviceName/version : ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by deviceName/version");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                break;
                            }
                            else if (installedDriverDate == expDriverDate)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE deviceName/date : ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by deviceName/date");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                break;
                            }
                            else { continue; }
                        }

                        else if (expectedDriverVersion.Equals(installedDriverVersion))
                        {
                            if (installedDeviceName.Equals(infNameToTest))
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE version/name : ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by version/name");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                break;
                            }
                            else if (installedDeviceName == infNameToTest)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE version/name : ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("result from checkInstalled TRUE by version/name");
                                Console.ForegroundColor = ConsoleColor.White;
                                Thread.Sleep(1000);
                                break;
                            }
                        }
                        else { continue; }
                    }
                    catch (Exception ex)
                    {
                        GetData.GetExceptionMessage(ex);
                    }
                }
            Logger.FunctionLeave();
            Logger.Comment("result actually being returned from checkInstalled : " + result);
            Thread.Sleep(1000);
            return result;
        }
    }
}
