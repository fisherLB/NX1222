 


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
        public DbSet<SysParam> SysParam { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class SysParam
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
				[DisplayName("PID")]
				public int  PID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Name")]
		        [MaxLength(100,ErrorMessage="最大长度为100")]
		public string  Name { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Value")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Value { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ValueType")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ValueType { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Value2")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Value2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Value2Type")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Value2Type { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Value3")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Value3 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Value3Type")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Value3Type { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Sort")]
				public int  Sort { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("InputFormat")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  InputFormat { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Remark")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsLock")]
				public bool  IsLock { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsUse")]
				public bool  IsUse { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LastUpdateTime")]
				public DateTime?  LastUpdateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public SysParam()
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
    public interface ISysParamService :IServiceBase<SysParam> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(SysParam entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class SysParamService :  ServiceBase<SysParam>,ISysParamService
    {


        public SysParamService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(SysParam entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
