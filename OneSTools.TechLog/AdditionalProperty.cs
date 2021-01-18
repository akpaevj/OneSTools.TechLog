using System;
using System.Collections.Generic;
using System.Text;

namespace OneSTools.TechLog
{
    public enum AdditionalProperty
    {
        None = 0x1,
        SqlHash = 0x2,
        CleanSql = 0x3,
        FirstContextLine = 0x4,
        LastContextLine = 0x5,
        All = 0xF
    }
}
