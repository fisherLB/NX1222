using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JN.Data.Extensions
{
    /// <summary>
    /// 节点Model
    /// </summary>
    /// <summary>
    /// 节点Model
    /// </summary>
    public class TreeNode
    {
        public int id { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public bool children { get; set; }
        public string type { get; set; }
        public string state { get; set; }
    }
}
