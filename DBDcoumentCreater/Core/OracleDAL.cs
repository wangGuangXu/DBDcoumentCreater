using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using System.Text;
using Chen.DB;
using Chen.Ext;
using Core.DB;
using System.Data;

namespace DBDcoumentCreater.Lib
{
    /// <summary>
    /// OracleDAL
    /// </summary>
    public class OracleDAL : IDAL
    {
        private DbHelp help;
        private DataTable dtStruct;//表以及字段详情表
        private DataTable dt;//表以及对应的描述信息

        public OracleDAL(string conn)
        {
            help = new DbHelp(DbProviderFactories.GetFactory("System.Data.OracleClient"), conn);
            var strSql = @"select ROWNUM 序号 ,ut.table_name 表名,utc.comments 表说明 
from user_tables ut 
left join user_tab_comments utc on ut.table_name = utc.table_name 
where utc.table_name  not like '%$%'
order by ut.table_name
";
            help.BeginTransation();
            //查询所有表名
            dt = help.ExecuteSql(strSql);
            //Oracle 主键关系表，使用临时表来管理
            var keyTempTable = @"CREATE GLOBAL TEMPORARY TABLE table_key_constraints  as         
          SELECT  col.table_name, col.column_name  from   user_constraints con,user_cons_columns col 
          WHERE  con.constraint_name=col.constraint_name and con.constraint_type='P' AND 1<>1";
            var dropTempTable = @"DROP TABLE table_key_constraints";
            var exportTmpTable = @"INSERT INTO table_key_constraints  SELECT col.table_name,  col.column_name   from   user_constraints con,user_cons_columns col 
          WHERE  con.constraint_name=col.constraint_name and con.constraint_type='P' ";

            //删除临时表
            try
            {
                help.ExecuteNonQuery(dropTempTable);
            }
            catch (Exception)
            {
                help.BeginTransation();
            }
            //创建临时表
            help.ExecuteNonQuery(keyTempTable);
            //导入临时表数据
            help.ExecuteNonQuery(exportTmpTable);

            strSql = @" select  row_number()over( partition by utc.table_name order by utc.COLUMN_ID, ROWNUM ) as 序号,
                utc.table_name as 表名,
                 utc.column_name as 列名, 
                 utc.data_type as 数据类型, 
                 utc.data_length as 长度, 
                 utc.data_precision as 精度,
                 utc.data_Scale 小数位数, 
                 case  ( select  count(*)   from  table_key_constraints where column_name = utc.COLUMN_NAME and table_name=upper(utc.table_name)  ) 
                 when 0    then '' else '√' 
                 end as 主键, 
                 case when utc.nullable = 'Y' then '√' else '' end as 允许空, 
                 utc.data_default as 默认值, 
                 ucc.comments as 列说明 
                 from 
                 user_tab_columns utc,user_col_comments ucc 
                 where  utc.table_name = ucc.table_name and utc.column_name = ucc.column_name  
                 and utc.table_name  not like '%$%' and  utc.table_name  not like '%plsql%' 
                 order by   utc.table_name,序号";
            dtStruct = help.ExecuteSql(strSql);
            //删除临时表
            help.ExecuteNonQuery(dropTempTable);
            help.Commit();
        }

        public DataTable GetTables()
        {
            return dt;
        }

        public List<DataTable> GetTableStruct(List<string> tables)
        {
            List<DataTable> lst = new List<DataTable>();
            foreach (var table in tables)
            {
                var dtData = dtStruct.GetNewDataTable("表名='" + table + "'");
                dtData.TableName = table;
                dtData.Columns.Remove("表名");
                lst.Add(dtData);
            }
            return lst;
        }

        public List<DataTable> GetTableData(List<string> tables)
        {
            List<DataTable> lst = new List<DataTable>();
            help.BeginTransation();
            foreach (var table in tables)
            {
                //避免取出来的数据过大
                var dt = help.ExecuteSql("select  * from \"" + table + "\" where ROWNUM < 50 ");
                dt.TableName = table;
                lst.Add(dt);
            }
            help.Commit();
            return lst;
        }
    }
}