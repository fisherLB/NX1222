 


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
        public DbSet<Settlement> Settlement { get; set; }
    }

	/// <summary>
    /// 结算表
    /// </summary>
	[DisplayName("结算表")]
    public partial class Settlement
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
				[DisplayName("Period")]
				public int  Period { get; set; }
		      
       
        
        /// <summary>
        /// 分红金额
        /// </summary>  
				[DisplayName("分红金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  Bonus { get; set; }
		      
       
        
        /// <summary>
        /// 结算方式（0系统,1手工）
        /// </summary>  
				[DisplayName("结算方式（0系统,1手工）")]
				public int  BalanceMode { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TotalUser")]
				public int  TotalUser { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TotalBonus")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  TotalBonus { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
		        [MaxLength(50,ErrorMessage="预留自段最大长度为50")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
		        [MaxLength(50,ErrorMessage="预留自段最大长度为50")]
		public string  ReserveStr2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
				public int?  ReserveInt1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
				public int?  ReserveInt2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
				public DateTime?  ReserveDate { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
				public bool?  ReserveBoolean { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal3 { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Settlement()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 结算表业务接口
    /// </summary>
    public interface ISettlementService :IServiceBase<Settlement> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Settlement entity);
	}
    /// <summary>
    /// 结算表业务类
    /// </summary>
    public class SettlementService :  ServiceBase<Settlement>,ISettlementService
    {


        public SettlementService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Settlement entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
