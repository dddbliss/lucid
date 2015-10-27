using NPLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lucid
{
    public static class ExceptionExtensions
    {
        public static void ReportException(this Exception @this)
        {
            if (!Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\Lucid"))
                Directory.CreateDirectory(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\Lucid");

            string fileName = string.Format(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\Lucid\\Exception_{0}.txt", DateTime.Now.ToString("yyyy_dd_MM_hh_mm_ss"));
            using (var fo = new StreamWriter(fileName))
            {
                fo.WriteLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
                fo.WriteLine(@this.Message);
                fo.WriteLine("-[Stack Trace]-----------------------------------------------------------------");
                fo.WriteLine(@this.StackTrace);
                fo.WriteLine("-[Body]------------------------------------------------------------------------");
                fo.WriteLine(ClientManager.Instance.GetLastRequestBody());
            };

            MessageBox.Show(string.Format("Lucid has reported a fatal error.\r\nThe file '{0}' has been generated for reporting.\r\n\r\nWe have stopped shopping for now.", fileName), "Fatal Error");
            //ClientManager.Instance.SendMessage("Stopped shopping due to fatal error.", NPLib.Models.LogLevel.Error);
        }
    }
}
