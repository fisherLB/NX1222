using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum HelpStatus
    {
        [Description("未匹配")]
        NoMatching = 1,

        [Description("匹配部分")]
        PartOfMatching = 2,

        [Description("匹配完成")]
        AllMatching = 3,

        [Description("部分成交")]
        PartOfDeal = 4,

        //[Description("全部成交[冻结]")]
        //AllDeal = 5,
        [Description("全部成交")]
        AllDeal = 5,

        //[Description("全部成交")]
        //AllTransactions = 6,

        [Description("已撤消")]
        Cancel = -1,
    }
}