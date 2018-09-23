using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySrvMon
{
    static class Extensions
    {
        public static Status WorstStatus(this Status status, Status newStatus)
        {
            return status > newStatus ? status : newStatus;
        }

        public static ConsoleColor GetConsoleColor(this Status status)
        {
            return status == Status.Healthy ? ConsoleColor.Green : status == Status.Warning ? ConsoleColor.Yellow : status == Status.RedAlert ? ConsoleColor.Red : ConsoleColor.Magenta;
        }
    }
}
