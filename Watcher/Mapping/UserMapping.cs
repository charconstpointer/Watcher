using TableDependency.SqlClient.Base;
using Watcher.Domain.Models;

namespace Watcher.Mapping
{
    public class UserMapping
    {
        public static ModelToTableMapper<User> GetMapper()
        {
            var mapper = new ModelToTableMapper<User>();
            mapper.AddMapping(u => u.Id, "Id");
            mapper.AddMapping(u => u.Username, "Username");
            return mapper;
        }
    }
}