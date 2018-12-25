using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //积分周期
    public enum UserLeve
    {
        /// <summary>
        /// 普通会员
        /// </summary>
        [Description("普通会员")]
        Level0 = 0,
        /// <summary>
        /// 1星会员
        /// </summary>
        [Description("1星会员")]
        Level1 = 1,
        /// <summary>
        /// 2星会员
        /// </summary>
        [Description("2星会员")]
        Level2 = 2,
        /// <summary>
        /// 1星会员
        /// </summary>
        [Description("3星会员")]
        Level3 =3,
        /// <summary>
        /// 1星会员
        /// </summary>
        [Description("4星会员")]
        Level4 = 4,
        /// <summary>
        /// 5星会员
        /// </summary>
        [Description("5星会员")]
        Level5 = 5,
        /// <summary>
        /// 黄钻
        /// </summary>
        [Description("黄钻")]
        Level6 = 6,
        /// <summary>
        /// 蓝钻
        /// </summary>
        [Description("蓝钻")]
        Level7 = 7,
        /// <summary>
        /// 红钻
        /// </summary>
        [Description("红钻")]
        Level8 = 8,
        /// <summary>
        /// 宝石
        /// </summary>
        [Description("宝石")]
        Level9 = 9,

    }
}