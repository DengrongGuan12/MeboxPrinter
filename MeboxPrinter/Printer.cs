using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace MeboxPrinter
{
    class Printer
    {
        /// <summary>  
        /// 构造函数  
        /// </summary>  
        /// <param name="name">打印机名称</param>  
        public Printer(string name)
        {
            this.printer_name = name;
        }

        // 设备名：EPSON R330 Series  
        private string _printer_name;
        /// <summary>  
        /// 打印机名称  
        /// </summary>  
        public string printer_name
        {
            get
            {
                return _printer_name;
            }
            set
            {
                _printer_name = value;
            }
        }

        /// <summary>  
        /// 获取打印机状态  
        /// </summary>  
        /// <returns></returns>  
        public int getStatus()
        {
            string path = @"win32_printer.DeviceId='" + this.printer_name + "'";
            ManagementObject printer = new ManagementObject(path);
            printer.Get();
            return Convert.ToInt32(printer.Properties["PrinterStatus"].Value);
        }
    }
}
