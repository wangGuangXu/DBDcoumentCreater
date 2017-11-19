using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Chen.Common;
using Chen.DB;
using DBDcoumentCreater.Lib;
using Chen.Ext;
using System.Threading;
using System.Diagnostics;
using Core.Ext;
namespace DBDcoumentCreater
{
    /// <summary>
    /// 登陆窗体
    /// </summary>
    public partial class Form1 : Form
    {
        private string defaultHtml = "数据库表目录.html";
        private IniFileHelp ini = new IniFileHelp(".//set.ini");
        private IDAL dal;
        public static bool NeedData { get; set; }

        public Form1()
        {
            InitializeComponent();
            txtTitle.Text = ini.GetString("Set", "title", "数据库帮助文档");
            txtLogPath.Text = ini.GetString("Set", "log", Path.Combine(Environment.CurrentDirectory, "更新日志.log"));
            cbbDbtype.SelectedIndex = ini.GetInt32("Set", "index", 0);
            cbbRows.SelectedText = ini.GetString("Set", "row", "50");
            CheckForIllegalCrossThreadCalls = false;
            new TaskBarUtil(this, notifyIcon1);
        }

        #region 连接数据库
        private void btnConn_Click(object sender, EventArgs e)
        {
            if (txtConn.Text.Length == 0)
            {
                lblMessage.Text = "连接字符串不能为空";
                return;
            }
            try
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
                {
                    try
                    {
                        dal = DALFacotry.Create(cbbDbtype.Text, txtConn.Text);
                        if (dal == null)
                        {
                            lblMessage.Text = "数据库连接失败";
                            return;
                        }
                        lblMessage.Text = "成功连接数据库";
                        //成功连接之后将配置保存
                        ini.WriteValue("Set", "index", cbbDbtype.SelectedIndex);
                        ini.WriteValue("Set", "index_" + cbbDbtype.SelectedIndex, txtConn.Text);
                        //加载表信息
                        ckbTables.Items.Clear();
                        ckbData.Items.Clear();
                        var dt = dal.GetTables();
                        if (dt.Rows.Count == 0)
                        {
                            lblMessage.Text = "查询表信息异常，请选择正确的数据库!";
                            return;
                        }
                        foreach (DataRow dr in dt.Rows)
                        {
                            ckbTables.Items.Add(dr["表名"].ToString());
                            ckbData.Items.Add(dr["表名"].ToString());
                        }
                        groupBox1.Enabled = true;
                        lblMessage.Text = "数据库已连接";
                    }
                    catch (Exception)
                    {
                        lblMessage.Text = "数据库操作失败";
                    }
                    finally
                    {
                        this.CloseLoading();
                    }
                }));
                this.Loading();
                lblMessage.Text = "正在连接数据库";
            }
            catch (Exception)
            {
                lblMessage.Text = "数据库异常 请确认";
            }
        }
        #endregion

        #region 导出
        private void btnExport_Click(object sender, EventArgs e)
        {
            //输入验证
            if (ckbData.CheckedItems.Count == 0 && ckbTables.CheckedItems.Count == 0)
            {
                lblMessage.Text = "请至少选择一张表";
                return;
            }
            if (txtTitle.Text.Length == 0)
            {
                lblMessage.Text = "请输入CHM文件标题";
                return;
            }
            //保存配置
            ini.WriteValue("Set", "title", txtTitle.Text);
            ini.WriteValue("Set", "log", txtLogPath.Text);
            ini.WriteValue("Set", "row", cbbRows.SelectedText);
            NeedData = ckbData.CheckedItems.Count > 0;
            this.Loading();
            ThreadPool.QueueUserWorkItem(new WaitCallback(Export));
        }
        #endregion

        #region 导出
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="o"></param>
        public void Export(object o)
        {
            Directory.CreateDirectory(".//tmp");
            //将日志文件复制到tmp文件夹下
            if (txtLogPath.Text.Length > 0 && File.Exists(txtLogPath.Text))
            {
                File.Copy(txtLogPath.Text, ".//tmp/更新日志.txt", true);
            }

            //定义目录DataTable 结构
            var dtMenus = new DataTable("<b>数据库表目录</b>");
            dtMenus.Columns.Add("序号", typeof(int));
            dtMenus.Columns.Add("表名", typeof(string));
            dtMenus.Columns.Add("表说明", typeof(string));

            //将选中项的总数 设置为进度条最大值 +1项是表目录文件
            tpbExport.Value = 0;
            tpbExport.Maximum = ckbData.CheckedItems.Count + ckbTables.CheckedItems.Count + 1;

            //获取需要导出的表结构 选中项
            List<string> lst = new List<string>();
            foreach (var item in ckbTables.CheckedItems)
            {
                lst.Add(item.ToString());
            }

            #region 导出表结构
            if (lst.Count > 0)
            {
                lblMessage.Text = "准备表结构文件...";
                //得到选中的表结构的字段信息
                var lstDt = dal.GetTableStruct(lst);
                var pathTables = "./tmp/表结构";
                Directory.CreateDirectory(pathTables);
                var tableIndex = 1;
                foreach (var dt in lstDt)
                {
                    //得到表描述
                    var drs = dal.GetTables().Select("表名='" + dt.TableName + "'");
                    var desp = string.Empty;
                    if (drs.Length > 0) desp = drs[0]["表说明"].ToString().RemoveValidFileChar();
                    //创建表字段信息的html
                    //得到文件存储路径
                    var tabPath = Path.Combine(pathTables, dt.TableName + ".html");
                    //指定table标题 将表的名称设置为超链接并返回
                    var title = "<a href='../" + defaultHtml + "'>" + dt.TableName + "</a>";
                    if (desp.Trim().Length > 0) title += "（" + drs[0]["表说明"].ToString() + "）";
                    DbCommon.CreateHtml(dt, true, tabPath, title);
                    //构建表目录
                    DataRow dr = dtMenus.NewRow();
                    dr["序号"] = tableIndex++;
                    dr["表说明"] = desp;
                    dr["表名"] = "<a href=\"表结构\\{0}.html\">{0}</a>".FormatString(dt.TableName);
                    dtMenus.Rows.Add(dr);
                    //改变进度
                    tpbExport.Value++;
                }
                //导出表目录
                DbCommon.CreateHtml(dtMenus, false, "./tmp/" + defaultHtml, "<b>数据库表目录</b>");
                tpbExport.Value++;
            }
            #endregion

            #region 导出表数据
            //传递需要导出数据的table选中项  得到数据内容
            lst.Clear();
            foreach (var item in ckbData.CheckedItems)
            {
                lst.Add(item.ToString());
            }
            if (lst.Count > 0)
            {
                lblMessage.Text = "正在生成表数据数据...";
                var lstDt = dal.GetTableData(lst);
                //创建常用数据的html
                var pathTables = "./tmp/常用数据";
                Directory.CreateDirectory(pathTables);
                foreach (var dt in lstDt)
                {
                    var tabPath = Path.Combine(pathTables, dt.TableName + ".html");
                    //指定table标题 将表的名称设置为超链接并返回
                    var title = "<a href='../" + defaultHtml + "'>" + dt.TableName + "</a>";
                    //得到表描述
                    var drs = dal.GetTables().Select("表名='" + dt.TableName + "'");
                    var desp = string.Empty;
                    if (drs.Length > 0) desp = drs[0]["表说明"].ToString();
                    if (desp.Trim().Length > 0) title += "（" + drs[0]["表说明"].ToString() + "）";
                    DbCommon.CreateHtml(dt, true, tabPath, title);
                    tpbExport.Value++;
                }
            }
            #endregion

            try
            {
                lblMessage.Text = "正在编译CHM文件...";
                //编译CHM文档
                ChmHelp c3 = new ChmHelp
                {
                    DefaultPage = defaultHtml,
                    Title = txtTitle.Text
                };
                c3.ChmFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), c3.Title + ".chm");
                c3.RootPath = @"./tmp";
                c3.Compile();
                lblMessage.Text = DateTime.Now.ToString() + " 导出成功";
                Process.Start(c3.ChmFileName);
                Directory.Delete("./tmp", true);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "导出发生异常";
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.CloseLoading();
            }
        } 
        #endregion

        private void txtConn_TextChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = false;
        }

        #region 全选控制
        private void btnChoseAll2_Click(object sender, EventArgs e)
        {
            ckbData.SelectedAll();
        }

        private void btnChoseReverse2_Click(object sender, EventArgs e)
        {
            ckbData.SelectedReverse();
        }

        private void btnChoseAll_Click(object sender, EventArgs e)
        {
            ckbTables.SelectedAll();
        }

        private void btnChoseReverse_Click(object sender, EventArgs e)
        {
            ckbTables.SelectedReverse();
        }

        #endregion

        private void cbbDbtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtConn.Text = ini.GetString("Set", "index_" + cbbDbtype.SelectedIndex, "");
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            frmConn fc = new frmConn(cbbDbtype.SelectedIndex);
            fc.ShowDialog();
            if (fc.chosed)
            {
                cbbDbtype.SelectedIndex = fc.index;
                txtConn.Text = fc.conn;
            }
        }

        #region 更新日志
        OpenFileDialog dialog = new OpenFileDialog()
        {
            InitialDirectory = Environment.CurrentDirectory,
            Filter = "更新日志|*txt;*log",
            RestoreDirectory = true
        };
        private void txtLogPath_MouseClick(object sender, MouseEventArgs e)
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                dialog.InitialDirectory = Path.GetDirectoryName(dialog.FileName);
                txtLogPath.Text = dialog.FileName;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLogPath.Text = string.Empty;
        }
        #endregion


    }
}
