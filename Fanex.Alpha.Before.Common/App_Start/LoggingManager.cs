using Fanex.Logging;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Extensions.Web;
using Fanex.Logging.RabbitMQ;
using Fanex.Logging.Sentry.RavenClient;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

namespace Fanex.Alpha.Before.Common.App_Start
{
    public static class LoggingManager
    {
        public static void Init()
        {
            var logEngines = new List<ILogEngine>
            {
                new RabbitMQLogging(ConfigurationManager.AppSettings["Fanex.Logging:RabbitMQConnectionStrings"])
            };

            var sentryUrl = ConfigurationManager.AppSettings["Fanex.Logging:SentryUrl"];
            if (!string.IsNullOrWhiteSpace(sentryUrl))
            {
                logEngines.Add(
                    new SentryLogging(new SentryEngineOptions
                    {
                        Dsn = new SharpRaven.Dsn(sentryUrl)
                    },
                    "assets/scripts/raven.min.js"));
            }

            LogManager
                    .SetDefaultLogCategory(ConfigurationManager.AppSettings["Fanex.Logging:DefaultCategory"])
                    .Use(logEngines);

            ExceptionHandler
                .UseConfig(new Logging.Extensions.Web.LoggingConfiguration()
                .GetHttpHeaderInfo()
                .HideSensitiveInfo(GetRegexToHideSensitiveInfo())
                .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo));
        }

        private static List<string> GetRegexToHideSensitiveInfo()
        {
            var result = new List<string>();
            var sensitiveInfo = new[] { "password", "txtpassword" };

            foreach (var info in sensitiveInfo)
            {
                result.Add($"(?:{info}=)(?<paramValue>[^&|^\\s]+)");
                result.Add($"(\"{info}\":)(?<paramValue>[^,|^\r]+)");
            }

            return result;
        }
    }
}