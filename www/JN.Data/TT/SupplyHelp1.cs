 


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
        public DbSet<SupplyHelp> SupplyHelp { get; set; }
    }

	/// <summary>
    /// 市场交易卖出表
    /// </summary>
	[DisplayName("市场交易卖出表")]
    public partial class SupplyHelp
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
				[DisplayName("MainSupplyID")]
				public int  MainSupplyID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SupplyNo")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SupplyNo { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UserName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 提供帮助金额
        /// </summary>  
				[DisplayName("提供帮助金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  SupplyAmount { get; set; }
		      
       
        
        /// <summary>
        /// 汇率金额
        /// </summary>  
				[DisplayName("汇率金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  ExchangeAmount { get; set; }
		      
       
        
        /// <summary>
        /// 已匹配金额
        /// </summary>  
				[DisplayName("已匹配金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  HaveMatchingAmount { get; set; }
		      
       
        
        /// <summary>
        /// 已被接受帮助的额度
        /// </summary>  
				[DisplayName("已被接受帮助的额度")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  HaveAcceptAmount { get; set; }
		      
       
        
        /// <summary>
        /// 1未匹配,2部分匹配,3全部匹配
        /// </summary>  
				[DisplayName("1未匹配,2部分匹配,3全部匹配")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 付款方式
        /// </summary>  
				[DisplayName("付款方式")]
		        [MaxLength(50,ErrorMessage="付款方式最大长度为50")]
		public string  PayWay { get; set; }
		      
       
        
        /// <summary>
        /// 是否置顶
        /// </summary>  
				[DisplayName("是否置顶")]
				public bool  IsTop { get; set; }
		      
       
        
        /// <summary>
        /// 是否已经重复排队
        /// </summary>  
				[DisplayName("是否已经重复排队")]
				public bool  IsRepeatQueuing { get; set; }
		      
       
        
        /// <summary>
        /// 到期时间
        /// </summary>  
				[DisplayName("到期时间")]
				public DateTime  EndTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RepeatQueuingTime")]
				public DateTime?  RepeatQueuingTime { get; set; }
		      
       
        
        /// <summary>
        /// 订单总额（含利息）
        /// </summary>  
				[DisplayName("订单总额（含利息）")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  TotalMoney { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 利息
        /// </summary>  
				[DisplayName("利息")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  AccrualMoney { get; set; }
		      
       
        
        /// <summary>
        /// 利息天数
        /// </summary>  
				[DisplayName("利息天数")]
				public int  AccrualDay { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SurplusAccrualDay")]
				public int  SurplusAccrualDay { get; set; }
		      
       
        
        /// <summary>
        /// 订单日利率
        /// </summary>  
				[DisplayName("订单日利率")]
		        [Filters.DecimalPrecision(18,3)]
		public decimal?  AccruaRate { get; set; }
		      
       
        
        /// <summary>
        /// 利息是否生效
        /// </summary>  
				[DisplayName("利息是否生效")]
				public bool  IsAccrualEffective { get; set; }
		      
       
        
        /// <summary>
        /// 是否还计算利息
        /// </summary>  
				[DisplayName("是否还计算利息")]
				public bool  IsAccruaCount { get; set; }
		      
       
        
        /// <summary>
        /// 利息停止原因
        /// </summary>  
				[DisplayName("利息停止原因")]
		        [MaxLength(50,ErrorMessage="利息停止原因最大长度为50")]
		public string  AccrualStopReason { get; set; }
		      
       
        
        /// <summary>
        /// 省份
        /// </summary>  
				[DisplayName("省份")]
		        [MaxLength(50,ErrorMessage="省份最大长度为50")]
		public string  Province { get; set; }
		      
       
        
        /// <summary>
        /// 城市
        /// </summary>  
				[DisplayName("城市")]
		        [MaxLength(50,ErrorMessage="城市最大长度为50")]
		public string  City { get; set; }
		      
       
        
        /// <summary>
        /// 区县
        /// </summary>  
				[DisplayName("区县")]
		        [MaxLength(50,ErrorMessage="区县最大长度为50")]
		public string  County { get; set; }
		      
       
        
        /// <summary>
        /// 取消时间
        /// </summary>  
				[DisplayName("取消时间")]
				public DateTime?  CancelTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CancelReason")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  CancelReason { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
		        [MaxLength(50,ErrorMessage="预留最大长度为50")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
		        [MaxLength(50,ErrorMessage="预留最大长度为50")]
		public string  ReserveStr2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
				public int?  ReserveInt1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
				public int?  ReserveInt2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
				public DateTime?  ReserveDate1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
				public DateTime?  ReserveDate2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
				public bool?  ReserveBoolean1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
				public bool?  ReserveBoolean2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留
        /// </summary>  
				[DisplayName("预留")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal2 { get; set; }
		      
       
        
        /// <summary>
        /// 订单类型（0预定金,1全额单）
        /// </summary>  
				[DisplayName("订单类型（0预定金,1全额单）")]
				public int?  OrderType { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("OrderMoney")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  OrderMoney { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Remark")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsAgent")]
				public bool?  IsAgent { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RoundAmount")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  RoundAmount { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public SupplyHelp()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 市场交易卖出表业务接口
    /// </summary>
    public interface ISupplyHelpService :IServiceBase<SupplyHelp> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(SupplyHelp entity);
	}
    /// <summary>
    /// 市场交易卖出表业务类
    /// </summary>
    public class SupplyHelpService :  ServiceBase<SupplyHelp>,ISupplyHelpService
    {


        public SupplyHelpService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(SupplyHelp entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
