using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Watcher.Notifications
{
    public static class Notification
    {
        public static async Task Notify(string entity)
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
    }
}