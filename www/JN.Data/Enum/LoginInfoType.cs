using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Enum
{
    public enum LoginInfoType
    {
        /*
         * 添加账户类型的时候，枚举值为2的倍数
         * 
         */

        /// <summary>
        /// 管理人员
        /// </summary>
        [Description("管理人员")]
        Manager = 2,

        /// <summary>
        /// 用户
        /// </summary>
        [Description("用户")]
        User = 4,

        ///// <summary>
        ///// 商铺
        ///// </summary>
        //[Description("商铺")]
        //Store = 8,
    }
}
