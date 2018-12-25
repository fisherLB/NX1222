 


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
        public DbSet<AdminUser> AdminUser { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class AdminUser
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("AdminName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  AdminName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RealName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  RealName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Phone")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Phone { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Password")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Password { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Password2")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Password2 { get; set; }
		      
       
        
        /// <summary>
        /// 最后登录IP
        /// </summary>  
				[DisplayName("最后登录IP")]
		        [MaxLength(50,ErrorMessage="最后登录IP最大长度为50")]
		public string  LastLoginIP { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LastLoginTime")]
				public DateTime?  LastLoginTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RoleID")]
				public int  RoleID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsPassed")]
				public bool  IsPassed { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public AdminUser()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 业务接口
    /// </summary>
    public interface IAdminUserService :IServiceBase<AdminUser> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(AdminUser entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class AdminUserService :  ServiceBase<AdminUser>,IAdminUserService
    {


        public AdminUserService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(AdminUser entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
