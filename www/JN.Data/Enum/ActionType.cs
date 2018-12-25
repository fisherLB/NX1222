using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //操作类型
    public enum ActionType
    {
        /// <summary>
        /// 登录
        /// </summary>
        [Description("登录")]
        Login = 0,

        /// <summary>
        /// 登出
        /// </summary>
        [Description("登出")]
        Logout = 1,

        /// <summary>
        /// 新增
        /// </summary>
        [Description("新增")]
        Add = 2,

        /// <summary>
        /// 编辑
        /// </summary>
        [Description("编辑")]
        Edit = 3,

        /// <summary>
        /// 删除
        /// </summary>
        [Description("删除")]
        Delete = 4,

        /// <summary>
        /// 审核
        /// </summary>
        [Description("审核")]
        Passed = 5,

        /// <summary>
        /// 更改状态
        /// </summary>
        [Description("更改状态")]
        ChangeState = 6,

        /// <summary>
        /// 打印
        /// </summary>
        [Description("打印")]
        Print = 7,

        /// <summary>
        /// 导出
        /// </summary>
        [Description("导出")]
        Export = 8,

        /// <summary>
        /// 浏览
        /// </summary>
        [Description("浏览")]
        Browse = 9,

        /// <summary>
        /// 群发
        /// </summary>
        [Description("群发")]
        Send = 10,

        /// <summary>
        /// 备份
        /// </summary>
        [Description("备份")]
        BackUp = 11,

        /// <summary>
        /// 恢复
        /// </summary>
        [Description("恢复")]
        Restore = 12,
    }
}