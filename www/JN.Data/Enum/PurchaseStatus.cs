using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum PurchaseStatus
    {
        [Description("待付款")]
        Sales = 1,

        [Description("已付款")]
        Transaction = 2,

        [Description("已成交")]
        Deal = 3,

        [Description("已取消")]
        Cancel = -1,

        [Description("卖方投诉")]
        Complaint = -2,
    }
}