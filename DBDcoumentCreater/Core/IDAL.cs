using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DBDcoumentCreater.Lib
{
    /// <summary>
    /// IDAL
    /// </summary>
    public interface IDAL
    {
        DataTable GetTables();
        List<DataTable> GetTableStruct(List<string> tables);
        List<DataTable> GetTableData(List<string> tables);
    }
}