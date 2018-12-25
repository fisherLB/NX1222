 


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
        public DbSet<Matching> Matching { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class Matching
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 订单号
        /// </summary>  
				[DisplayName("订单号")]
		        [MaxLength(50,ErrorMessage="订单号最大长度为50")]
		public string  MatchingNo { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("AcceptNo")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  AcceptNo { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SupplyNo")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SupplyNo { get; set; }
		      
       
        
        /// <summary>
        /// 提供帮助的ID
        /// </summary>  
				[DisplayName("提供帮助的ID")]
				public int  SupplyUID { get; set; }
		      
       
        
        /// <summary>
        /// 提供帮助的用户名（编号）
        /// </summary>  
				[DisplayName("提供帮助的用户名（编号）")]
		        [MaxLength(50,ErrorMessage="提供帮助的用户名（编号）最大长度为50")]
		public string  SupplyUserName { get; set; }
		      
       
        
        /// <summary>
        /// 接受帮助Id号
        /// </summary>  
				[DisplayName("接受帮助Id号")]
				public int  AcceptUID { get; set; }
		      
       
        
        /// <summary>
        /// 接受帮助用户名
        /// </summary>  
				[DisplayName("接受帮助用户名")]
		        [MaxLength(50,ErrorMessage="接受帮助用户名最大长度为50")]
		public string  AcceptUserName { get; set; }
		      
       
        
        /// <summary>
        /// 交易金额
        /// </summary>  
				[DisplayName("交易金额")]
		        [Filters.DecimalPrecision(18,0)]
		public decimal  MatchAmount { get; set; }
		      
       
        
        /// <summary>
        /// 当前状态
        /// </summary>  
				[DisplayName("当前状态")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 提供图片（证据）
        /// </summary>  
				[DisplayName("提供图片（证据）")]
		        [MaxLength(255,ErrorMessage="提供图片（证据）最大长度为255")]
		public string  ProofImageUrl { get; set; }
		      
       
        
        /// <summary>
        /// 付款时间
        /// </summary>  
				[DisplayName("付款时间")]
				public DateTime?  PayTime { get; set; }
		      
       
        
        /// <summary>
        /// 付款时限
        /// </summary>  
				[DisplayName("付款时限")]
				public DateTime?  PayEndTime { get; set; }
		      
       
        
        /// <summary>
        /// 创建时间
        /// </summary>  
				[DisplayName("创建时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
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
        /// 确认收款时限
        /// </summary>  
				[DisplayName("确认收款时限")]
				public DateTime?  VerifiedEndTime { get; set; }
		      
       
        
        /// <summary>
        /// 取消时间
        /// </summary>  
				[DisplayName("取消时间")]
				public DateTime?  CancelTime { get; set; }
		      
       
        
        /// <summary>
        /// 取消原因
        /// </summary>  
				[DisplayName("取消原因")]
		        [MaxLength(50,ErrorMessage="取消原因最大长度为50")]
		public string  CanceReason { get; set; }
		      
       
        
        /// <summary>
        /// 全部成交时间
        /// </summary>  
				[DisplayName("全部成交时间")]
				public DateTime?  AllDealTime { get; set; }
		      
       
        
        /// <summary>
        /// 备注
        /// </summary>  
				[DisplayName("备注")]
				public string  Remark { get; set; }
		      
       
        
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
        /// 
        /// </summary>  
				[DisplayName("FromUID")]
				public int?  FromUID { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("FromUserName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  FromUserName { get; set; }
		      
       
        
        /// <summary>
        /// 是否进入抢单池
        /// </summary>  
				[DisplayName("是否进入抢单池")]
				public bool?  IsOpenBuying { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsHide")]
				public bool?  IsHide { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Matching()
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
    public interface IMatchingService :IServiceBase<Matching> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Matching entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class MatchingService :  ServiceBase<Matching>,IMatchingService
    {


        public MatchingService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Matching entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
