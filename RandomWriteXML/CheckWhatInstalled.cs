using Microsoft.HWSW.Test.Utilities;
using System;
using System.IO;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

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
                    if (WmiObject["InfName"] == null) { continue; }
                        else { infName = WmiObject["InfName"].ToString(); }
                    if (WmiObject["DriverVersion"] == null) { continue; }
                        else { installedDriverVersion = WmiObject["DriverVersion"].ToString(); }
                    if (WmiObject["DeviceName"] == null) { continue; }
                        else { installedDeviceName = WmiObject["DeviceName"].ToString(); }
                    if (WmiObject["DriverDate"] == null) { continue; }
                        else { driverDate = WmiObject["DriverDate"].ToString(); }

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
                            continue;
                        }

                        if (installedDriverDate == expDriverDate)
                        {
                            if (infName.Equals(infNameToTest))
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE by date/name");
                                break;
                            }
                            else if (Regex.Match(installedDeviceName, infNameToTest, RegexOptions.IgnoreCase).Success)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE by date/name");
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
                                Thread.Sleep(1000);
                                break;
                            }
                            else if (installedDriverDate == expDriverDate)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE friendlyName/date : ");
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
                                Thread.Sleep(1000);
                                break;
                            }
                            else if (installedDriverDate == expDriverDate)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE name/date : ");
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
                                Thread.Sleep(1000);
                                break;
                            }
                            else if (installedDriverDate == expDriverDate)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE name/date : ");
                                Thread.Sleep(1000);
                                break;
                            }
                            else { continue; }
                        }
                        else if (installedDeviceName.Contains(infNameToTest))
                        {
                            if (expectedDriverVersion.Equals(installedDriverVersion))
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE deviceName/version : ");
                                Thread.Sleep(1000);
                                break;
                            }
                            else if (installedDriverDate == expDriverDate)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE deviceName/date : ");
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
                                Thread.Sleep(1000);
                                break;
                            }
                            else if (installedDeviceName == infNameToTest)
                            {
                                Logger.Comment(infNameToTest);
                                result = true;
                                Logger.Comment("result from checkInstalled TRUE version/name : ");
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

        internal static string CSVParse(string infName)
        {
            string result = string.Empty;
            var path = @"C:\Person.csv"; // Habeeb, "Dubai Media City, Dubai"
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
                    string CsvInfName = fields[1];

                    if (CsvInfName.Equals(infName))
                    {
                        result = DeviceName;
                    }
                }
            }
            return result;
        }
    }
}
