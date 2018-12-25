using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum PayStatus
    {
        [Description("待付款")]
        ToPaid = 0,

        [Description("付款成功")]
        Successful = 1,

        [Description("支付失败")]
        Failure = -1,
    }
}