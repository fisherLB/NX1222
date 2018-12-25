 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using MvcCore;
using System.Data.Entity;
using System.ComponentModel;
using MvcCore.Infrastructure;
using System.Data.Entity.Validation;
namespace JN.Data
{
    public partial class SysDbContext : FrameworkContext
    {
        /// <summary>
        /// 把实体添加到EF上下文
        /// </summary>
        public DbSet<AdminRole> AdminRole { get; set; }
    }

	/// <summary>
    /// 管理员角色表
    /// </summary>
	[DisplayName("管理员角色表")]
    public partial class AdminRole
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 角色名称
        /// </summary>  
				[DisplayName("角色名称")]
		        [MaxLength(50,ErrorMessage="角色名称最大长度为50")]
		public string  RoleName { get; set; }
		      
       
        
        /// <summary>
        /// 备注
        /// </summary>  
				[DisplayName("备注")]
		        [MaxLength(250,ErrorMessage="备注最大长度为250")]
		public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("AuthorityIds")]
				public string  AuthorityIds { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public AdminRole()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 管理员角色表业务接口
    /// </summary>
    public interface IAdminRoleService :IServiceBase<AdminRole> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(AdminRole entity);
	}
    /// <summary>
    /// 管理员角色表业务类
    /// </summary>
    public class AdminRoleService :  ServiceBase<AdminRole>,IAdminRoleService
    {


        public AdminRoleService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(AdminRole entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
