using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Linq;
using System.Text;
using View.CommonStruct;
using View.CommonEnum;
using UPNPLib;
using System.Diagnostics;

namespace View.Manager
{
    internal class DriveManager {

        /// <summary>
        /// USB機器からivdrドライブを取得する
        /// </summary>
        /// <param name="strDriveId"></param>
        /// <returns></returns>
        static public List<DataStruct.Drivemap> GetUsbStorName(string strDriveId) {
            ManagementObjectCollection mocDiskDrives;
            ManagementObjectCollection mocLogicalDisks;
            ManagementObjectCollection mocPartitions;
            ManagementObjectCollection mocUSBControllers;

            string strDriveDeviceID;
            string strUsbPNPDeviceID;
            string strPartitionDeviceID;
            string strLogicalDeviceID;
            string strPNPDeviceID;
            string strUsbBindID;
            string strPNPUsbBindID;
            List<DataStruct.Drivemap> value = new List<DataStruct.Drivemap> {};
            string[] array = strDriveId.Split(',');
            List<string> strDriveIdList = new List<string> (array);

            using (ManagementObjectSearcher mojWMISeacher = new ManagementObjectSearcher(
                                "root\\CIMV2",
                                "SELECT * FROM Win32_DiskDrive WHERE InterfaceType=\'USB\'")) { ///< USBデバイスから検索
                mocDiskDrives = mojWMISeacher.Get();
                foreach (ManagementObject mojDrive in mocDiskDrives) {
                    strDriveDeviceID = mojDrive.GetPropertyValue("DeviceID").ToString();        ///< 実行時、"\\\\.\\PHYSICALDRIVE0" が入る

                    mojWMISeacher.Query.QueryString =
                    "ASSOCIATORS OF {Win32_DiskDrive.DeviceID=\'" +
                    strDriveDeviceID + "\'} WHERE AssocClass = " +
                    "Win32_DiskDriveToDiskPartition";
                    mocPartitions = mojWMISeacher.Get();

                    if (mocPartitions.Count == 0) {
                        string driveType =  mojDrive.GetPropertyValue("PNPDeviceID").ToString();
                        string driveCaption =  mojDrive.GetPropertyValue("Caption").ToString();
                        string driveIndex =  mojDrive.GetPropertyValue("Index").ToString();
                        continue;
                    }
                    foreach (ManagementObject mojPartition in mocPartitions) {
                        strPartitionDeviceID = mojPartition.GetPropertyValue("DeviceID").ToString();
                        mojWMISeacher.Query.QueryString =
                        "ASSOCIATORS OF {Win32_DiskPartition.DeviceID=\'" +
                        strPartitionDeviceID + "\'} WHERE AssocClass = " +
                        "Win32_LogicalDiskToPartition";

                        mocLogicalDisks = mojWMISeacher.Get();
                        if (mocLogicalDisks.Count == 0) continue;
                        foreach (ManagementObject mojLogicakDisk in mocLogicalDisks) {
                            strLogicalDeviceID = mojLogicakDisk.GetPropertyValue("DeviceID").ToString();
                            strPNPDeviceID = mojDrive.GetPropertyValue("PNPDeviceID").ToString();

                            int iYen = strPNPDeviceID.LastIndexOf('\\');
                            string deviceId = strPNPDeviceID.Remove(iYen);
                            strUsbBindID = strPNPDeviceID.Substring(iYen + 1);
                            int iAnd = strUsbBindID.LastIndexOf('&');
                            // もし追加情報があればカットする
                            if (iAnd != -1) {
                                strUsbBindID = strUsbBindID.Remove(iAnd);
                            }
                            DataStruct.UpnpDevice deviceInfo = splitDeviceId(deviceId);
                            mojWMISeacher.Query.QueryString =
                               "SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID Like \'USB\\\\VID%\'";

                            mocUSBControllers = mojWMISeacher.Get();
                            if (mocUSBControllers.Count == 0) continue;
                            strUsbPNPDeviceID = string.Empty;
                            foreach (ManagementObject mojUSBController in mocUSBControllers) {
                                strUsbPNPDeviceID = mojUSBController.GetPropertyValue("PNPDeviceID").ToString();
                                iYen = strUsbPNPDeviceID.LastIndexOf('\\');
                                strPNPUsbBindID = strUsbPNPDeviceID.Substring(iYen + 1);
                                strUsbPNPDeviceID = strUsbPNPDeviceID.Substring(4, iYen-4);
                                if (strUsbBindID.Equals(strPNPUsbBindID)) {
                                    break;
                                }
                            }
                            if (strDriveIdList.IndexOf(strUsbPNPDeviceID) >= 0) {
                                DriveInfo cDrive = new DriveInfo(strLogicalDeviceID);
                                if (cDrive.IsReady && cDrive.DriveFormat.Equals("UDF")) {             ///< UDFか確認
                                    value.Add( new DataStruct.Drivemap(
                                        strLogicalDeviceID, cDrive.DriveFormat, deviceInfo.manufacturerName,
                                        deviceInfo.modelName, deviceInfo.modelNumber) );
                                }
                            }
                        }
                    }
                }
            }
            return value; // 見付からなかった時はEmptyを返す
        }

        // デバイスIDから、メーカ名、モデル名、レビジョンを分解して返す
        static DataStruct.UpnpDevice splitDeviceId(string deviceId) {
            DataStruct.UpnpDevice value = new DataStruct.UpnpDevice();
            string[] list =  deviceId.Split('&');
            foreach(string data in list) {
                if(data.IndexOf("VEN_") >= 0) {
                    value.manufacturerName = data.Replace("VEN_","").Replace("_"," ");
                } else if(data.IndexOf("PROD_") >= 0) {
                    value.modelName = data.Replace("PROD_","").Replace("_"," ");
                } else if(data.IndexOf("REV_") >= 0) {
                    value.modelNumber = data.Replace("REV_","");
                }
            }
            return value;
        }
    }
}
