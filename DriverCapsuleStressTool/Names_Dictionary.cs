using System;
using System.Collections.Generic;

namespace DriverCapsuleStressTool
{
    class Names_Dictionary
    {
        #region - USE THIS INSTEAD OF A CSV FILE
        /// <summary>
        /// Dictionary with known Driver names matched with their actual INF name 
        /// to eliminate execution of multiple unnecessary INF installs
        /// Names_Dictionary.GetNamesMatch(infName);
        /// </summary>
        /// <param name="line"></param>
        internal static string GetNamesMatch(string infName)
        {
            List<KeyValuePair<string, string>> driverInfNames = new List<KeyValuePair<string, string>>();

            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R)Serial IO GPIO Driver", "iaLPSS2_GPIO2_SKL.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R)Serial IO UART Driver", "iaLPSS2_UART2_SKL.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R)Imaging Signal Processor 2500", "iaisp64.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Audio SST OED", "IntcOED.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) AVStream Camera 2500", "iacamera64.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) CIO2 Host Controller", "CSI2HostControllerDriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Control Logic ", "iactrllogic64.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Display Audio Driver", "IntcDAud.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Display Graphics Adapter Driver ", "64jp6136.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Dynamic Platform and Thermal Framework (DPTF) ", "dptf_cpu.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Dynamic Platform Thermal Framework (DPTF) ", "dptf_acpi.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Graphics Display Driver ", "64gh6136.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Imaging Signal Processor 2500 ", "iaisp64.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Management Engine Interface", "heci.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Microsoft Camera Front ", "ov5693.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Microsoft Camera IR", "ov7251.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Microsoft Camera Rear", "ov8865.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Precise Touch ", "iaPreciseTouch.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Serial IO 12C Host Controller Driver (LPSS) ", "iaLPSS2_I2C_SKL.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Serial IO GPIO Driver ", "iaLPSS2_GPIO2_SKL.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Serial IO GPIO Host Controller ", "iaLPSS2_GPIO2_SKL.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Serial IO I2C Host Controller ", "iaLPSS2_I2C_SKL.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Serial IO UART Driver ", "iaLPSS2_UART2_SKL.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Smart Sound Technology (SST) Audio Bus ", "IntcAudioBus.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Smart Sound Technology (SST) OED", "IntcOED.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) SST Audio Bus", "IntcAudioBus.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) SST Audio Bus ", "IntcAudioBus.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) HD Graphics 615", "iigd_ext.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) SST Audio OED ", "IntcOED.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) CSI2 Host Controller", "csi2hostcontrollerdriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) HD Graphics 620", "64jp4840.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Dynamic Platform and Thermal Framework Processor Participant", "dptf_acpi.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Dynamic Platform and Thermal Framework Manager", "esif_manager.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Control Logic", "iactrllogic64.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Imaging Signal Processor 2500", "iaisp64.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Serial IO GPIO Host Controller - INT344B", "ialpss2_gpio2_skl.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Serial IO I2C Host Controller - 9D64", "ialpss2_i2c_skl.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Serial IO UART Host Controller - 9D27", "ialpss2_uart2_skl.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Precise Touch Device", "iaprecisetouch.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Smart Sound Technology (Intel(R) SST) Audio Controller", "intcaudiobus.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Display Audio", "intcdaud.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Smart Sound Technology (Intel(R) SST) OED", "intcoed.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Intel(R) Integrated Sensor Solution", "ish.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Realtek High Definition Audio Driver", "HDXSSTM.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("SAR - Surface Radio Monitor", "SarProxy.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Accessory Device Service", "SurfaceAccessoryDevice.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface ACPI Notify Driver ", "SurfaceAcpiNotifyDriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface ACPI-Compliant Control Method Battery ", "SurfaceACPIBattery.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Display Color Calibration ", "SurfaceDisplayColor.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Integration Service Null Driver [SIS]", "SurfaceServiceNullDriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface PTP Filter Driver", "SurfacePTPFilter.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Serial Hub Driver", "SurfaceSerialHubDriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Storage FW Update Driver", "SurfaceStorageFwUpdate.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("System (Gemalto eSIM Firmware Update) ", "GtoPatchDrvUMDF.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Dial Detection", "SurfaceDialDetection.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Dial Filter", "SurfaceDialFilter.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Digitizer Integration", "SurfaceDigitizerIntegration.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Pen Pairing", "SurfacePenPairing.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Pen Settings", "SurfacePenDriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Digitizer Integration Driver", "SurfaceDigitizerIntegration.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Dial Filter Driver", "SurfaceDialFilter.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Dial Detection ", "SurfaceDialDetection.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface SAM", "SurfaceSam.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface SAM", "SurfaceSAM.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface UEFI", "SurfaceUEFI.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Touch", "SurfaceTouch.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Touch Servicing ML", "SurfaceTouchServicingML.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("OEMAP_UEFI", "OEMAP_UEFI.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Integration Driver", "SurfaceIntegrationDriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Embedded Controller", "SurfaceEC.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Marvell AVASTAR Wireless-AC Network Controller", "mrvlpcie8897.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Marvell AVASTAR Bluetooth Radio Adapter", "mbtr8897w81x64.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface1832DigitizerIntegration Device", "digitizerintegration.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Accessory Bluetooth Pairing", "accessorybluetoothpairing.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Realtek High Definition Audio(SST)", "hdxsstm.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("HID PCI Minidriver for ISS", "hid_pci.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Maxim Power Meter", "maximpowermeter.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("ISS Dynamic Bus Enumerator", "ish_busdriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Xbox Wireless Adapter for Windows", "mt7612us_br.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("NVIDIA GeForce GPU", "nvmso.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Camera Sensor OV5693", "ov5693.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Camera Sensor OV7251", "ov7251.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Camera Sensor OV8865", "ov8865.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Mobile 6th/7th Generation Intel(R) Processor Family I/O PCI Express Root Port #1 - 9D10", "sunrisepoint-lpsystem.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Mobile 6th/7th Generation Intel(R) Processor Family I/O Northpeak - 9D26", "sunrisepoint-lpsystemnorthpeak.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Mobile 6th/7th Generation Intel(R) Processor Family I/O Thermal subsystem - 9D31", "sunrisepoint-lpsystemthermal.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Accessory Firmware Update", "surfaceaccessoryfwupdate.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface ACPI Notify Driver", "surfaceacpinotifydriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Base 2 Integration", "surfacebase2integration.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Button", "surfacebutton.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Display", "surfacedisplay.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Display Color", "surfacedisplaycolor.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Dock Firmware Update", "surfacedockfwupdate.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Dock Integration", "surfacedockintegration.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface DTX", "surfacedtxdriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Hot Plug", "surfacehotplug.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Integration", "surfaceintegrationdriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Integrated Sensor Hub", "surfaceish.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Keyboard Backlight", "surfacekeyboardbacklight.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface ME", "surfaceme.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface NVM Express Controller", "surfacenvmexpresscontroller.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Power Meter", "surfacepowermeterdriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface System Aggregator", "surfacesam.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Radio Monitor", "surfacesarmanager.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Integration Service Device", "surfaceservicenulldriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Software Servicing", "surfacesoftwareservicingdriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Storage Firmware Update", "surfacestoragefwupdate.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface System Telemetry", "surfacesystemtelemetrydriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Tcon Device", "surfacetcondriver.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Ucsi Device", "surfaceucsi.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface USB Hub Firmware Update", "surfaceusbhubfwupdate.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface Power Delivery", "RtkFWCapsule.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Surface CIF", "SurfaceCIF.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Qualcomm Atheros QCA61x4 Bluetooth 4.1", "atheros_bth.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Qualcomm Atheros QCA61x4 Wireless Network Adapter", "netathr10x.inf"));
            driverInfNames.Add(new KeyValuePair<string, string>("Generic IO & Memory Access", "QIOMEM.inf"));

        string result = string.Empty;

            foreach (KeyValuePair<string, string> infInfo in driverInfNames)
            {
                if (infInfo.Value.Contains(infName))
                {
                    string driverName = infInfo.Key;
                    result = driverName;
                }
            }
            return result;
        }
        #endregion
    }
}
