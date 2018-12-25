using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum MatchingStatus
    {
        [Description("未付款")]
        UnPaid = 1,

        [Description("延迟付款")]
        Delayed = 2,

        [Description("已付款")]
        Paid = 3,

        [Description("已成交")]
        Verified = 4,

        [Description("取消")]
        Cancel = -1,

        [Description("虚假信息")]
        Falsehood = -2,
    }
}