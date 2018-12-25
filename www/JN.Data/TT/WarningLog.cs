 


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
        public DbSet<WarningLog> WarningLog { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class WarningLog
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 平台
        /// </summary>  
				[DisplayName("平台")]
		        [MaxLength(50,ErrorMessage="平台最大长度为50")]
		public string  Platform { get; set; }
		      
       
        
        /// <summary>
        /// 帐号
        /// </summary>  
				[DisplayName("帐号")]
		        [MaxLength(50,ErrorMessage="帐号最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 警告内容
        /// </summary>  
				[DisplayName("警告内容")]
		        [MaxLength(250,ErrorMessage="警告内容最大长度为250")]
		public string  WarningMsg { get; set; }
		      
       
        
        /// <summary>
        /// 警告级别(普通,严重,特别严重)
        /// </summary>  
				[DisplayName("警告级别(普通,严重,特别严重)")]
		        [MaxLength(50,ErrorMessage="警告级别(普通,严重,特别严重)最大长度为50")]
		public string  WarningLevel { get; set; }
		      
       
        
        /// <summary>
        /// 处理结果
        /// </summary>  
				[DisplayName("处理结果")]
		        [MaxLength(250,ErrorMessage="处理结果最大长度为250")]
		public string  ResultMsg { get; set; }
		      
       
        
        /// <summary>
        /// 访问地址
        /// </summary>  
				[DisplayName("访问地址")]
		        [MaxLength(250,ErrorMessage="访问地址最大长度为250")]
		public string  Location { get; set; }
		      
       
        
        /// <summary>
        /// IP地址
        /// </summary>  
				[DisplayName("IP地址")]
		        [MaxLength(50,ErrorMessage="IP地址最大长度为50")]
		public string  IP { get; set; }
		      
       
        
        /// <summary>
        /// 操作时间
        /// </summary>  
				[DisplayName("操作时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public WarningLog()
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
    public interface IWarningLogService :IServiceBase<WarningLog> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(WarningLog entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class WarningLogService :  ServiceBase<WarningLog>,IWarningLogService
    {


        public WarningLogService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(WarningLog entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
