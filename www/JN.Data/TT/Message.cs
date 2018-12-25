 


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
        public DbSet<Message> Message { get; set; }
    }

	/// <summary>
    /// 会员短信息表
    /// </summary>
	[DisplayName("会员短信息表")]
    public partial class Message
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 用户ID
        /// </summary>  
				[DisplayName("用户ID")]
				public int  UID { get; set; }
		      
       
        
        /// <summary>
        /// 发出ID
        /// </summary>  
				[DisplayName("发出ID")]
				public int  FormUID { get; set; }
		      
       
        
        /// <summary>
        /// 发出
        /// </summary>  
				[DisplayName("发出")]
		        [MaxLength(50,ErrorMessage="发出最大长度为50")]
		public string  FormUserName { get; set; }
		      
       
        
        /// <summary>
        /// 发送到ID
        /// </summary>  
				[DisplayName("发送到ID")]
				public int  ToUID { get; set; }
		      
       
        
        /// <summary>
        /// 发送到
        /// </summary>  
				[DisplayName("发送到")]
		        [MaxLength(50,ErrorMessage="发送到最大长度为50")]
		public string  ToUserName { get; set; }
		      
       
        
        /// <summary>
        /// 邮件主题
        /// </summary>  
				[DisplayName("邮件主题")]
		        [MaxLength(50,ErrorMessage="邮件主题最大长度为50")]
		public string  Title { get; set; }
		      
       
        
        /// <summary>
        /// 留言选项
        /// </summary>  
				[DisplayName("留言选项")]
		        [MaxLength(50,ErrorMessage="留言选项最大长度为50")]
		public string  MessageType { get; set; }
		      
       
        
        /// <summary>
        /// 邮件内容
        /// </summary>  
				[DisplayName("邮件内容")]
		        [MaxLength(1000,ErrorMessage="邮件内容最大长度为1000")]
		public string  Content { get; set; }
		      
       
        
        /// <summary>
        /// 附件
        /// </summary>  
				[DisplayName("附件")]
		        [MaxLength(250,ErrorMessage="附件最大长度为250")]
		public string  Attachment { get; set; }
		      
       
        
        /// <summary>
        /// 是否已读
        /// </summary>  
				[DisplayName("是否已读")]
				public bool  IsRead { get; set; }
		      
       
        
        /// <summary>
        /// 是否标记
        /// </summary>  
				[DisplayName("是否标记")]
				public bool  IsFlag { get; set; }
		      
       
        
        /// <summary>
        /// 是否加星
        /// </summary>  
				[DisplayName("是否加星")]
				public bool  IsStar { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CreateTime")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 是否回复
        /// </summary>  
				[DisplayName("是否回复")]
				public bool  IsReply { get; set; }
		      
       
        
        /// <summary>
        /// 回复ID
        /// </summary>  
				[DisplayName("回复ID")]
				public int  ReplyID { get; set; }
		      
       
        
        /// <summary>
        /// 是否转送
        /// </summary>  
				[DisplayName("是否转送")]
				public bool  IsForward { get; set; }
		      
       
        
        /// <summary>
        /// 转送ID
        /// </summary>  
				[DisplayName("转送ID")]
				public int  ForwardID { get; set; }
		      
       
        
        /// <summary>
        /// 是否送出（Flase时为草稿）
        /// </summary>  
				[DisplayName("是否送出（Flase时为草稿）")]
				public bool  IsSendSuccessful { get; set; }
		      
       
        
        /// <summary>
        /// 关联ID
        /// </summary>  
				[DisplayName("关联ID")]
				public int  RelationID { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Message()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 会员短信息表业务接口
    /// </summary>
    public interface IMessageService :IServiceBase<Message> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Message entity);
	}
    /// <summary>
    /// 会员短信息表业务类
    /// </summary>
    public class MessageService :  ServiceBase<Message>,IMessageService
    {


        public MessageService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Message entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
 
