using System.Collections.Generic;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace MySrvMon
{
    class Settings
    {
        public List<Module> Modules = new List<Module> { };
        public List<EmailRecipient> EmailRecipients = new List<EmailRecipient> { new EmailRecipient() };
        public SmtpSettings SmtpSettings = new SmtpSettings();
    }

    class SmtpSettings : RTSmtpSettings
    {
        public string From = "mysrvmon@example.com";

        private static byte[] _key = "cbe1fe22283c8d5a7d54dc38a19255ed2b275fb91b33d694604a9b00cb10112e".FromHex(); // exactly 32 bytes
        protected override string DecryptPassword(string encrypted) => SettingsUtil.DecryptPassword(encrypted, _key);
        protected override string EncryptPassword(string decrypted) => SettingsUtil.EncryptPassword(decrypted, _key);
    }

    class EmailRecipient
    {
        public string Email = "bob@example.com";
        public bool SendExecutionFailed = false;
        public bool SendWarning = false;
        public bool SendHealthy = false;
        public List<string> IgnoreModules = new List<string>();
#warning TODO: better rules
    }
}
