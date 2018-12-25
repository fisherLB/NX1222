 


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
        public DbSet<PINCode> PINCode { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class PINCode
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
				[DisplayName("UID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 拥有者
        /// </summary>  
				[DisplayName("拥有者")]
		        [MaxLength(50,ErrorMessage="拥有者最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 激活码
        /// </summary>  
				[DisplayName("激活码")]
		        [MaxLength(50,ErrorMessage="激活码最大长度为50")]
		public string  KeyCode { get; set; }
		      
       
        
        /// <summary>
        /// 激活码适用金额
        /// </summary>  
				[DisplayName("激活码适用金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  Investment { get; set; }
		      
       
        
        /// <summary>
        /// 是否使用
        /// </summary>  
				[DisplayName("是否使用")]
				public bool  IsUsed { get; set; }
		      
       
        
        /// <summary>
        /// 原用户ID
        /// </summary>  
				[DisplayName("原用户ID")]
				public int?  OriginUID { get; set; }
		      
       
        
        /// <summary>
        /// 原用户
        /// </summary>  
				[DisplayName("原用户")]
		        [MaxLength(50,ErrorMessage="原用户最大长度为50")]
		public string  OriginUserName { get; set; }
		      
       
        
        /// <summary>
        /// 使用者ID
        /// </summary>  
				[DisplayName("使用者ID")]
				public int?  UseUID { get; set; }
		      
       
        
        /// <summary>
        /// 使用者
        /// </summary>  
				[DisplayName("使用者")]
		        [MaxLength(50,ErrorMessage="使用者最大长度为50")]
		public string  UseUserName { get; set; }
		      
       
        
        /// <summary>
        /// 使用时间
        /// </summary>  
				[DisplayName("使用时间")]
				public DateTime?  UseTime { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 来源
        /// </summary>  
				[DisplayName("来源")]
		        [MaxLength(50,ErrorMessage="来源最大长度为50")]
		public string  OriginDescribe { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public PINCode()
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
    public interface IPINCodeService :IServiceBase<PINCode> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(PINCode entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class PINCodeService :  ServiceBase<PINCode>,IPINCodeService
    {


        public PINCodeService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(PINCode entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
