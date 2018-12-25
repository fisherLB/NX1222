using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JN.Data.Extensions
{
    /// <summary>
    /// CellOrders
    /// </summary>
    public class USDOrders
    {
        public DateTime CreateTime { get; set; }
        public string OrderType { get; set; }
        public decimal Money { get; set; }
        public int UID { get; set; }
        public string UserName { get; set; }
        public int ID { get; set; }
        public int Status { get; set; }
    }
}
