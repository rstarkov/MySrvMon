using System;
using System.Collections.Generic;
using System.Linq;
using BasicSMART;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;


namespace MySrvMon
{
    class SmartModule : Module
    {
        public override string Name => "S.M.A.R.T. parameters";

        class ReportRule
        {
            public byte AttributeId;
            public int? RawThreshold;
            public Status Severity;
            public List<MediaType> MediaTypeFilter = new List<MediaType>();
        }

        private List<ReportRule> ReportRules = new List<ReportRule> { new ReportRule { AttributeId = 1, RawThreshold = 1, Severity = Status.RedAlert } };

        protected override ExecuteResult ExecuteCore()
        {
            var drives = SMART.GetDrivesWithSMART();

            var result = new ExecuteResult();
            result.ConsoleReport = "";

            foreach (var drive in drives)
            {
                var driveStatus = Status.Healthy;
                ConsoleColoredString smartReport = "";
                bool any = false;
                foreach (var smart in drive.SmartReadings)
                {
                    var reportables = ReportRules.Where(ra => ra.AttributeId == smart.Id && (ra.MediaTypeFilter == null || ra.MediaTypeFilter.Count == 0 || ra.MediaTypeFilter.Contains(drive.MediaType))).ToList();
                    if (reportables.Count == 0)
                        continue;

                    any = true;
                    var statusCur = Status.Healthy;
                    var statusRaw = Status.Healthy;
                    foreach (var reportable in reportables)
                    {
                        if (smart.Worst <= smart.Threshold)
                            statusCur = statusCur.WorstStatus(reportable.Severity);
                        if (reportable.RawThreshold != null && smart.Raw >= reportable.RawThreshold)
                            statusRaw = statusCur.WorstStatus(reportable.Severity);
                    }
                    driveStatus = driveStatus.WorstStatus(statusCur).WorstStatus(statusRaw);
                    result.UpdateStatus(statusCur.WorstStatus(statusRaw));

                    var clrCur = statusCur.GetConsoleColor();
                    var clrRaw = reportables.All(r => r.RawThreshold == null) ? ConsoleColor.Gray : statusRaw.GetConsoleColor();
                    smartReport += $"    {smart.Id:X2}  {smart.Name.SubstringSafe(0, 30),-30}  :  " + smart.Raw.ToString().PadRight(8, ' ').Color(clrRaw) + $" (cur {smart.Current,3}, worst {smart.Worst,3}, thresh {smart.Threshold,3})\r\n".Color(clrCur);
                }

                result.ConsoleReport += drive.Model.Color(ConsoleColor.Cyan) + "   " + $"{drive.Size / 1_000_000_000.0:#,0} GB   ".Color(ConsoleColor.Magenta) + drive.MediaType.ToString().Color(ConsoleColor.White)
                    + $"   s/n: {drive.SerialNumber}\r\n";
                result.ConsoleReport += "SMART prediction: " + (drive.SmartPredictFailure ? "FAILURE IMMINENT".Color(ConsoleColor.Red) : "Healthy".Color(ConsoleColor.Green)) + "\r\n";
                if (drive.SmartPredictFailure)
                    result.UpdateStatus(Status.RedAlert);
                if (any)
                {
                    result.ConsoleReport += "Our prediction: " + driveStatus.ToString().Color(driveStatus.GetConsoleColor()) + "\r\n\r\n";
                    result.ConsoleReport += smartReport;
                }
                else
                {
                    result.ConsoleReport += "\r\n    No relevant SMART attributes found for this drive.\r\n".Color(ConsoleColor.Yellow);
                    result.UpdateStatus(Status.Warning);
                }
                result.ConsoleReport += "\r\n\r\n";
            }

            return result;
        }
    }
}
