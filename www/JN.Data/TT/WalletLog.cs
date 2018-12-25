 


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
        public DbSet<WalletLog> WalletLog { get; set; }
    }

	/// <summary>
    /// 帐户明细表
    /// </summary>
	[DisplayName("帐户明细表")]
    public partial class WalletLog
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public long  ID { get; set; }
		      
       
        
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
        /// 币种ID
        /// </summary>  
				[DisplayName("币种ID")]
				public int  CoinID { get; set; }
		      
       
        
        /// <summary>
        /// 币种名称
        /// </summary>  
				[DisplayName("币种名称")]
		        [MaxLength(50,ErrorMessage="币种名称最大长度为50")]
		public string  CoinName { get; set; }
		      
       
        
        /// <summary>
        /// 变更金额
        /// </summary>  
				[DisplayName("变更金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  ChangeMoney { get; set; }
		      
       
        
        /// <summary>
        /// 余额
        /// </summary>  
				[DisplayName("余额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  Balance { get; set; }
		      
       
        
        /// <summary>
        /// 描述
        /// </summary>  
				[DisplayName("描述")]
		        [MaxLength(250,ErrorMessage="描述最大长度为250")]
		public string  Description { get; set; }
		      
       
        
        /// <summary>
        /// 变更时间
        /// </summary>  
				[DisplayName("变更时间")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public WalletLog()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 帐户明细表业务接口
    /// </summary>
    public interface IWalletLogService :IServiceBase<WalletLog> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(WalletLog entity);
	}
    /// <summary>
    /// 帐户明细表业务类
    /// </summary>
    public class WalletLogService :  ServiceBase<WalletLog>,IWalletLogService
    {


        public WalletLogService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(WalletLog entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
