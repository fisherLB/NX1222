 


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
        public DbSet<LeaveWord> LeaveWord { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class LeaveWord
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 匹配单号
        /// </summary>  
				[DisplayName("匹配单号")]
		        [MaxLength(50,ErrorMessage="匹配单号最大长度为50")]
		public string  MatchingNo { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Content")]
				public string  Content { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 留言用户名
        /// </summary>  
				[DisplayName("留言用户名")]
		        [MaxLength(50,ErrorMessage="留言用户名最大长度为50")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MsgType")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  MsgType { get; set; }
		      
       
        
        /// <summary>
        /// 附件
        /// </summary>  
				[DisplayName("附件")]
		        [MaxLength(250,ErrorMessage="附件最大长度为250")]
		public string  Attachment { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public LeaveWord()
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
    public interface ILeaveWordService :IServiceBase<LeaveWord> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(LeaveWord entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class LeaveWordService :  ServiceBase<LeaveWord>,ILeaveWordService
    {


        public LeaveWordService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(LeaveWord entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
