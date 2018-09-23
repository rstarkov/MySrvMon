using System.Reflection;
using RT.Util;
using RT.Util.Serialization;

namespace MySrvMon
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 2 && args[0] == "--post-build-check")
                return Ut.RunPostBuildChecks(args[1], Assembly.GetExecutingAssembly());

            return 0;
        }

#if DEBUG
        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            Classify.PostBuildStep<Settings>(rep);
        }
#endif
    }
}
