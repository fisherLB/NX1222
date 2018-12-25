 


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
        public DbSet<SysSetting> SysSetting { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class SysSetting
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 站点开启关闭
        /// </summary>  
				[DisplayName("站点开启关闭")]
				public bool  IsOpenUp { get; set; }
		      
       
        
        /// <summary>
        /// 站点关闭时提示
        /// </summary>  
				[DisplayName("站点关闭时提示")]
		        [MaxLength(250,ErrorMessage="站点关闭时提示最大长度为250")]
		public string  CloseHint { get; set; }
		      
       
        
        /// <summary>
        /// 系统名称
        /// </summary>  
				[DisplayName("系统名称")]
		        [MaxLength(50,ErrorMessage="系统名称最大长度为50")]
		public string  SysName { get; set; }
		      
       
        
        /// <summary>
        /// 站点标题
        /// </summary>  
				[DisplayName("站点标题")]
		        [MaxLength(50,ErrorMessage="站点标题最大长度为50")]
		public string  SiteTitle { get; set; }
		      
       
        
        /// <summary>
        /// 站点关键字
        /// </summary>  
				[DisplayName("站点关键字")]
		        [MaxLength(50,ErrorMessage="站点关键字最大长度为50")]
		public string  SiteKeywords { get; set; }
		      
       
        
        /// <summary>
        /// 站点描述
        /// </summary>  
				[DisplayName("站点描述")]
		        [MaxLength(50,ErrorMessage="站点描述最大长度为50")]
		public string  SiteDescription { get; set; }
		      
       
        
        /// <summary>
        /// 网站域名
        /// </summary>  
				[DisplayName("网站域名")]
		        [MaxLength(50,ErrorMessage="网站域名最大长度为50")]
		public string  SiteUrl { get; set; }
		      
       
        
        /// <summary>
        /// 版权信息
        /// </summary>  
				[DisplayName("版权信息")]
				public string  CopyInfo { get; set; }
		      
       
        
        /// <summary>
        /// 开启关闭注册
        /// </summary>  
				[DisplayName("开启关闭注册")]
				public bool  IsRegisterOpen { get; set; }
		      
       
        
        /// <summary>
        /// 关闭注册时提示
        /// </summary>  
				[DisplayName("关闭注册时提示")]
		        [MaxLength(250,ErrorMessage="关闭注册时提示最大长度为250")]
		public string  CloseRegisterHint { get; set; }
		      
       
        
        /// <summary>
        /// 管理员独占操作
        /// </summary>  
				[DisplayName("管理员独占操作")]
				public bool  AdminOneSelf { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsOpenKefu")]
				public bool  IsOpenKefu { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuUrl1")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  KefuUrl1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuName1")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  KefuName1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuUrl2")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  KefuUrl2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuName2")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  KefuName2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuUrl3")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  KefuUrl3 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuName3")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  KefuName3 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuUrl4")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  KefuUrl4 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuName4")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  KefuName4 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuUrl5")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  KefuUrl5 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuName5")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  KefuName5 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuTel1")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  KefuTel1 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuTel2")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  KefuTel2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("KefuTel3")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  KefuTel3 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LoginBackgroundImage")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  LoginBackgroundImage { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LogoImage")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  LogoImage { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BannerImage")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  BannerImage { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BannerImage2")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  BannerImage2 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("BannerImage3")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  BannerImage3 { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsWrongPwdLock")]
				public bool  IsWrongPwdLock { get; set; }
		      
       
        
        /// <summary>
        /// 匹配模式（0手动,1自动）
        /// </summary>  
				[DisplayName("匹配模式（0手动,1自动）")]
				public int?  MatchingMode { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LastupdateTime")]
				public DateTime?  LastupdateTime { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
		        [MaxLength(50,ErrorMessage="预留字段最大长度为50")]
		public string  ReserveStr1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
		        [MaxLength(50,ErrorMessage="预留字段最大长度为50")]
		public string  ReserveStr2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
		        [MaxLength(50,ErrorMessage="预留字段最大长度为50")]
		public string  ReserveStr3 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
				public int?  ReserveInt1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
				public int?  ReserveInt2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
				public int?  ReserveInt3 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
				public DateTime?  ReserveDate1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
				public DateTime?  ReserveDate2 { get; set; }
		      
       
        
        /// <summary>
        /// 注册填写项
        /// </summary>  
				[DisplayName("注册填写项")]
		        [MaxLength(250,ErrorMessage="注册填写项最大长度为250")]
		public string  RegistItems { get; set; }
		      
       
        
        /// <summary>
        /// 注册必填项
        /// </summary>  
				[DisplayName("注册必填项")]
		        [MaxLength(250,ErrorMessage="注册必填项最大长度为250")]
		public string  RegistNotNullItems { get; set; }
		      
       
        
        /// <summary>
        /// 注册唯一项（如手机、身份证）
        /// </summary>  
				[DisplayName("注册唯一项（如手机、身份证）")]
		        [MaxLength(250,ErrorMessage="注册唯一项（如手机、身份证）最大长度为250")]
		public string  RegistOnlyOneItems { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("RegistNeedSMSVerification")]
				public bool?  RegistNeedSMSVerification { get; set; }
		      
       
        
        /// <summary>
        /// 取回密码方式（短信/邮箱/密保）
        /// </summary>  
				[DisplayName("取回密码方式（短信/邮箱/密保）")]
		        [MaxLength(10,ErrorMessage="取回密码方式（短信/邮箱/密保）最大长度为10")]
		public string  RetrievePasswordType { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("VerificationCodeType")]
		        [MaxLength(10,ErrorMessage="最大长度为10")]
		public string  VerificationCodeType { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("VerificationCodeLength")]
				public int?  VerificationCodeLength { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SmtpServer")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SmtpServer { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SmtpEmailAddress")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SmtpEmailAddress { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SmtpUserName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SmtpUserName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SmtpPassword")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SmtpPassword { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SmtpPort")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SmtpPort { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SmtpSSL")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SmtpSSL { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SMSSerivce")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SMSSerivce { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SMSUid")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  SMSUid { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("SMSKey")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  SMSKey { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("DevelopMode")]
				public int?  DevelopMode { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Theme")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Theme { get; set; }
		      
       
        
        /// <summary>
        /// 用于接收系统短信的手机号
        /// </summary>  
				[DisplayName("用于接收系统短信的手机号")]
		        [MaxLength(50,ErrorMessage="用于接收系统短信的手机号最大长度为50")]
		public string  SysMobile { get; set; }
		      
       
        
        /// <summary>
        /// 严重预警时是否发送短信
        /// </summary>  
				[DisplayName("严重预警时是否发送短信")]
				public bool?  IsWarnningSMS { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("MostUseCity")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  MostUseCity { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public SysSetting()
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
    public interface ISysSettingService :IServiceBase<SysSetting> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(SysSetting entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class SysSettingService :  ServiceBase<SysSetting>,ISysSettingService
    {


        public SysSettingService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(SysSetting entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
