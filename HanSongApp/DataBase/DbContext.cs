using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace HanSongApp.DataBase
{
    public class DbContext : IDisposable
    {
        private SqlSugarClient? _client;
        public SqlSugarClient Instance => _client ?? (_client = new SqlSugarClient(
            new ConnectionConfig()
            {
                ConnectionString = "Server=10.10.1.80;Port=3306;Database=hts_pcs;Uid=htsusr;Pwd=HtsUsr.1;Connect Timeout=10;CharSet=utf8mb4;Pooling=false;",
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            }));

        public void Dispose()
        {
            _client?.Close();  // 关闭连接
            _client?.Dispose();
        }
    }
}
