﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Chen.Common;

namespace DBDcoumentCreater
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            TaskBarUtil.CheckCreated();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
