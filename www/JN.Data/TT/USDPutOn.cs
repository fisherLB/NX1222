 


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
        public DbSet<USDPutOn> USDPutOn { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class USDPutOn
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
				[DisplayName("PutonNumber")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  PutonNumber { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("UID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 挂单会员
        /// </summary>  
				[DisplayName("挂单会员")]
		        [MaxLength(50,ErrorMessage="挂单会员最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SeekID")]
				public int?  SeekID { get; set; }
		      
       
        
        /// <summary>
        /// 手续费
        /// </summary>  
				[DisplayName("手续费")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  Poundage { get; set; }
		      
       
        
        /// <summary>
        /// 申请挂单金币
        /// </summary>  
				[DisplayName("申请挂单金币")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  PutAmount { get; set; }
		      
       
        
        /// <summary>
        /// 卖出金额
        /// </summary>  
				[DisplayName("卖出金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  PutMoney { get; set; }
		      
       
        
        /// <summary>
        /// 已成交金币
        /// </summary>  
				[DisplayName("已成交金币")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  DealAmount { get; set; }
		      
       
        
        /// <summary>
        /// 货币类型
        /// </summary>  
				[DisplayName("货币类型")]
		        [MaxLength(50,ErrorMessage="货币类型最大长度为50")]
		public string  MoneyType { get; set; }
		      
       
        
        /// <summary>
        /// 联系电话
        /// </summary>  
				[DisplayName("联系电话")]
		        [MaxLength(50,ErrorMessage="联系电话最大长度为50")]
		public string  Phone { get; set; }
		      
       
        
        /// <summary>
        /// 付款类型
        /// </summary>  
				[DisplayName("付款类型")]
		        [MaxLength(50,ErrorMessage="付款类型最大长度为50")]
		public string  PayType { get; set; }
		      
       
        
        /// <summary>
        /// 备注
        /// </summary>  
				[DisplayName("备注")]
		        [MaxLength(250,ErrorMessage="备注最大长度为250")]
		public string  Remark { get; set; }
		      
       
        
        /// <summary>
        /// 1未成交,2部分成功,3全部成交
        /// </summary>  
				[DisplayName("1未成交,2部分成功,3全部成交")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
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
        /// 
        /// </summary>  
				[DisplayName("IsTop")]
				public bool?  IsTop { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public USDPutOn()
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
    public interface IUSDPutOnService :IServiceBase<USDPutOn> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(USDPutOn entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class USDPutOnService :  ServiceBase<USDPutOn>,IUSDPutOnService
    {


        public USDPutOnService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(USDPutOn entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
