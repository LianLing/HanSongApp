﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanSongApp.Models.HtsModels
{
    public class Ack
    {
        public string result = "";    // SUCCESS / :成功， other Defect Code : FAIL
        public string message = "";   // 失败时表示失败信息
        public string sn = "";        //  
    }
}
