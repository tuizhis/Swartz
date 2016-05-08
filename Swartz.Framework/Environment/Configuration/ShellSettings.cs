using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace Swartz.Environment.Configuration
{
    public class ShellSettings
    {
        public const string DefaultName = "Default";
        private readonly IDictionary<string, string> _values;

        public ShellSettings()
        {
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public ShellSettings(ShellSettings settings)
        {
            _values = new Dictionary<string, string>(settings._values, StringComparer.OrdinalIgnoreCase);
        }

        public string this[string key]
        {
            get
            {
                string retVal;
                return _values.TryGetValue(key, out retVal) ? retVal : null;
            }
            set { _values[key] = value; }
        }

        public IEnumerable<string> Keys => _values.Keys;

        public string Name
        {
            get { return this["Name"] ?? ""; }
            set { this["Name"] = value; }
        }

        public string DataProvider
        {
            get { return this["DataProvider"] ?? ""; }
            set { this["DataProvider"] = value; }
        }

        public string Host
        {
            get { return this["Host"]; }
            set { this["Host"] = value; }
        }

        public string User
        {
            get { return this["User"]; }
            set { this["User"] = value; }
        }

        public string Password
        {
            get { return this["Password"]; }
            set { this["Password"] = value; }
        }

        public string Database
        {
            get { return this["Database"]; }
            set { this["Database"] = value; }
        }

        public string Port
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this["Port"]))
                {
                    if (string.Compare(DataProvider, "MySql", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return "3306";
                    }
                    if (string.Compare(DataProvider, "SqlServer", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return "1433";
                    }
                }
                return this["Port"];
            }
            set { this["Port"] = value; }
        }

        public string EncryptionAlgorithm
        {
            get { return this["EncryptionAlgorithm"]; }
            set { this["EncryptionAlgorithm"] = value; }
        }

        public string EncryptionKey
        {
            get { return this["EncryptionKey"]; }
            set { this["EncryptionKey"] = value; }
        }

        public string HashAlgorithm
        {
            get { return this["HashAlgorithm"]; }
            set { this["HashAlgorithm"] = value; }
        }

        public string HashKey
        {
            get { return this["HashKey"]; }
            set { this["HashKey"] = value; }
        }

        public string GetDataConnectionString()
        {
            if (string.Compare(DataProvider, "MySql", StringComparison.OrdinalIgnoreCase) == 0)
            {
                var builder = new MySqlConnectionStringBuilder
                {
                    CharacterSet = "utf8",
                    UserID = User,
                    Password = Password,
                    Database = Database,
                    Server = Host,
                    Port = Convert.ToUInt32(Port)
                };

                return builder.GetConnectionString(true);
            }

            if (string.Compare(DataProvider, "SqlServer", StringComparison.OrdinalIgnoreCase) == 0)
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = $"{Host},{Port}",
                    Pooling = true,
                    Password = Password,
                    UserID = User,
                    IntegratedSecurity = false,
                    InitialCatalog = Database,
                };

                return builder.ConnectionString;
            }

            throw new NotSupportedException("不支持的数据库提供者类型：" + DataProvider);
        }
    }
}