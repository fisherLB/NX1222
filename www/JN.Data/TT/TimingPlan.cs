 


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
        public DbSet<TimingPlan> TimingPlan { get; set; }
    }

	/// <summary>
    /// 
    /// </summary>
	[DisplayName("")]
    public partial class TimingPlan
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// (0:每天,1:每周,2:每月)
        /// </summary>  
				[DisplayName("(0:每天,1:每周,2:每月)")]
				public int  CycleType { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("CycleValue")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  CycleValue { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("PlanTime")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  PlanTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("PlanName")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  PlanName { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Status")]
				public int  Status { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Remark")]
		        [MaxLength(250,ErrorMessage="最大长度为250")]
		public string  Remark { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public TimingPlan()
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
    public interface ITimingPlanService :IServiceBase<TimingPlan> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(TimingPlan entity);
	}
    /// <summary>
    /// 业务类
    /// </summary>
    public class TimingPlanService :  ServiceBase<TimingPlan>,ITimingPlanService
    {


        public TimingPlanService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(TimingPlan entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
