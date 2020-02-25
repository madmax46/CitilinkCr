using System;
using System.Collections.Generic;
using System.Text;

namespace MySqlUtils
{
    public class MySqlConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public string UserId { get; set; }
        public string Database { get; set; }
        public string SslMode { get; set; }
        public string CharacterSet { get; set; }

        public string CreateConnectionString()
        {
            List<string> stringConnect = new List<string>();
            stringConnect.Add($"Host={Host}");
            if (Port != 0)
                stringConnect.Add($"Port={Port}");
            stringConnect.Add($"User Id={UserId}");
            stringConnect.Add($"Password={Password}");

            if (!string.IsNullOrEmpty(Database))
                stringConnect.Add($"Database={Database}");

            if (!string.IsNullOrEmpty(SslMode))
                stringConnect.Add($"SslMode={SslMode}");

            if (!string.IsNullOrEmpty(CharacterSet))
                stringConnect.Add($"Character Set={CharacterSet}");

            stringConnect.Add($"ConnectionLifeTime=360");
            stringConnect.Add($"default command timeout=300");


            return string.Join(";", stringConnect);
        }
    }
}
