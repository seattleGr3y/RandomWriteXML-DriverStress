using Microsoft.HWSW.Test.Utilities;
using System;
using System.IO;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;

namespace DriverCapsuleStressTool
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
                                           "HardwareID",
                                           "DriverDate"
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
                                              WmiObject["HardwareID"],
                                              WmiObject["DriverVersion"],
                                              WmiObject["DriverDate"]
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
        internal static bool CheckInstalled(string line, string hardwareID, string friendlyDriverName, string infNameToTest, string expectedDriverVersion, string expectedDriverDate)
        {
            Logger.FunctionEnter();
            string installedDriversCSVPath = Program.dirName + @"\installledDrivers.csv";
            string csvFileName = "DeviceName-InfName.czv";
            string csvFileFullPath = Program.dirName + csvFileName;
            string infName = string.Empty;
            string infNameFromReg = string.Empty;
            string installedDriverVersion = string.Empty;
            string installedDeviceName = string.Empty;
            bool result = false;
            string classGUID = GetData.GetClassGUID(line);
            if (string.IsNullOrEmpty(hardwareID))
            {
                //do nothing for now
                hardwareID = "0x00000000";
                Console.WriteLine("not getting a hardwareID...??");
                Logger.Comment("no hardwareID in the INF so we can't use it to check if driver is installed");
            }
            else
            {
                infNameFromReg = GetDataFromReg.GetOEMinfNameFromRegSTR(infName, hardwareID, classGUID);
            }
            try
            {
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

                        // there are zeros in driver versions in inf files that are not in the devMgr
                        // and viceVersa will have to remove\replace all zeros with ""
                        if (expectedDriverVersion.EndsWith("0"))
                        {
                            expectedDriverVersion = expectedDriverVersion.TrimEnd('0');
                            expectedDriverVersion = expectedDriverVersion.TrimEnd('.');
                            expectedDriverVersion = expectedDriverVersion.TrimEnd('0');
                        }

                        if (installedDriverVersion.EndsWith("0"))
                        {
                            installedDriverVersion = installedDriverVersion.TrimEnd('0');
                            installedDriverVersion = installedDriverVersion.TrimEnd('.');
                            installedDriverVersion = installedDriverVersion.TrimEnd('0');
                        }

                        if (installedDeviceName.Contains(" ")) { installedDeviceName = installedDeviceName.Replace(" ", ""); }
                        if (infName.Contains(" ")) { infName = infName.Replace(" ", ""); }
                        if (infNameToTest.Contains(" ")) { infNameToTest = infNameToTest.Replace(" ", ""); }
                        if (friendlyDriverName.Contains(" ")) { friendlyDriverName = friendlyDriverName.Replace(" ", ""); }
                        infNameToTest = infNameToTest.Split('.')[0];
                        if (infNameToTest.Equals("SurfaceEC")) { infNameToTest = "SurfaceEmbeddedControllerFirmware"; }

                        string lineContains = string.Empty;

                        switch (lineContains)
                        {
                            case string infNameFromR when infName.Equals(infNameFromReg):
                                if (expectedDriverVersion.Equals(installedDriverVersion))
                                {
                                    Logger.Comment(infNameToTest);
                                    result = true;
                                    Logger.Comment("result from checkInstalled TRUE infName/version : ");
                                }
                                break;

                            case string installedDeviceNam when friendlyDriverName.Equals(installedDeviceName):
                                if (expectedDriverVersion.Equals(installedDriverVersion))
                                {
                                    Logger.Comment(infNameToTest);
                                    result = true;
                                    Logger.Comment("result from checkInstalled TRUE infName/version : ");
                                }
                                break;

                            case string failedResult when Regex.Match(infName, friendlyDriverName).Success:
                                if (expectedDriverVersion.Equals(installedDriverVersion))
                                {
                                    Logger.Comment(infNameToTest);
                                    result = true;
                                    Logger.Comment("result from checkInstalled TRUE infName/version : ");
                                }
                                break;

                            case string successinstallResult when infName.Equals(infNameFromReg):
                                if (expectedDriverVersion.Equals(installedDriverVersion))
                                {
                                    Logger.Comment(infNameToTest);
                                    result = true;
                                    Logger.Comment("result from checkInstalled TRUE infName/version : ");
                                }
                                break;

                            case string last when installedDeviceName.Contains(infNameToTest):
                                if (expectedDriverVersion.Equals(installedDriverVersion))
                                {
                                    Logger.Comment(infNameToTest);
                                    result = true;
                                    Logger.Comment("result from checkInstalled TRUE infName/version : ");
                                }
                                break;

                            default:
                                continue;
                        }
                    }
            }

                catch (Exception ex)
                {
                    GetData.GetExceptionMessage(ex);
                }
            Logger.Comment("result actually being returned from checkInstalled : " + result);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("result actually being returned from checkInstalled : " + result);
            Console.ForegroundColor = ConsoleColor.White;
            Logger.FunctionLeave();
            return result;
        }
    }
}
