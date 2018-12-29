using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    public enum USDStatus
    {
        [Description("挂单中")]
        Sales = 1,

        [Description("交易中")]
        Transaction = 2,

        [Description("已成交")]
        Deal = 3,

        [Description("已取消")]
        Cancel = -1,

        [Description("投诉纠纷")]
        Complaint = -2,
    }
}