using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Models.HtsModels
{
    public class ChkOutReq
    {
        public string sn = "";             // sn
        public string type_code = "";      // 机型代码
        public string module_code = "";    // 模组代码
        public string stage_code = "";    // 工艺代码
        public string process_code = "";   // 制程代码
        public string station_code = "";   // 站点代码
        public string station_vercode = "";   // 当前站点的版本,如欧洲版,美洲版
        public string station_next = "";   // 当前站的下一站
        public int test_rst = 0;          // 测试结果  0:测试成功， -1：测试失败
        public string note = "";           // 备注
        public string jsonLog = "";        // 测试log
        public string mo = "";             // 工单
                                           //public string pn;             // 品号
        public int elapse = 0;            // 测试耗时S
        public string err_code = "";       // 错误代码

        public string userid = "";         // 测试员
        public string host_name = "";      // 测试电脑名， 可以归并到 测试设备里
        public string prod_line = "";      // 线别
        public string team = "";           // 班组
        public string tool_name = "";      // 测试工具名
        public string tool_ver = "";       // 测试工具版本
                                           //public List<string> equipment;  // 测试设备清单
                                           // public string cfg_id;           // 因为测试工艺会被修改，这里记录测试时使用的配置文件id，用于追溯
                                           // 其它TBD
    }
}
