using JN.Data;
using MvcCore;
using MvcCore.Infrastructure;
using MvcCore.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JN
{
    //------------------------------ 数据库上下文管理工厂 开始 ----------------------------------
    public interface ISysDbFactory : IDatabaseFactory
    {

    }
    public class SysDbFactory : DatabaseFactory<SysDbContext>, ISysDbFactory
    {

    }
    //------------------------------ 数据库上下文管理工厂 结束 ----------------------------------
}

namespace JN.Data
{
    /// <summary>
    /// 用户资料库
    /// </summary>
    public partial class SysDbContext : FrameworkContext
    {
        //此处修改链接字符串，可以为 Web.Config 中的 ConnectionStrings 中的 Name ，也可以是数据库连接字符串
        public SysDbContext()
            : base("sys_connection")
        {

        }
        static SysDbContext()
        {
            Database.SetInitializer(new NullDatabaseInitializer<SysDbContext>());//禁用实体映射表自动改动数据结构功能
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestContext, Configuration>());//启用数据迁移形式初始化


        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //金额类型小数位
            foreach (Type classType in from t in Assembly.GetAssembly(typeof(Filters.DecimalPrecisionAttribute)).GetTypes()
                                       where t.IsClass && t.Namespace == "JN.Data"
                                       select t)
            {
                foreach (var propAttr in classType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<Filters.DecimalPrecisionAttribute>() != null).Select(
                       p => new { prop = p, attr = p.GetCustomAttribute<Filters.DecimalPrecisionAttribute>(true) }))
                {

                    var entityConfig = modelBuilder.GetType().GetMethod("Entity").MakeGenericMethod(classType).Invoke(modelBuilder, null);
                    ParameterExpression param = ParameterExpression.Parameter(classType, "c");
                    Expression property = Expression.Property(param, propAttr.prop.Name);
                    LambdaExpression lambdaExpression = Expression.Lambda(property, true,
                                                                             new ParameterExpression[]
                                                                                 {param});
                    DecimalPropertyConfiguration decimalConfig;
                    if (propAttr.prop.PropertyType.IsGenericType && propAttr.prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        MethodInfo methodInfo = entityConfig.GetType().GetMethods().Where(p => p.Name == "Property").ToList()[7];
                        decimalConfig = methodInfo.Invoke(entityConfig, new[] { lambdaExpression }) as DecimalPropertyConfiguration;
                    }
                    else
                    {
                        MethodInfo methodInfo = entityConfig.GetType().GetMethods().Where(p => p.Name == "Property").ToList()[6];
                        decimalConfig = methodInfo.Invoke(entityConfig, new[] { lambdaExpression }) as DecimalPropertyConfiguration;
                    }

                    decimalConfig.HasPrecision(propAttr.attr.Precision, propAttr.attr.Scale);
                }
            }

            //非自增表
            modelBuilder.Entity<SysParam>().Property(p => p.ID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            base.OnModelCreating(modelBuilder);
        }
    }
}

namespace JN.Data.Service
{
    //------------------------- ADO.NET T-SQL 扩展 开始 -----------------------------------

    public interface ISysDBTool : IDBTool { }
    public class SysDBTool : DBTool, ISysDBTool
    {
        public SysDBTool(ISysDbFactory DbFactory) : base(DbFactory) { }
    }
}