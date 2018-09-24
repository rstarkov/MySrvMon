using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace MySrvMon
{
    class DiskSpaceModule : Module
    {
        public override string Name => "Disk Space";

        class VolumeConfig
        {
            public string Path; // supports UNC volume paths and paths that go through junctions
            public double WarnBelowGB;
            public double RedAlertBelowGB;
        }

        private List<VolumeConfig> Volumes = new List<VolumeConfig>();

        protected override ExecuteResult ExecuteCore()
        {
            var result = new ExecuteResult();

            var volumes = convert(new ManagementObjectSearcher("select * from Win32_Volume").Get());
            var table = new TextTable();
            table.HeaderRows = 1;
            table.SetCell(0, 0, "Path".Color(ConsoleColor.White));
            table.SetCell(1, 0, "Volume".Color(ConsoleColor.White));
            table.SetCell(2, 0, "Free space".Color(ConsoleColor.White), alignment: HorizontalTextAlignment.Right);
            table.ColumnSpacing = 3;
            int nextRow = 1;
            foreach (var volume in Volumes)
            {
                var volumeName = new string(new char[1024]);
                try
                {
                    GetVolumePathName(volume.Path, volumeName, volumeName.Length);
                    volumeName = volumeName.Substring(0, volumeName.IndexOf('\0'));
                }
                catch
                {
                    throw new Exception($"Unable to retrieve volume name for path {volume.Path}");
                }

                double spaceGb;
                try
                {
                    var vol = volumes.Single(v => (string) v["Name"] == volumeName || (string) v["DeviceID"] == volumeName);
                    volumeName = (string) vol["Name"];
                    spaceGb = ((ulong) vol["FreeSpace"]) / 1_000_000_000.0;
                }
                catch
                {
                    throw new Exception($"Unable to retrieve free space for volume {volumeName}, path {volume.Path}");
                }

                var status = spaceGb < volume.RedAlertBelowGB ? Status.RedAlert : spaceGb < volume.WarnBelowGB ? Status.Warning : Status.Healthy;
                table.SetCell(0, nextRow, volume.Path);
                table.SetCell(1, nextRow, volumeName);
                table.SetCell(2, nextRow, $"{spaceGb:#,0.0} GB".Color(status.GetConsoleColor()), alignment: HorizontalTextAlignment.Right);
                nextRow++;
                result.UpdateStatus(status);
            }
            result.ConsoleReport += table.ToColoredString();

            return result;
        }

        private static List<Dictionary<string, object>> convert(ManagementObjectCollection coll)
        {
            return coll.OfType<ManagementObject>().Select(obj => obj.Properties.OfType<PropertyData>().ToDictionary(pd => pd.Name, pd => pd.Value)).ToList();
        }

        [DllImport("kernel32.dll", EntryPoint = "GetVolumePathNameW")]
        private extern static bool GetVolumePathName([MarshalAs(UnmanagedType.LPWStr)]string path, [MarshalAs(UnmanagedType.LPWStr)] string volumePath, int volumePathSize);
    }
}
