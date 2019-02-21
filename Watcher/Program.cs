using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.EventArgs;
using Watcher.Domain.Models;
using Watcher.Notifications;

namespace Watcher
{
    internal static class Program
    {
        private static void OnChange(object sender, RecordChangedEventArgs<User> e)
        {
            Console.WriteLine("Id " + e.Entity.Id + " Name " + e.Entity.Username);
            Notification.Notify(e.Entity.Username).Start();
        }


        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();
            var cs = configuration.GetSection("DbConnections").GetSection("Default").Value;
//            const string cs = "Data Source=LAPTOP-SVEAL1S7\\SQLEXPRESS;Initial Catalog=Curry;Integrated Security=True";
            var mapper = new ModelToTableMapper<User>();
            mapper.AddMapping(u => u.Id, "Id");
            mapper.AddMapping(u => u.Username, "Username");

            using (var dependency = new SqlTableDependency<User>
                (cs, "Users", mapper: mapper, executeUserPermissionCheck: false))
            {
                dependency.OnChanged += OnChange;
                dependency.Start();
                Console.ReadKey();
                dependency.Stop();
            }
        }
    }
}