using System;
using RT.TagSoup;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.Serialization;

namespace MySrvMon
{
    enum Status
    {
        Healthy,
        Warning,
        RedAlert,
        ExecutionFailed,
    }

    abstract class Module
    {
        protected class ExecuteResult
        {
            public Status Status { get; private set; }
            public ConsoleColoredString ConsoleReport;
            public object HtmlReport;

            public void UpdateStatus(Status status)
            {
                if (Status < status)
                    Status = status;
            }
        }

        [ClassifyIgnore]
        private ExecuteResult _result;

        public abstract string Name { get; }
        protected abstract ExecuteResult ExecuteCore();

        public Status Status => _result == null ? throw new InvalidOperationException() : _result.Status;
        public ConsoleColoredString ConsoleReport => _result == null ? throw new InvalidOperationException() : _result.ConsoleReport;
        public object HtmlReport => _result == null ? throw new InvalidOperationException() : _result.HtmlReport;

        public void Execute()
        {
#if !DEBUG
            try
#endif
            {
                _result = ExecuteCore();
            }
#if !DEBUG
            catch (Exception e)
            {
                _result = new ExecuteResult
                {
                    Status = Status.ExecutionFailed,
                    ConsoleReport = $"ERROR: {e.Message} ({e.GetType().Name})".Color(ConsoleColor.Red),
                    HtmlReport = new P(new B("ERROR: "), $"{e.Message} ({e.GetType().Name})"),
                };
            }
#endif
        }
    }
}
