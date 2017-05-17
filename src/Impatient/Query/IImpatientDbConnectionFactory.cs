using System.Data.Common;

namespace Impatient.Query
{
    public interface IImpatientDbConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
