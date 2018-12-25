 


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
        public DbSet<AcceptHelp> AcceptHelp { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class AcceptHelp
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 接受帮助编号
        /// </summary>  
				[DisplayName("接受帮助编号")]
		        [MaxLength(50,ErrorMessage="接受帮助编号最大长度为50")]
		public string  AcceptNo { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名
        /// </summary>  
				[DisplayName("用户名")]
		        [MaxLength(50,ErrorMessage="用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 币种ID（钱包）
        /// </summary>  
				[DisplayName("币种ID（钱包）")]
				public int  CoinID { get; set; }
		      
       
        
        /// <summary>
        /// 币种（钱包）
        /// </summary>  
				[DisplayName("币种（钱包）")]
		        [MaxLength(50,ErrorMessage="币种（钱包）最大长度为50")]
		public string  CoinName { get; set; }
		      
       
        
        /// <summary>
        /// 接受帮助金额
        /// </summary>  
				[DisplayName("接受帮助金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  AcceptAmount { get; set; }
		      
       
        
        /// <summary>
        /// 接受帮助汇率金额
        /// </summary>  
				[DisplayName("接受帮助汇率金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  ExchangeAmount { get; set; }
		      
       
        
        /// <summary>
        /// 已匹配金额
        /// </summary>  
				[DisplayName("已匹配金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  HaveMatchingAmount { get; set; }
		      
       
        
        /// <summary>
        /// 接供帮助的单号集合
        /// </summary>  
				[DisplayName("接供帮助的单号集合")]
		        [MaxLength(250,ErrorMessage="接供帮助的单号集合最大长度为250")]
		public string  UseSupplyNo { get; set; }
		      
       
        
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
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 重复排队时间
        /// </summary>  
				[DisplayName("重复排队时间")]
				public DateTime?  RepeatQueuingTime { get; set; }
		      
       
        
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
				public DateTime?  ReserveDate1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
				public DateTime?  ReserveDate2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
				public bool?  ReserveBoolean1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留自段
        /// </summary>  
				[DisplayName("预留自段")]
				public bool?  ReserveBoolean2 { get; set; }
		      
       
        
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
        /// 全部成交时间
        /// </summary>  
				[DisplayName("全部成交时间")]
				public DateTime?  AllDealTime { get; set; }
		      
       
        
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
        /// 构造函数
        /// </summary>
		
        public AcceptHelp()
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
    public interface IAcceptHelpService :IServiceBase<AcceptHelp> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(AcceptHelp entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class AcceptHelpService :  ServiceBase<AcceptHelp>,IAcceptHelpService
    {


        public AcceptHelpService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(AcceptHelp entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
