using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace JN.Data.Enum
{
    //模块
    public enum AdminModule
    {
        [Description("系统管理")]
        sys = 1,

        [Description("友链管理")]
        FriendUrl = 2,

        [Description("积分查询")]
        pointSearch = 3,

        [Description("广告管理")]
        ad = 4,

        [Description("公告管理")]
        notice = 5,

        [Description("财务管理")]
        order = 6,

        [Description("招标管理")]
        bidder = 7,

        [Description("工程管理")]
        project = 8,

        [Description("行情管理")]
        market = 9,

        [Description("商机管理")]
        ware = 10,

        [Description("新闻管理")]
        news = 11,

        [Description("会员管理")]
        member = 12,

        [Description("会员查询")]
        memberSearch = 13,

        [Description("招聘管理")]
        job = 14,

        [Description("求职管理")]
        jobwanted = 15,

        [Description("评论管理")]
        comment = 16,

        [Description("留言管理")]
        consultation = 17,

        [Description("帮助管理")]
        help = 18,

        [Description("积分商城")]
        giftshop = 19,
    }
}