 


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
        public DbSet<ActLog> ActLog { get; set; }
    }

	/// <summary>
    /// 用户/管理员行为日志
    /// </summary>
	[DisplayName("用户/管理员行为日志")]
    public partial class ActLog
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
        /// <summary>
        /// 平台(用户,管理员)
        /// </summary>  
				[DisplayName("平台(用户,管理员)")]
		        [MaxLength(50,ErrorMessage="平台(用户,管理员)最大长度为50")]
		public string  Platform { get; set; }
		      
       
        
        /// <summary>
        /// 会员ID/管理员ID
        /// </summary>  
				[DisplayName("会员ID/管理员ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名/管理员
        /// </summary>  
				[DisplayName("用户名/管理员")]
		        [MaxLength(50,ErrorMessage="用户名/管理员最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 来路
        /// </summary>  
				[DisplayName("来路")]
		        [MaxLength(250,ErrorMessage="来路最大长度为250")]
		public string  Source { get; set; }
		      
       
        
        /// <summary>
        /// 访问地址
        /// </summary>  
				[DisplayName("访问地址")]
		        [MaxLength(250,ErrorMessage="访问地址最大长度为250")]
		public string  Location { get; set; }
		      
       
        
        /// <summary>
        /// 行为内容
        /// </summary>  
				[DisplayName("行为内容")]
				public string  ActContent { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("PacketFile")]
				public string  PacketFile { get; set; }
		      
       
        
        /// <summary>
        /// IP
        /// </summary>  
				[DisplayName("IP")]
		        [MaxLength(50,ErrorMessage="IP最大长度为50")]
		public string  IP { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public ActLog()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 用户/管理员行为日志业务接口
    /// </summary>
    public interface IActLogService :IServiceBase<ActLog> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(ActLog entity);
	}
    /// <summary>
    /// 用户/管理员行为日志业务类
    /// </summary>
    public class ActLogService :  ServiceBase<ActLog>,IActLogService
    {


        public ActLogService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(ActLog entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
