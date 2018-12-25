using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webdiyer.WebControls.Mvc;
namespace JN.Web.Areas.UserCenter.Models
{
    /// <summary>
    /// 多组异步分页
    /// </summary>
    public class CompositeWalletLog
    {
        public PagedList<JN.Data.WalletLog> Wallet2001 { get; set; }
        public PagedList<JN.Data.WalletLog> Wallet2002 { get; set; }
        public PagedList<JN.Data.WalletLog> Wallet2003 { get; set; }
        public PagedList<JN.Data.WalletLog> Wallet2004 { get; set; }
        public PagedList<JN.Data.BonusDetail> BonusDetailList { get; set; }
    }
}
