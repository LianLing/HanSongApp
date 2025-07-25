using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Models
{
    [SugarTable("prod_team")]
    public class TeamModel
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string leader { get; set; }
        public int type { get; set; }
        public string factory_code { get; set; }
    }
}
