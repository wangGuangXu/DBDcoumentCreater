using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Chen.Common
{
    /// <summary>
    /// 任务栏简单封装
    /// </summary>
    /// <remarks>
    /// 检查程序是否再次运行:在main方法里调用：TaskBarUtil.CheckCreated();
    /// 主窗体在load事件或者构造方法初始化组件后调用：new TaskBarUtil(this, notifyIcon1);
    /// 其中notifyIcon必须指定图标
    /// </remarks>
    public class TaskBarUtil
    {
        private Form mainForm;
        private NotifyIcon notifyIcon1;
        private bool canExit;//是否允许直接关闭窗体
        public static EventWaitHandle ProgramStarted;

        public TaskBarUtil(Form main, NotifyIcon notifyIcon1, bool canExit = true)
        {
            this.mainForm = main;
            this.notifyIcon1 = notifyIcon1;
            this.canExit = canExit;
            this.canExit = false;
            Load();
        }

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #region 右下角图标控制
        private void Load()
        {
            if (ProgramStarted != null)
                //注册进程OnProgramStarted
                ThreadPool.RegisterWaitForSingleObject(ProgramStarted,
                    (obj, timeout) =>
                    {
                        mainForm.Invoke(new ThreadStart(() =>
                         {
                             //冗余代码说明:当窗体未被最小化即被其他窗体遮挡时，win7下偶尔无法将其显示出来，但窗体先隐藏到任务栏中，在显示就没有问题
                             if (mainForm.WindowState == FormWindowState.Minimized)
                             {
                                 mainForm.Visible = true; //显示窗体
                                 mainForm.WindowState = FormWindowState.Normal;  //恢复窗体默认大小
                             }
                             mainForm.Show();
                             //前置该窗体
                             SetForegroundWindow(mainForm.Handle);
                         }));
                    },
                    null, -1, false);

            #region 窗体事件
            mainForm.SizeChanged += new EventHandler((sender, e) =>
              {
                  if (mainForm.WindowState == FormWindowState.Minimized)
                  {
                      HideForm();
                  }
              });
            mainForm.FormClosing += new FormClosingEventHandler((sender, e) =>
            {
                //注意判断关闭事件Reason来源于窗体按钮，否则用菜单退出时无法退出!           
                if (e.CloseReason == CloseReason.UserClosing && !canExit)
                {
                    mainForm.WindowState = FormWindowState.Minimized;    //使关闭时窗口向右下角缩小的效果
                    notifyIcon1.Visible = true;
                    e.Cancel = true;
                }
            });
            #endregion

            #region 任务栏图标上下文事件
            ContextMenuStrip contextMenuStrip1 = new ContextMenuStrip();
            //设置任务栏图标上下文事件
            var tsmShow = new ToolStripMenuItem();
            tsmShow.Name = "tsmShow";
            tsmShow.Text = "显示";
            tsmShow.Click += new System.EventHandler((sender, e) =>
            {
                if (mainForm.Visible) return;
                ShowForm();
            });
            var tsmExit = new ToolStripMenuItem();
            tsmExit.Text = "退出";
            tsmExit.Name = "tsmShow";
            tsmExit.Click += new System.EventHandler((sender, e) =>
            {
                Application.Exit();
            });
            contextMenuStrip1.Items.Add(tsmShow);
            contextMenuStrip1.Items.Add(tsmExit);
            #endregion

            #region 任务栏图标事件
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.MouseClick += new MouseEventHandler((sender, e) =>
            {
                if (e.Button != MouseButtons.Right)
                {
                    ShowForm();
                }
            });
            #endregion
        }

        private void ShowForm()
        {
            if (mainForm.WindowState == FormWindowState.Minimized)
                mainForm.WindowState = FormWindowState.Normal;  //恢复窗体默认大小
            mainForm.Visible = true; //显示窗体
            mainForm.Show();
            //前置该窗体
            SetForegroundWindow(mainForm.Handle);
        }

        private void HideForm()
        {
            mainForm.Visible = false; //隐藏窗体
            mainForm.Hide();
        }

        #endregion

        #region 检查是否启动过，如果启动则通知前一个进程，并退出当前进程
        /// <summary>
        /// 检查是否启动过，如果启动则通知前一个进程，并退出当前进程
        /// </summary>
        public static void CheckCreated()
        {
            // 尝试创建一个命名事件
            bool createNew;
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, Application.ProductName, out createNew);

            // 如果该命名事件已经存在(存在有前一个运行实例)，则发事件通知并退出
            if (!createNew)
            {
                TaskBarUtil.ProgramStarted.Set();
                Environment.Exit(0);
            }
        }
        #endregion
    }
}
