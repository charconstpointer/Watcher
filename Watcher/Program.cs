using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;
using Watcher.Mapping;

namespace Watcher
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
    }

    internal static class Program
    {
        private static void OnChange(object sender, RecordChangedEventArgs<User> e)
        {
            Notify(e.Entity.Username).Start();
            Console.WriteLine("Id " + e.Entity.Id + " Name " + e.Entity.Username);
        }

        private static async Task Notify(string entity)
        {
            using (var client = new HttpClient())
            {
                var message = entity + " added to db";
                var res = await client.PostAsync("https://localhost:44303/api/events",
                    message.AsJson()
                );
                Console.WriteLine(res.ReasonPhrase);
            }
        }

        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();
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