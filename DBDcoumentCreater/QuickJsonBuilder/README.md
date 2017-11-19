修改部分
JsonParser.cs 控制Key转换
        Func<string, string> keyConvert;
        public JsonParser(Func<string, string> keyConvert = null)
        {
            this.keyConvert = keyConvert;
        }

        private void FillDictionary(IDictionary dict, Type keyType, Type elementType, UnsafeJsonReader reader)
        {
            if (reader.Current == '}') return;
            if (keyType == typeof(string) || keyType == typeof(object))
            {
                while (true)
                {
                    string key = ReadKey(reader);      //获取Key
                    if (keyConvert != null) key = keyConvert(key);
                    object val = ReadValue(reader, elementType);//得到值
                    dict[key] = val;
                    if (reader.SkipChar(',') == false)//跳过,号
                    {
                        return;                     //失败,终止方法
                    }
                }
            }
            while (true)
            {
                string skey = ReadKey(reader);      //获取Key
                if (keyConvert != null) skey = keyConvert(skey);

                object key = ChangeType(skey, keyType);
                object val = ReadValue(reader, elementType);//得到值
                dict[key] = val;
                if (reader.SkipChar(',') == false)//跳过,号
                {
                    return;                     //失败,终止方法
                }
            }
        }
JsonBuilder.cs
        protected virtual void AppendDataTable(DataTable table)
        {
            //Buffer.Append("{\"columns\":");
            //AppendArray(table.Columns, o => ((DataColumn)o).ColumnName);
            //Buffer.Append(",\"rows\":");
            //AppendArray(table.Rows, o => ((DataRow)o).ItemArray);
            //Buffer.Append('}');
            //更改默认实现
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
