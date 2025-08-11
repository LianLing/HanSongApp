using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Models.HtsModels
{
    public class SnInfoReq
    {
        public SnInfo input { get; set; }
        public string type_code { get; set; }
        public List<SnInfo> data { get; set; }
    }
}
