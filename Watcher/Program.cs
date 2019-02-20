using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base;
using TableDependency.SqlClient.Base.EventArgs;

namespace Watcher
{
    internal class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
    }
    internal static class Program
    {
        private static void OnChange(object sender, RecordChangedEventArgs<User> e)
        {
            Notify(e.Entity.Username).Start();
            Console.WriteLine("Id "+ e.Entity.Id + " Name " + e.Entity.Username);
        }

        private static async Task Notify(string entity)
        {
            using (var client = new HttpClient())
            {
                var message = entity + " added to db";
                var res = await client.PostAsync("https://localhost:44303/api/events",
                    message.ToString().AsJson()
                );
                Console.WriteLine(res.ReasonPhrase);
            }
        }
        private static void Main(string[] args)
        {
            const string cs = "Data Source=LAPTOP-SVEAL1S7\\SQLEXPRESS;Initial Catalog=Curry;Integrated Security=True";
            var mapper = new ModelToTableMapper<User>();
            mapper.AddMapping(u => u.Id, "Id");
            mapper.AddMapping(u => u.Username, "Username");

            using (var dependency = new SqlTableDependency<User>
                (cs,"Users", mapper: mapper, executeUserPermissionCheck:false))
            {
                dependency.OnChanged += OnChange;
                dependency.Start();
                Console.ReadKey();
                dependency.Stop();
            }
        }
    }
}