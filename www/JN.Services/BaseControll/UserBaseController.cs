using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using JN.Services.Filters;

namespace JN.BLL.BaseControll
{
    [LoginAttribute]
    public class UserBaseController : Controller
    {
        /// <summary>
        /// 当前登录的店铺对象
        /// </summary>
        //protected Domain.Store userLoginInfo;

        //public UserBaseController()
        //{
        //    userLoginInfo = Extensions.CacheLoginInfoHelper.CurrentUserInfo();
        //}
    }
}
