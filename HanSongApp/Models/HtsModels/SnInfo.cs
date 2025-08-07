using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Models.HtsModels
{
    public class SnInfo
    {
        public string? type { get; set; }     // sn / var /res
        public string? key { get; set; }       // csn / btmac /wifimac / deviceid ....
        public string? val { get; set; }        // 
    }
}
