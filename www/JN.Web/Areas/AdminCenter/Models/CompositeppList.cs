using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webdiyer.WebControls.Mvc;
namespace JN.Web.Areas.AdminCenter.Models
{
    /// <summary>
    /// 多组异步分页
    /// </summary>
    public class CompositeppList
    {
        public PagedList<JN.Data.SupplyHelp> SupplyHelp { get; set; }
        public PagedList<JN.Data.AcceptHelp> AcceptHelp { get; set; }
        
    }
}
