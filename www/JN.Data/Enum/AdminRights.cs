using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //管理员权限指令
    public enum AdminRights
    {
        [Description("浏览")]
        Browse = 0,

        [Description("详细")]
        Detail = 1,

        [Description("新增")]
        Add = 2,

        [Description("编辑")]
        Edit = 3,

        [Description("删除")]
        Delete = 4,

        [Description("审核")]
        Audit = 5,

        [Description("打印")]
        Print = 6,

        [Description("导出")]
        Export = 7,
    }
}