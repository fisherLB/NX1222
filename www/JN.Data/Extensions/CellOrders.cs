using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JN.Data.Extensions
{
    /// <summary>
    /// CellOrders
    /// </summary>
    public class CellOrders
    {
        public string OrderNo { get; set; }
        public string OrderTable { get; set; }
        public DateTime CreateTime { get; set; }

        //单号标识
        public string OrderSign { get; set; }

    }
}
