using SqlSugar;
using System.ComponentModel;

namespace HanSongApp.Models
{
    [SugarTable("station")]
    public class Station 
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int id { get; set; }
        public string? name { get; set; }

    }
}

