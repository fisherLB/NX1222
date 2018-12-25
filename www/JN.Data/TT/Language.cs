 


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
        public DbSet<Language> Language { get; set; }
    }

	/// <summary>
    /// 语言资源表
    /// </summary>
	[DisplayName("语言资源表")]
    public partial class Language
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 页面
        /// </summary>  
				[DisplayName("页面")]
		        [MaxLength(250,ErrorMessage="页面最大长度为250")]
		public string  Location { get; set; }
		      
       
        
        /// <summary>
        /// 语言
        /// </summary>  
				[DisplayName("语言")]
		        [MaxLength(50,ErrorMessage="语言最大长度为50")]
		public string  LanguageName { get; set; }
		      
       
        
        /// <summary>
        /// 名称
        /// </summary>  
				[DisplayName("名称")]
		        [MaxLength(250,ErrorMessage="名称最大长度为250")]
		public string  Name { get; set; }
		      
       
        
        /// <summary>
        /// 值
        /// </summary>  
				[DisplayName("值")]
		        [MaxLength(250,ErrorMessage="值最大长度为250")]
		public string  Value { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public Language()
        {
        //    ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 语言资源表业务接口
    /// </summary>
    public interface ILanguageService :IServiceBase<Language> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(Language entity);
	}
    /// <summary>
    /// 语言资源表业务类
    /// </summary>
    public class LanguageService :  ServiceBase<Language>,ILanguageService
    {


        public LanguageService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(Language entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
