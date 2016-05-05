using System;
using System.IO;
using System.Text;

namespace Swartz.Environment.Configuration
{
    public class ShellSettingsSerializer
    {
        public const char Separator = ':';
        public const string EmptyValue = "null";

        public static ShellSettings ParseSettings(string text)
        {
            var shellSettings = new ShellSettings();
            if (string.IsNullOrWhiteSpace(text))
            {
                return shellSettings;
            }

            var settings = new StringReader(text);
            string setting;
            while ((setting = settings.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(setting)) continue;
                var separatorIndex = setting.IndexOf(Separator);
                if (separatorIndex == -1)
                {
                    continue;
                }
                var key = setting.Substring(0, separatorIndex).Trim();
                var value = setting.Substring(separatorIndex + 1).Trim();

                if (!value.Equals(EmptyValue, StringComparison.OrdinalIgnoreCase))
                {
                    switch (key)
                    {
                        case "Name":
                            shellSettings.Name = value;
                            break;
                        case "DataProvider":
                            shellSettings.DataProvider = value;
                            break;
                        case "Host":
                            shellSettings.Host = value;
                            break;
                        case "User":
                            shellSettings.User = value;
                            break;
                        case "Password":
                            shellSettings.Password = value;
                            break;
                        case "Database":
                            shellSettings.Database = value;
                            break;
                        case "Port":
                            shellSettings.Port = value;
                            break;
                        case "EncryptionAlgorithm":
                            shellSettings.EncryptionAlgorithm = value;
                            break;
                        case "EncryptionKey":
                            shellSettings.EncryptionKey = value;
                            break;
                        case "HashAlgorithm":
                            shellSettings.HashAlgorithm = value;
                            break;
                        case "HashKey":
                            shellSettings.HashKey = value;
                            break;
                        default:
                            shellSettings[key] = value;
                            break;
                    }
                }
            }

            return shellSettings;
        }

        public static string ComposeSettings(ShellSettings settings)
        {
            if (settings == null)
                return "";

            var sb = new StringBuilder();
            foreach (var key in settings.Keys)
            {
                sb.AppendLine(key + ": " + (settings[key] ?? EmptyValue));
            }

            return sb.ToString();
        }
    }
}