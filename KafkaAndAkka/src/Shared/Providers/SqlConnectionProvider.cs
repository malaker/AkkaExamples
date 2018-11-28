using Akka.Configuration;
using Shared.Interfaces;
using System;
using System.Text.RegularExpressions;

namespace Shared
{
    public class SqlConnectionProvider : ISqlConnectionProvider
    {
        private Regex regex = new Regex(@"{\w+}");
        private Config config;

        public SqlConnectionProvider(Akka.Configuration.Config config)
        {
            this.config = config;
        }

        public string Provide()
        {
            var connString = config.GetString("akka.actor.persistence.journal.sql-server.connection-string", "").Replace("$", "");

            if (string.IsNullOrEmpty(connString))
            {
                throw new System.ArgumentNullException(nameof(connString));
            }

            var matches = regex.Matches(connString);
            foreach (Match m in matches)
            {
                var envRaw = m.Value.Replace("{", "").Replace("}", "");
                var env = Environment.GetEnvironmentVariable(envRaw);

                if (string.IsNullOrEmpty(env))
                {
                    throw new ArgumentNullException(envRaw);
                }
                connString = connString.Replace(m.Value, env);
            }
            return connString;
        }
    }
}