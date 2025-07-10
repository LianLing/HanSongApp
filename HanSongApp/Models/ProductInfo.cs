using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Models
{
    public class ProductInfo
    {
        public string prod_type { get; set; }
        /// <summary>
        /// 工序
        /// </summary>
        public string prod_model { get; set; }
        /// <summary>
        /// 模组
        /// </summary>
        public string prod_process_grp { get; set; }
        /// <summary>
        /// 制程
        /// </summary>
        public string prod_process { get; set; }
        /// <summary>
        /// 线别
        /// </summary>
        public string Line { get; set; }
        /// <summary>
        /// 班组
        /// </summary>
        public string ClassTeam { get; set; }
        /// <summary>
        /// 工单
        /// </summary>
        public string Mo { get; set; }
    }
}
