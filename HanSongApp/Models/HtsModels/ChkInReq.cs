using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Models.HtsModels
{
    public class ChkInReq
    {

        public SnInfo input { get; set; }

        public string type_code { get; set; }      // 机型代码
        public string module_code { get; set; }    // 模组代码
        public string stage_code { get; set; }     // 工艺代码
        public string process_code { get; set; }   // 制程代码
        public string station_code { get; set; }   // 站点代码

        public string mo { get; set; }             // 工单

        public string userid { get; set; }         // 测试员
        public string host_name { get; set; }      // 测试电脑名， 可以归并到 测试设备里
        public string prod_line { get; set; }     // 线别
        public string team { get; set; }           // 班组
        public string cfg_id { get; set; }         // 因为测试工艺会被修改，这里记录测试时使用的配置文件id，用于追溯
        public int timeouts = 0;          // 测试允许的最长时间（S), 在超时之前该机器会被锁定，不允许其它客户端测试。 0:表示不限时
    }
}
