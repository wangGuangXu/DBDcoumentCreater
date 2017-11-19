using System;
using System.Collections.Generic;
using System.Text;

namespace DBDcoumentCreater.Lib
{
    /// <summary>
    /// DALFacotry
    /// </summary>
    public class DALFacotry
    {
        public static IDAL Create(string dbType, string Conn)
        {
            try
            {
                switch (dbType)
                {
                    case "SQL2005及以上": return new SqlDAL(Conn);
                    case "Oracle": return new OracleDAL(Conn);
                    default: return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}