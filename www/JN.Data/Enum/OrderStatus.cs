using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum OrderStatus
    {
        [Description("已付款待发货")]
        Sales = 1,

        [Description("已发货")]
        Transaction = 2,

        [Description("已完成")]
        Deal = 3,

        [Description("已取消")]
        Cancel = -1,
    }
}