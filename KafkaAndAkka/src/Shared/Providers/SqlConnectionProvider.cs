using Shared.Interfaces;

namespace Shared
{
    public class SqlConnectionProvider : ISqlConnectionProvider
    {
        public string Provide()
        {
            return "Data Source=192.168.1.10\\SQLEXPRESS,1433;Initial Catalog=AkkaTest;User Id=test2;Password=test2;";
        }
    }
}