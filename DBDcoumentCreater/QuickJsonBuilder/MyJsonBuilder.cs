using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
namespace Core.JsonSerialize
{
    public class MyJsonBuilder : QuickJsonBuilder
    {
        /// <summary> 表风格
        /// <para> 0    { columns:[colname,colname2],rows:[[x,x],[y,y]] } </para>
        /// <para> 1    [{ colname:x,colname2:x },{ colname:y,colname2:y }] </para>
        /// </summary>
        public int TableStyle { get; set; }
        /// <summary> 时间风格
        /// <para> 默认 yyyy-MM-dd HH:mm:ss </para>
        /// <para> 1    yyyy-MM-dd </para>
        /// <para> 2    HH:mm:ss </para>
        /// </summary>
        public int DateTimeStyle { get; set; }

        protected override void AppendDateTime(DateTime value)
        {
            switch (DateTimeStyle)
            {
                case 1:
                    UnsafeAppend('"');
                    UnsafeAppend(value.ToString("yyyy-MM-dd"));
                    UnsafeAppend('"');
                    break;
                case 2:
                    UnsafeAppend('"');
                    UnsafeAppend(value.ToString("HH:mm:ss"));
                    UnsafeAppend('"');
                    break;
                default:
                    base.AppendDateTime(value);
                    break;
            }
        }

        protected override void AppendGuid(Guid value)
        {
            UnsafeAppend('"');
            UnsafeAppend(value.ToString("N"));
            UnsafeAppend('"');
        }

        protected override void AppendBoolean(bool value)
        {
            base.AppendNumber(value.GetHashCode());
        }

        protected override void AppendDataTable(System.Data.DataTable table)
        {
            if (TableStyle == 0)
            {
                UnsafeAppend('[');
                var fix = "";

                foreach (DataRow row in table.Rows)
                {
                    UnsafeAppend(fix);
                    var fix2 = "";
                    UnsafeAppend('{');
                    foreach (DataColumn col in table.Columns)
                    {
                        UnsafeAppend(fix2);
                        AppendKey(col.ColumnName, false);
                        AppendObject(row[col]);
                        fix2 = ",";
                    }
                    UnsafeAppend('}');
                    fix = ",";
                }
                UnsafeAppend(']');
            }
            else
            {
                base.AppendDataTable(table);
            }
        }

    }
}
