using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Models
{
    [SugarTable("prod_line")]
    public class LineModel
    {
        public int id { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string leader { get; set; }
        public int status { get; set; }  
        public string note { get; set; }
        public string imp_time { get; set; }

    }
}
