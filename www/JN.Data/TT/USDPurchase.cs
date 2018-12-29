 


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
        public DbSet<USDPurchase> USDPurchase { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class USDPurchase
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
				[DisplayName("OrderNumber")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  OrderNumber { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名
        /// </summary>  
				[DisplayName("用户名")]
		        [MaxLength(50,ErrorMessage="用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("AgentUser")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  AgentUser { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SellUID")]
				public int  SellUID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SellUserName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SellUserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SeekID")]
				public int?  SeekID { get; set; }
		      
       
        
        /// <summary>
        /// 购买的挂单ID
        /// </summary>  
				[DisplayName("购买的挂单ID")]
				public int  PutOnID { get; set; }
		      
       
        
        /// <summary>
        /// 购买数量
        /// </summary>  
				[DisplayName("购买数量")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  BuyAmount { get; set; }
		      
       
        
        /// <summary>
        /// 购买金额
        /// </summary>  
				[DisplayName("购买金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  BuyMoney { get; set; }
		      
       
        
        /// <summary>
        /// 1未付款,2已付款,3已成交
        /// </summary>  
				[DisplayName("1未付款,2已付款,3已成交")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 付款时间
        /// </summary>  
				[DisplayName("付款时间")]
				public DateTime?  PayTime { get; set; }
		      
       
        
        /// <summary>
        /// 付款备注
        /// </summary>  
				[DisplayName("付款备注")]
		        [MaxLength(250,ErrorMessage="付款备注最大长度为250")]
		public string  PayRemark { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("PayImg")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  PayImg { get; set; }
		      
       
        
        /// <summary>
        /// 购买时间
        /// </summary>  
				[DisplayName("购买时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr1")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveStr2")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  ReserveStr2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveInt")]
				public int?  ReserveInt { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDecamal")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ReserveDate")]
				public DateTime?  ReserveDate { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public USDPurchase()
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
    public interface IUSDPurchaseService :IServiceBase<USDPurchase> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(USDPurchase entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class USDPurchaseService :  ServiceBase<USDPurchase>,IUSDPurchaseService
    {


        public USDPurchaseService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(USDPurchase entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
