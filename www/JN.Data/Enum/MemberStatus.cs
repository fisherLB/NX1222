using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
namespace JN.Data.Enum
{
    public enum MemberStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Confirm = 1,

        /// <summary>
        /// 待审
        /// </summary>
        [Description("待审")]
        NoPassed = 0,

        /// <summary>
        /// 冻结
        /// </summary>
        [Description("冻结")]
        Freeze= 2,

        ///// <summary>
        ///// 异常
        ///// </summary>
        //[Description("异常")]
        //Other = 4,
    }
}
