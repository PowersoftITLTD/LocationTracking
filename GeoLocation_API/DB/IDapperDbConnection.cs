using System.Data;

namespace GeoLocation_API.DB
{
    public interface IDapperDbConnection
    {
        public IDbConnection CreateConnection();
    }
}
