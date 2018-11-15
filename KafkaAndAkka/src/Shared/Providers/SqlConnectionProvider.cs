using Shared.Interfaces;

namespace Shared
{
    public class SqlConnectionProvider : ISqlConnectionProvider
    {
        public string Provide()
        {
            return "Data Source=.\\SQLEXPRESS;Initial Catalog=AkkaTest;Integrated Security=True";
        }
    }
}