using HanSongApp.DataBase;
using HanSongApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Services
{
    internal class CheckStationService : IDisposable
    {
        private readonly DbContext _db;
        public CheckStationService() => _db = new DbContext();
        public void Dispose() => _db?.Dispose();

        public List<Station> GetStations()
        {
            string sql = $@"select t.* from station t";
            var result = _db.Instance.Ado.SqlQuery<Station>(sql).ToList();
            return result;
        }
    }
}
