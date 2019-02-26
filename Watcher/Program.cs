using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;
using Watcher.Domain.Models;
using Watcher.Mapping;

namespace Watcher
{
    internal static class Program
    {
        private static IConfigurationBuilder _builder;

        private static void OnChange(object sender, RecordChangedEventArgs<User> e)
        {
            var host = _builder.Build().GetSection("Api").GetSection("Default").ToString();
            var hostUri = new Uri(host);
            Notify(e.Entity.Username, hostUri).Start();
            Console.WriteLine("Id " + e.Entity.Id + " Name " + e.Entity.Username);
        }

        private static async Task Notify(string entity, Uri receiver)
        {
            using (var client = new HttpClient())
            {
                var message = entity + " added to db";
                var res = await client.PostAsync(receiver,
                    message.AsJson()
                );
                Console.WriteLine(res.ReasonPhrase);
            }
        }

        private static void Main()
        {
            _builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json");

            var configuration = _builder.Build();
            var cs = configuration.GetSection("DbConnections").GetSection("Backup").Value;


            using (var dependency = new SqlTableDependency<User>
                (cs, "Users", mapper: UserMapping.GetMapper(), executeUserPermissionCheck: false))
            {
                dependency.OnChanged += OnChange;
                dependency.Start();
                Console.ReadKey();
                dependency.Stop();
            }
        }
    }
}