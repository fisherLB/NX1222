using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum AgentLevel
    {
        [Description("普通会员")]
        Level0 = 0,

        [Description("经理")]
        Level1 = 1,

        [Description("高级经理")]
        Level2 = 2,
    }
}