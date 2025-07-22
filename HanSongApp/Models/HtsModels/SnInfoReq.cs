using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Models.HtsModels
{
    public class SnInfoReq
    {
        public SnInfo input = new SnInfo();
        public string type_code = "";
        public List<SnInfo> data = new List<SnInfo>();
    }
}
