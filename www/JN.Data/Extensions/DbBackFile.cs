using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JN.Data.Extensions
{
    public class DbBackFile
    {
        public string BackFileName { get; set; }
        public string BackFileFullName { get; set; }
        public long BackFileSize { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
