 


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
        public DbSet<User> User { get; set; }
    }

	/// <summary>
    /// 会员表
    /// </summary>
	[DisplayName("会员表")]
    public partial class User
    {

		
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("ID")]
				[Key]
		public int  ID { get; set; }
		      
       
        
        /// <summary>
        /// 用户名
        /// </summary>  
				[DisplayName("用户名")]
		        [MaxLength(20,ErrorMessage="用户名最大长度为20")]
		public string  UserName { get; set; }
		      
       
        
        /// <summary>
        /// 推荐人ID
        /// </summary>  
				[DisplayName("推荐人ID")]
				public int  RefereeID { get; set; }
		      
       
        
        /// <summary>
        /// 推荐人
        /// </summary>  
				[DisplayName("推荐人")]
		        [MaxLength(50,ErrorMessage="推荐人最大长度为50")]
		public string  RefereeUser { get; set; }
		      
       
        
        /// <summary>
        /// 推荐关系路径
        /// </summary>  
				[DisplayName("推荐关系路径")]
				public string  RefereePath { get; set; }
		      
       
        
        /// <summary>
        /// 推荐关系层
        /// </summary>  
				[DisplayName("推荐关系层")]
				public int  RefereeDepth { get; set; }
		      
       
        
        /// <summary>
        /// 所属商务中心ID
        /// </summary>  
				[DisplayName("所属商务中心ID")]
				public int  AgentID { get; set; }
		      
       
        
        /// <summary>
        /// 所属商务中心
        /// </summary>  
				[DisplayName("所属商务中心")]
		        [MaxLength(50,ErrorMessage="所属商务中心最大长度为50")]
		public string  AgentUser { get; set; }
		      
       
        
        /// <summary>
        /// 安置人ID
        /// </summary>  
				[DisplayName("安置人ID")]
				public int  ParentID { get; set; }
		      
       
        
        /// <summary>
        /// 安置用户名
        /// </summary>  
				[DisplayName("安置用户名")]
		        [MaxLength(50,ErrorMessage="安置用户名最大长度为50")]
		public string  ParentUser { get; set; }
		      
       
        
        /// <summary>
        /// 安置关系路径
        /// </summary>  
				[DisplayName("安置关系路径")]
				public string  ParentPath { get; set; }
		      
       
        
        /// <summary>
        /// 安置关系层
        /// </summary>  
				[DisplayName("安置关系层")]
				public int  Depth { get; set; }
		      
       
        
        /// <summary>
        /// 安置节点数
        /// </summary>  
				[DisplayName("安置节点数")]
				public int  Child { get; set; }
		      
       
        
        /// <summary>
        /// 安置关系根ID
        /// </summary>  
				[DisplayName("安置关系根ID")]
				public int  RootID { get; set; }
		      
       
        
        /// <summary>
        /// 安置关系层排位
        /// </summary>  
				[DisplayName("安置关系层排位")]
				public int  DepthSort { get; set; }
		      
       
        
        /// <summary>
        /// 安置关系节点排位
        /// </summary>  
				[DisplayName("安置关系节点排位")]
				public int  ChildPlace { get; set; }
		      
       
        
        /// <summary>
        /// 登录密码
        /// </summary>  
				[DisplayName("登录密码")]
		        [MaxLength(50,ErrorMessage="登录密码最大长度为50")]
		public string  Password { get; set; }
		      
       
        
        /// <summary>
        /// 二级密码
        /// </summary>  
				[DisplayName("二级密码")]
		        [MaxLength(50,ErrorMessage="二级密码最大长度为50")]
		public string  Password2 { get; set; }
		      
       
        
        /// <summary>
        /// 三级密码
        /// </summary>  
				[DisplayName("三级密码")]
		        [MaxLength(50,ErrorMessage="三级密码最大长度为50")]
		public string  Password3 { get; set; }
		      
       
        
        /// <summary>
        /// 昵称
        /// </summary>  
				[DisplayName("昵称")]
		        [MaxLength(20,ErrorMessage="昵称最大长度为20")]
		public string  NickName { get; set; }
		      
       
        
        /// <summary>
        /// 真实姓名
        /// </summary>  
				[DisplayName("真实姓名")]
		        [MaxLength(50,ErrorMessage="真实姓名最大长度为50")]
		public string  RealName { get; set; }
		      
       
        
        /// <summary>
        /// 性别
        /// </summary>  
				[DisplayName("性别")]
		        [MaxLength(2,ErrorMessage="性别最大长度为2")]
		public string  Sex { get; set; }
		      
       
        
        /// <summary>
        /// 生日
        /// </summary>  
				[DisplayName("生日")]
				public DateTime?  Birthday { get; set; }
		      
       
        
        /// <summary>
        /// 邮箱
        /// </summary>  
				[DisplayName("邮箱")]
		        [MaxLength(30,ErrorMessage="邮箱最大长度为30")]
		public string  Email { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("QQ")]
		        [MaxLength(20,ErrorMessage="最大长度为20")]
		public string  QQ { get; set; }
		      
       
        
        /// <summary>
        /// 银行名称
        /// </summary>  
				[DisplayName("银行名称")]
		        [MaxLength(50,ErrorMessage="银行名称最大长度为50")]
		public string  BankName { get; set; }
		      
       
        
        /// <summary>
        /// 银行卡号
        /// </summary>  
				[DisplayName("银行卡号")]
		        [MaxLength(50,ErrorMessage="银行卡号最大长度为50")]
		public string  BankCard { get; set; }
		      
       
        
        /// <summary>
        /// 开户行
        /// </summary>  
				[DisplayName("开户行")]
		        [MaxLength(50,ErrorMessage="开户行最大长度为50")]
		public string  BankOfDeposit { get; set; }
		      
       
        
        /// <summary>
        /// 银行户名
        /// </summary>  
				[DisplayName("银行户名")]
		        [MaxLength(50,ErrorMessage="银行户名最大长度为50")]
		public string  BankUser { get; set; }
		      
       
        
        /// <summary>
        /// 国家
        /// </summary>  
				[DisplayName("国家")]
		        [MaxLength(50,ErrorMessage="国家最大长度为50")]
		public string  Country { get; set; }
		      
       
        
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
        /// 地址
        /// </summary>  
				[DisplayName("地址")]
		        [MaxLength(50,ErrorMessage="地址最大长度为50")]
		public string  Address { get; set; }
		      
       
        
        /// <summary>
        /// 手机
        /// </summary>  
				[DisplayName("手机")]
		        [MaxLength(50,ErrorMessage="手机最大长度为50")]
		public string  Mobile { get; set; }
		      
       
        
        /// <summary>
        /// 电话
        /// </summary>  
				[DisplayName("电话")]
		        [MaxLength(50,ErrorMessage="电话最大长度为50")]
		public string  Tel { get; set; }
		      
       
        
        /// <summary>
        /// 头像
        /// </summary>  
				[DisplayName("头像")]
		        [MaxLength(250,ErrorMessage="头像最大长度为250")]
		public string  HeadFace { get; set; }
		      
       
        
        /// <summary>
        /// 是否激活
        /// </summary>  
				[DisplayName("是否激活")]
				public bool  IsActivation { get; set; }
		      
       
        
        /// <summary>
        /// 是否锁定
        /// </summary>  
				[DisplayName("是否锁定")]
				public bool  IsLock { get; set; }
		      
       
        
        /// <summary>
        /// 是否代理商（商务中心）
        /// </summary>  
				[DisplayName("是否代理商（商务中心）")]
				public bool?  IsAgent { get; set; }
		      
       
        
        /// <summary>
        /// 商务中心名称
        /// </summary>  
				[DisplayName("商务中心名称")]
		        [MaxLength(50,ErrorMessage="商务中心名称最大长度为50")]
		public string  AgentName { get; set; }
		      
       
        
        /// <summary>
        /// 申请商务中心备注
        /// </summary>  
				[DisplayName("申请商务中心备注")]
		        [MaxLength(250,ErrorMessage="申请商务中心备注最大长度为250")]
		public string  ApplyAgentRemark { get; set; }
		      
       
        
        /// <summary>
        /// 申请商务中心时间
        /// </summary>  
				[DisplayName("申请商务中心时间")]
				public DateTime?  ApplyAgentTime { get; set; }
		      
       
        
        /// <summary>
        /// 身份证号码
        /// </summary>  
				[DisplayName("身份证号码")]
		        [MaxLength(50,ErrorMessage="身份证号码最大长度为50")]
		public string  IDCard { get; set; }
		      
       
        
        /// <summary>
        /// 密保问题
        /// </summary>  
				[DisplayName("密保问题")]
		        [MaxLength(50,ErrorMessage="密保问题最大长度为50")]
		public string  Question { get; set; }
		      
       
        
        /// <summary>
        /// 密保答案
        /// </summary>  
				[DisplayName("密保答案")]
		        [MaxLength(50,ErrorMessage="密保答案最大长度为50")]
		public string  Answer { get; set; }
		      
       
        
        /// <summary>
        /// 激活时间
        /// </summary>  
				[DisplayName("激活时间")]
				public DateTime?  ActivationTime { get; set; }
		      
       
        
        /// <summary>
        /// 冻结原因
        /// </summary>  
				[DisplayName("冻结原因")]
		        [MaxLength(250,ErrorMessage="冻结原因最大长度为250")]
		public string  LockReason { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LockTime")]
				public DateTime?  LockTime { get; set; }
		      
       
        
        /// <summary>
        /// 用户级别
        /// </summary>  
				[DisplayName("用户级别")]
				public int?  UserLevel { get; set; }
		      
       
        
        /// <summary>
        /// 注册时间
        /// </summary>  
				[DisplayName("注册时间")]
				public DateTime  CreateTime { get; set; }
		      
       
        
        /// <summary>
        /// 登录错误次数
        /// </summary>  
				[DisplayName("登录错误次数")]
				public int?  InputWrongPwdTimes { get; set; }
		      
       
        
        /// <summary>
        /// 最后登录时间
        /// </summary>  
				[DisplayName("最后登录时间")]
				public DateTime?  LastLoginTime { get; set; }
		      
       
        
        /// <summary>
        /// 最后修改时间
        /// </summary>  
				[DisplayName("最后修改时间")]
				public DateTime?  LastUpdateTime { get; set; }
		      
       
        
        /// <summary>
        /// 主钱包
        /// </summary>  
				[DisplayName("主钱包")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  Wallet2001 { get; set; }
		      
       
        
        /// <summary>
        /// 种子钱包
        /// </summary>  
				[DisplayName("种子钱包")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  Wallet2002 { get; set; }
		      
       
        
        /// <summary>
        /// 注册钱包
        /// </summary>  
				[DisplayName("注册钱包")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  Wallet2003 { get; set; }
		      
       
        
        /// <summary>
        /// 游戏钱包
        /// </summary>  
				[DisplayName("游戏钱包")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Wallet2004 { get; set; }
		      
       
        
        /// <summary>
        /// 资金钱包
        /// </summary>  
				[DisplayName("资金钱包")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Wallet2005 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金(奖金ID1101)
        /// </summary>  
				[DisplayName("累计奖金(奖金ID1101)")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Addup1101 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金(奖金ID1102)
        /// </summary>  
				[DisplayName("累计奖金(奖金ID1102)")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Addup1102 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金(奖金ID1103)
        /// </summary>  
				[DisplayName("累计奖金(奖金ID1103)")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Addup1103 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金(奖金ID1104)
        /// </summary>  
				[DisplayName("累计奖金(奖金ID1104)")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Addup1104 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金(奖金ID1105)
        /// </summary>  
				[DisplayName("累计奖金(奖金ID1105)")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Addup1105 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金(奖金ID1106)
        /// </summary>  
				[DisplayName("累计奖金(奖金ID1106)")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Addup1106 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金(奖金ID1107)
        /// </summary>  
				[DisplayName("累计奖金(奖金ID1107)")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Addup1107 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金(奖金ID1108)
        /// </summary>  
				[DisplayName("累计奖金(奖金ID1108)")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Addup1008 { get; set; }
		      
       
        
        /// <summary>
        /// 累计奖金(奖金ID1109)
        /// </summary>  
				[DisplayName("累计奖金(奖金ID1109)")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  Addup1009 { get; set; }
		      
       
        
        /// <summary>
        /// 个人业绩（累计提供订单的业绩）
        /// </summary>  
				[DisplayName("个人业绩（累计提供订单的业绩）")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  AddupSupplyAmount { get; set; }
		      
       
        
        /// <summary>
        /// 投资金额
        /// </summary>  
				[DisplayName("投资金额")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal  Investment { get; set; }
		      
       
        
        /// <summary>
        /// 已分红期数
        /// </summary>  
				[DisplayName("已分红期数")]
				public int?  BounsPeriod { get; set; }
		      
       
        
        /// <summary>
        /// 是否可参与分红
        /// </summary>  
				[DisplayName("是否可参与分红")]
				public bool?  IsShareBouns { get; set; }
		      
       
        
        /// <summary>
        /// 分红停止原因
        /// </summary>  
				[DisplayName("分红停止原因")]
		        [MaxLength(50,ErrorMessage="分红停止原因最大长度为50")]
		public string  ShareBounsStopReason { get; set; }
		      
       
        
        /// <summary>
        /// 分红停止时间
        /// </summary>  
				[DisplayName("分红停止时间")]
				public DateTime?  ShareBounsStopTime { get; set; }
		      
       
        
        /// <summary>
        /// 左对碰余量
        /// </summary>  
				[DisplayName("左对碰余量")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  LeftDpMargin { get; set; }
		      
       
        
        /// <summary>
        /// 右对碰余量
        /// </summary>  
				[DisplayName("右对碰余量")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  RightDpMargin { get; set; }
		      
       
        
        /// <summary>
        /// 左区业绩
        /// </summary>  
				[DisplayName("左区业绩")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  LeftAchievement { get; set; }
		      
       
        
        /// <summary>
        /// 右区业绩
        /// </summary>  
				[DisplayName("右区业绩")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  RightAchievement { get; set; }
		      
       
        
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
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
				public bool?  ReserveBoolean1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
				public bool?  ReserveBoolean2 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal1 { get; set; }
		      
       
        
        /// <summary>
        /// 预留字段
        /// </summary>  
				[DisplayName("预留字段")]
		        [Filters.DecimalPrecision(18,2)]
		public decimal?  ReserveDecamal2 { get; set; }
		      
       
        
        /// <summary>
        /// 代理商级别
        /// </summary>  
				[DisplayName("代理商级别")]
				public int?  AgentLevel { get; set; }
		      
       
        
        /// <summary>
        /// 2001钱包锁
        /// </summary>  
				[DisplayName("2001钱包锁")]
				public bool?  Wallet2001Lock { get; set; }
		      
       
        
        /// <summary>
        /// 2002钱包锁
        /// </summary>  
				[DisplayName("2002钱包锁")]
				public bool?  Wallet2002Lock { get; set; }
		      
       
        
        /// <summary>
        /// 2003钱包锁
        /// </summary>  
				[DisplayName("2003钱包锁")]
				public bool?  Wallet2003Lock { get; set; }
		      
       
        
        /// <summary>
        /// 2004钱包锁
        /// </summary>  
				[DisplayName("2004钱包锁")]
				public bool?  Wallet2004Lock { get; set; }
		      
       
        
        /// <summary>
        /// 2005钱包锁
        /// </summary>  
				[DisplayName("2005钱包锁")]
				public bool?  Wallet2005Lock { get; set; }
		      
       
        
        /// <summary>
        /// 支付宝
        /// </summary>  
				[DisplayName("支付宝")]
		        [MaxLength(50,ErrorMessage="支付宝最大长度为50")]
		public string  AliPay { get; set; }
		      
       
        
        /// <summary>
        /// 微信号
        /// </summary>  
				[DisplayName("微信号")]
		        [MaxLength(50,ErrorMessage="微信号最大长度为50")]
		public string  WeiXin { get; set; }
		      
       
        
        /// <summary>
        /// 区块链钱包地址
        /// </summary>  
				[DisplayName("区块链钱包地址")]
		        [MaxLength(50,ErrorMessage="区块链钱包地址最大长度为50")]
		public string  Block { get; set; }
		      
       
        
        /// <summary>
        /// 创建人
        /// </summary>  
				[DisplayName("创建人")]
		        [MaxLength(50,ErrorMessage="创建人最大长度为50")]
		public string  CreateBy { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("Token")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  Token { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("TokenExpirationTime")]
				public DateTime?  TokenExpirationTime { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LastLoginIP")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  LastLoginIP { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("LastLoginAddress")]
		        [MaxLength(50,ErrorMessage="最大长度为50")]
		public string  LastLoginAddress { get; set; }
		      
       
        
        /// <summary>
        /// 
        /// </summary>  
				[DisplayName("IsProTeam")]
				public bool?  IsProTeam { get; set; }
		      
       
         

        /// <summary>
        /// 构造函数
        /// </summary>
		
        public User()
        {
           // ID = Guid.NewGuid();
        }
      
    }
 
    
}
namespace JN.Data.Service
{
    /// <summary>
    /// 会员表业务接口
    /// </summary>
    public interface IUserService :IServiceBase<User> {
		 /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        DbEntityValidationResult GetValidationResult(User entity);
	}
    /// <summary>
    /// 会员表业务类
    /// </summary>
    public class UserService :  ServiceBase<User>,IUserService
    {


        public UserService(ISysDbFactory dbfactory) : base(dbfactory) {}
         /// <summary>
        /// 获取实体对象验证结果
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public DbEntityValidationResult GetValidationResult(User entity)
        {
            return DataContext.Entry(entity).GetValidationResult();
        }
    }

}   
