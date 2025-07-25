using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Models
{
    [SugarTable("cfg_model")]
    public class modelModel
    {
        public int id { get; set; }
        /// <summary>
        /// 机型
        /// </summary>
        public string prod_type { get; set; }
        /// <summary>
        /// 工序
        /// </summary>
        public string prod_model { get; set; }
        /// <summary>
        /// 模组
        /// </summary>
        public string prod_module { get; set; }
        public int status { get; set; }
        public DateTime imp_time { get; set; }
    }
}
