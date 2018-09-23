using System;
using System.IO;
using System.Linq;
using System.Reflection;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Serialization;

namespace MySrvMon
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "--post-build-check")
                return Ut.RunPostBuildChecks(args[1], Assembly.GetExecutingAssembly());

            var settingsFile = PathUtil.AppPathCombine("MySrvMon.xml");
            if (!File.Exists(settingsFile))
            {
                var sample = new Settings();
                sample.Modules.Add(new SmartModule());
                ClassifyXml.SerializeToFile(sample, settingsFile + ".sample");
                Console.WriteLine("Sample settings file saved: " + settingsFile + ".sample");
                Console.WriteLine("Edit and rename to " + settingsFile);
                return 1;
            }
            var settings = ClassifyXml.DeserializeFile<Settings>(settingsFile);
            ClassifyXml.SerializeToFile(settings, settingsFile + ".rewrite"); // for SMTP password encryption

            foreach (var module in settings.Modules)
                module.Execute();

            foreach (var module in settings.Modules.OrderByDescending(m => m.Status))
            {
                ConsoleColoredString report =
                    "===========================\r\n" +
                    "=== " + module.Name + "\r\n" +
                    "===========================\r\n\r\n";
                report = report.Color(module.Status.GetConsoleColor());
                report += module.ConsoleReport + "\r\n";
                ConsoleUtil.Write(report);
            }

            var worstStatus = settings.Modules.Max(v => v.Status);
            return (int) worstStatus;
        }

#if DEBUG
        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            Classify.PostBuildStep<Settings>(rep);
        }
#endif
    }
}
