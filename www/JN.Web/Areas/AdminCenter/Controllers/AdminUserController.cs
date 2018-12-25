using JN.Data;
using JN.Data.Service;
using MvcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JN.Web.Areas.AdminCenter.Controllers
{
    public class AdminUserController : BaseController
    {
        private readonly IAdminUserService AdminUserService;
        private readonly IAdminRoleService AdminRoleService;
        private readonly IAdminAuthorityService AdminAuthorityService;
        private readonly ISysDBTool SysDBTool;
        private readonly IActLogService ActLogService;
     
        public AdminUserController(ISysDBTool SysDBTool, 
            IAdminUserService AdminUserService, 
            IAdminRoleService AdminRoleService, 
            IAdminAuthorityService AdminAuthorityService, 
            IActLogService ActLogService
           )
        {
            this.AdminUserService = AdminUserService;
            this.AdminRoleService = AdminRoleService;
            this.AdminAuthorityService = AdminAuthorityService;
            this.SysDBTool = SysDBTool;
            this.ActLogService = ActLogService;
            this.SysDBTool = SysDBTool;
        }

        public ActionResult Index()
        {
            var list = AdminUserService.List().OrderByDescending(x => x.ID).ToList();
            ViewBag.Title = "管理员管理";
            ActMessage = ViewBag.Title;
            return View(list);
        }

        public ActionResult Modify(int? id)
        {
            ViewBag.Title = "修改管理员";
            ActMessage = ViewBag.Title;
            Data.AdminUser model = new Data.AdminUser();
            if (id.HasValue) model = AdminUserService.Single(id);
            ViewData["AdminRoleList"] = new SelectList(AdminRoleService.List().ToList(), "ID", "RoleName", model.RoleID);
             return View(model);
        }

        [HttpPost]
        public ActionResult Modify(FormCollection fc)
        {
            var result = new ReturnResult();
            try
            {
                var entity = AdminUserService.SingleAndInit(fc["ID"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);
                if (entity.ID > 0)
                {
                    if (fc["resetpwd"] == "true")
                    {
                        entity.Password = ("111111").ToMD5().ToMD5();
                        entity.Password = ("222222").ToMD5().ToMD5();
                    }
                    AdminUserService.Update(entity);
                }
                else
                {
                    if (AdminUserService.List(x => x.AdminName.Equals(entity.AdminName)).Count() >0)
                        throw new Exception("管理员帐号已被使用");
                    entity.CreateTime = DateTime.Now;
                    entity.Password = entity.Password.ToMD5().ToMD5();
                    entity.Password = entity.Password2.ToMD5().ToMD5();
                    entity.IsPassed = true;
                    AdminUserService.Add(entity);
                }
                SysDBTool.Commit();
                result.Status = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                Services.Manager.logs.WriteErrorLog(HttpContext.Request.Url.ToString(), ex);
            }
            return Json(result);
        }

        public ActionResult Delete(int id)
        {
            ActMessage = "删除管理员";
            var model = AdminUserService.Single(id);
            if (model != null)
            {
                if (model.ID == 1)
                {
                    ViewBag.ErrorMsg = "超级管理员帐号不能删除！";
                    return View("Error");
                }
                else
                {
                    ActPacket = model;
                    AdminUserService.Delete(id);
                    SysDBTool.Commit();
                    ViewBag.SuccessMsg = "管理员“" + model.AdminName + "”已被删除！";
                    return View("Success");
                }
            }
            ViewBag.ErrorMsg = "记录不存在或已被删除！";
            return View("Error");
        }

        public ActionResult MakeLock(int id)
        {
            var model = AdminUserService.Single(id);
            if (model != null)
            {
                if (model.ID == 1)
                {
                    ViewBag.ErrorMsg = "超级管理员帐号不能被冻结！";
                    return View("Error");
                }
                else
                {
                    model.IsPassed = false;
                    AdminUserService.Update(model);
                    SysDBTool.Commit();
                    ViewBag.SuccessMsg = "管理员“" + model.AdminName + "”帐号已被冻结！";
                    ActPacket = model;
                    return View("Success");
                }
            }
            ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            return View("Error");
        }

        public ActionResult MakeUnLock(int id)
        {
            Data.AdminUser model = AdminUserService.Single(id);
            if (model != null)
            {
                model.IsPassed = true;
                AdminUserService.Update(model);
                SysDBTool.Commit();
                ViewBag.SuccessMsg = "管理员“" + model.AdminName + "”帐号已解锁！";
                ActPacket = model;
                return View("Success");
            }
            ViewBag.ErrorMsg = "系统异常！请查看系统日志。";
            return View("Error");
        }


        public ActionResult Role(int? page)
        {
            ViewBag.Title = "角色管理";
            ActMessage = ViewBag.Title;
            var list = AdminRoleService.List().ToList();
            return View(list);
        }

        [HttpPost]
        public ActionResult Role(FormCollection fc)
        {
            try
            {
                var entity = AdminRoleService.SingleAndInit(fc["ID"].ToInt());
                TryUpdateModel(entity, fc.AllKeys);
                if (entity.ID > 0)
                    AdminRoleService.Update(entity);
                else
                    AdminRoleService.Add(entity);
                SysDBTool.Commit();
            }
            catch (Exception ex)
            {
                return Json(new { result = "err", refMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { result = "ok" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ModifyRole(int id)
        {
            var model = AdminRoleService.Single(id);
            if (model != null)
                return Json(new { result = "ok", data = model }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { result = "err", refMsg = "记录不存在或已被删除" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Authority(int rid)
        {
            ActMessage = "配置角色权限";
            var role = AdminRoleService.Single(rid);
            if (role != null)
            {
                ViewBag.AuthorityIds = role.AuthorityIds + ",";
                var list = AdminAuthorityService.List().OrderByDescending(x => x.ControllerName).ThenByDescending(x => x.ActionName).ToList();
                ViewBag.Title = "配置角色权限（" + role.RoleName + "）";
                return View(list);
            }
            else
            {
                ViewBag.ErrorMsg = "错误的参数。";
                return View("Error");
            }
        }

        public ActionResult MakeAuthority(string ids, int rid)
        {
            var role = AdminRoleService.Single(rid);
            if (role != null)
            {
                role.AuthorityIds = ids;
                AdminRoleService.Update(role);
                SysDBTool.Commit();
                return Content("ok");
            }
            return Content("err");
        }
    }
}
