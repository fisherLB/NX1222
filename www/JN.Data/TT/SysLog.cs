 


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
        public DbSet<SysLog> SysLog { get; set; }
    }

	/// <summary>
    /// 系统运行日志
    /// </summary>
	[DisplayName("系统运行日志")]
    public partial class SysLog
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 程序BUG,程序警告
        /// </summary>  
				[DisplayName("程序BUG,程序警告")]
		        [MaxLength(50,ErrorMessage="程序BUG,程序警告最大长度为50")]
		public string  Type { get; set; }
		      
       
        
        /// <summary>
        /// 内容
        /// </summary>  
				[DisplayName("内容")]
		        [MaxLength(8,ErrorMessage="内容最大长度为8")]
		public string  Content { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public SysLog()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 系统运行日志业务接口
    /// </summary>
    public interface ISysLogService :IServiceBase<SysLog> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(SysLog entity);
	}
    /// <summary>
    /// 系统运行日志业务类
    /// </summary>
    public class SysLogService :  ServiceBase<SysLog>,ISysLogService
    {


        public SysLogService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(SysLog entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
