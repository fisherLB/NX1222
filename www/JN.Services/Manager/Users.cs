using System;
using System.Collections.Generic;
using System.Linq;
namespace JN.Services.Manager
{
    public partial class Users
    {


        #region 获得团队人数集合，正向查询会员/反向查询会员

        /// <summary>
        /// 获得用户所有子节点会员集合（安置关系，按上至下左至右排序）
        /// </summary>
        /// <param name="onUser">会员实体</param>
        /// <param name="countDepth">层深（几层内）</param>
        /// <returns></returns>
        public static List<Data.User> GetAllChild(Data.User onUser, int countDepth = 0)
        {
            var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => (x.ParentPath.Contains("," + onUser.ID + ",") || x.ParentID == onUser.ID) && x.IsActivation).OrderBy(x => x.RefereeDepth).ThenBy(x => x.DepthSort).ToList();
            if (countDepth > 0)
                userlist = userlist.Where(x => x.Depth <= (onUser.Depth + countDepth)).ToList();
            return userlist;
        }

        /// <summary>
        /// 获得用户所有子节点会员集合（推荐关系，按上至下左至右排序）
        /// </summary>
        /// <param name="onUser">会员实体</param>
        /// <param name="countDepth">几代内</param>
        /// <returns></returns>
        public static List<Data.User> GetAllRefereeChild(Data.User onUser, int countDepth = 0)
        {
            var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => (x.RefereePath.Contains("," + onUser.ID + ",") || x.RefereeID == onUser.ID) && x.IsActivation).OrderBy(x => x.RefereeDepth).ThenBy(x => x.DepthSort).ToList();
            if (countDepth > 0)
                userlist = userlist.Where(x => x.Depth <= (onUser.RefereeDepth + countDepth)).ToList();
            return userlist;
        }

        /// <summary>
        /// 获得用户所有父节点会员集合（安置关系，反向查询）
        /// </summary>
        /// <param name="onUser">会员实体</param>
        /// <param name="countDepth">反向查询几层</param>
        /// <returns></returns>
        public static List<Data.User> GetAllParent(Data.User onUser, int countDepth = 0)
        {
            if (!string.IsNullOrEmpty(onUser.ParentPath))
            {
                string[] ids = onUser.ParentPath.Split(',');
                var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString())).OrderByDescending(x => x.Depth).ToList();
                if (countDepth > 0)
                    userlist = userlist.Where(x => x.Depth >= (onUser.Depth - countDepth)).ToList();
                return userlist;
            }
            return null;
        }

        /// <summary>
        /// 获得用户所有父节点会员集合（推荐关系，反向查询）
        /// </summary>
        /// <param name="onUser">会员实体</param>
        /// <param name="countDepth">反向查询几代</param>
        /// <returns></returns>
        public static List<Data.User> GetAllRefereeParent(Data.User onUser, int countDepth = 0)
        {
            if (!string.IsNullOrEmpty(onUser.RefereePath))
            {
                string[] ids = onUser.RefereePath.Split(',');
                var userlist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => ids.Contains(x.ID.ToString())).OrderByDescending(x => x.RefereeDepth).ToList();
                if (countDepth > 0)
                    userlist = userlist.Where(x => x.Depth >= (onUser.RefereeDepth - countDepth)).ToList();
                return userlist;
            }
            return null;
        }

        #endregion

        #region 会员激活操作
        public static void ActivationAccount(Data.User onUser)
        {
            var cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().ListCache("sysparam", MvcCore.Extensions.CacheTimeType.ByHours, 24, x => x.ID < 4000);
            //更新激活标记
            onUser.IsActivation = true;
            onUser.ActivationTime = DateTime.Now;
            MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(onUser);
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            //激活会员扣除激活币


            ////会员升级 
            //string[] userids = onUser.ParentPath.Split(',');
            //var parentusers = MvcCore.Unity.Get<JN.Data.Service.IUserService>().List(x => userids.Contains(x.ID.ToString())).ToList();
            //foreach (var puser in parentusers)
            //{
            //    var updateEntity = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(puser.ID);
            //    int tdrs = getrdrs(puser);
            //    if (tdrs >= cacheSysParam.SingleAndInit(x => x.ID == 3001).Value.ToInt() && updateEntity.UserLevel <= 0)
            //        updateEntity.UserLevel = (int)JN.Data.Enum.UserLeve.Level1;
            //    else if (tdrs >= cacheSysParam.SingleAndInit(x => x.ID == 3002).Value.ToInt() && updateEntity.UserLevel <= (int)JN.Data.Enum.UserLeve.Level1)
            //        updateEntity.UserLevel = (int)JN.Data.Enum.UserLeve.Level2;
            //    else if (tdrs >= cacheSysParam.SingleAndInit(x => x.ID == 3003).Value.ToInt() && updateEntity.UserLevel <= (int)JN.Data.Enum.UserLeve.Level2)
            //        updateEntity.UserLevel = (int)JN.Data.Enum.UserLeve.Level3;
            //    else if (tdrs >= cacheSysParam.SingleAndInit(x => x.ID == 3004).Value.ToInt() && updateEntity.UserLevel <= (int)JN.Data.Enum.UserLeve.Level3)
            //        updateEntity.UserLevel = (int)JN.Data.Enum.UserLeve.Level4;
            //    else if (tdrs >= cacheSysParam.SingleAndInit(x => x.ID == 3005).Value.ToInt() && updateEntity.UserLevel <= (int)JN.Data.Enum.UserLeve.Level4)
            //        updateEntity.UserLevel = (int)JN.Data.Enum.UserLeve.Level5;
            //    else if (tdrs >= cacheSysParam.SingleAndInit(x => x.ID == 3006).Value.ToInt() && updateEntity.UserLevel <= (int)JN.Data.Enum.UserLeve.Level5)
            //        updateEntity.UserLevel = (int)JN.Data.Enum.UserLeve.Level6;
            //    else if (tdrs >= cacheSysParam.SingleAndInit(x => x.ID == 3007).Value.ToInt() && updateEntity.UserLevel <= (int)JN.Data.Enum.UserLeve.Level6)
            //        updateEntity.UserLevel = (int)JN.Data.Enum.UserLeve.Level7;
            //    else if (tdrs >= cacheSysParam.SingleAndInit(x => x.ID == 3008).Value.ToInt() && updateEntity.UserLevel <= (int)JN.Data.Enum.UserLeve.Level7)
            //        updateEntity.UserLevel = (int)JN.Data.Enum.UserLeve.Level8;
            //    else if (tdrs >= cacheSysParam.SingleAndInit(x => x.ID == 3009).Value.ToInt() && updateEntity.UserLevel <= (int)JN.Data.Enum.UserLeve.Level8)
            //        updateEntity.UserLevel = (int)JN.Data.Enum.UserLeve.Level9;
            //    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
            //}
        }
        #endregion

        #region 申请经理
        /// <summary>
        /// 申请经理
        /// </summary>
        public static bool HaveApplyAgent(int uid, int level, ref string outMsg)
        {
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();

            var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(uid);

            if (level == 1)
            {
                //推荐人数 
                //decimal PARAM_TJRYJ = cacheSysParam.SingleAndInit(x => x.ID == 1801).Value2.Split('|')[1].ToDecimal(); //推荐要求
                int PARAM_TJRS = cacheSysParam.SingleAndInit(x => x.ID == 1801).Value.ToInt(); //推荐要求
                int tjrs = GetAllRefereeChild(onUser, 1).Where(x => (x.AddupSupplyAmount ?? 0) > 0).Count(); //如累计提供0以上的直推人数

                //团队人数
                int tdrs = GetAllRefereeChild(onUser, 0).Where(x => (x.AddupSupplyAmount ?? 0) > 0).Count();//累计提供0以上的团队人数
                int PARAM_TDRS = cacheSysParam.SingleAndInit(x => x.ID == 1801).Value2.ToInt(); //团队人数

                if (tjrs >= PARAM_TJRS && tdrs >= PARAM_TDRS)
                    return true;
                else
                {
                    outMsg = "您没达到条件申请普通经理！详情：符合直推：" + tjrs + "/" + PARAM_TJRS + "，团队" + tdrs + "/" + PARAM_TDRS;
                    return false;
                }
            }
            else if (level == 2)
            {
                ////推荐人数 
                //decimal PARAM_TJRYJ = cacheSysParam.SingleAndInit(x => x.ID == 1802).Value2.Split('|')[1].ToDecimal(); //推荐要求
                //int PARAM_TJRS = cacheSysParam.SingleAndInit(x => x.ID == 1802).Value2.Split('|')[0].ToInt(); //推荐要求
                //int tjrs = GetAllRefereeChild(onUser, 1).Where(x => (x.AddupSupplyAmount ?? 0) > PARAM_TJRYJ).Count(); //如累计提供2000以上的直推人数

                ////团队业绩
                //decimal tdyj = GetAllRefereeChild(onUser).Count() > 0 ? GetAllRefereeChild(onUser).Sum(x => (x.AddupSupplyAmount ?? 0)) : 0;
                //decimal PARAM_GRYJ = cacheSysParam.SingleAndInit(x => x.ID == 1802).Value.ToDecimal(); //个人业绩
                //decimal PARAM_TDYJ = cacheSysParam.SingleAndInit(x => x.ID == 1802).Value3.ToDecimal(); //团队业绩

                //if (tjrs >= PARAM_TJRS && (onUser.AddupSupplyAmount ?? 0) >= PARAM_GRYJ && tdyj >= PARAM_TDYJ)
                //    return true;
                //else
                //{
                //    outMsg = "您没达到条件申请高级经理！详情：符合推荐：" + tjrs + "/" + PARAM_TJRS + "，本人：" + onUser.AddupSupplyAmount + "/" + PARAM_GRYJ + "，团队" + tdyj + "/" + PARAM_TDYJ;
                //    return false;
                //}

                int PARAM_TJRS = cacheSysParam.SingleAndInit(x => x.ID == 1802).Value.ToInt(); //推荐要求
                int tjrs = GetAllRefereeChild(onUser, 1).Where(x => (x.AddupSupplyAmount ?? 0) > 0).Count(); //如累计提供0以上的直推人数

                //团队人数
                int tdrs = GetAllRefereeChild(onUser, 0).Where(x => (x.AddupSupplyAmount ?? 0) > 0).Count();//累计提供0以上的团队人数
                int PARAM_TDRS = cacheSysParam.SingleAndInit(x => x.ID == 1802).Value2.ToInt(); //团队人数

                if (tjrs >= PARAM_TJRS && tdrs >= PARAM_TDRS)
                    return true;
                else
                {
                    outMsg = "您没达到条件申请高级经理！详情：符合直推：" + tjrs + "/" + PARAM_TJRS + "，团队" + tdrs + "/" + PARAM_TDRS;
                    return false;
                }
            }
            else
            {
                outMsg = "错误的经理级别";
                return false;
            }
        }

        public static void ApplyAgent(int uid, int level, string agentname, string remark, ref string msg)
        {
            var onUser = MvcCore.Unity.Get<Data.Service.IUserService>().Single(uid);

            if (HaveApplyAgent(uid, level, ref msg))
            {
                if (!(onUser.IsAgent ?? false) && String.IsNullOrEmpty(onUser.AgentName))
                {
                    //更新申请状态
                    onUser.AgentName = agentname;
                    onUser.AgentLevel = level;
                    onUser.ApplyAgentRemark = remark;
                    onUser.ApplyAgentTime = DateTime.Now;
                    MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(onUser);
                    MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                    msg = "您已成功申请" + (level == 1 ? "普通经理" : "高级经理") + "，请耐心等待公司审核！";
                }
                else
                    msg = "您已提交过申请，请耐心等待审核。";
            }
        }
        #endregion

        #region 会员升级
        public static void UpdateUser()
        {
            var ulist = MvcCore.Unity.Get<Data.Service.IUserService>().List(x => x.IsActivation==true);
            List<Data.SysParam> cacheSysParam = MvcCore.Unity.Get<JN.Data.Service.ISysParamService>().List(x => x.ID < 4000).ToList();
            foreach (var item in ulist)
            {
                var itm = MvcCore.Unity.Get<JN.Data.Service.IUserService>().Single(item.ID);
                int tjrs = GetAllRefereeChild(item, 1).Where(x => (x.AddupSupplyAmount ?? 0) > 0).Count(); //如累计提供0以上的直推人数

                //团队人数
                int tdrs = GetAllRefereeChild(item, 0).Where(x => (x.AddupSupplyAmount ?? 0) > 0).Count();//累计提供0以上的团队人数

                var AgentParam = cacheSysParam.SingleAndInit(x => x.ID == 1801); //经理要求   
                var ProTeamParam = cacheSysParam.SingleAndInit(x => x.ID == 1802); //专业团队要求  
             

                if ((item.IsProTeam !=true) && tjrs >= ProTeamParam.Value.ToInt() && tdrs >= ProTeamParam.Value2.ToInt())
                {
                   
                    itm.IsProTeam = true;
                    MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(itm);
                   // MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
                }

                if (item.IsAgent !=true  && tjrs >= AgentParam.Value.ToInt() && tdrs >= AgentParam.Value2.ToInt())
                {
                    
                    itm.AgentName = item.UserName;
                    itm.AgentLevel = 1;
                    itm.ApplyAgentRemark = "";
                    itm.ApplyAgentTime = DateTime.Now;
                    itm.IsAgent = true;
                    MvcCore.Unity.Get<JN.Data.Service.IUserService>().Update(itm);
                  
                }

            }
            MvcCore.Unity.Get<Data.Service.ISysDBTool>().Commit();
        }
        #endregion


    }
}