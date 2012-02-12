using System;
using System.Configuration;

namespace JabbR.Services
{
    public class ApplicationSettings : IApplicationSettings
    {
        public string AuthApiKey
        {
            get
            {
                return ConfigurationManager.AppSettings["auth.apiKey"];
            }
        }

        public string RedisServer
        {
            get { 
                return ConfigurationManager.AppSettings["redis.server"];
            }
        }

        public string RedisPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["redis.password"];
            }
        }

        public int RedisPort
        {
            get
            {
                string value = ConfigurationManager.AppSettings["redis.port"];
                int port;
                if (Int32.TryParse(value, out port))
                {
                    return port;
                }
                return -1;
            }
        }
    }
}