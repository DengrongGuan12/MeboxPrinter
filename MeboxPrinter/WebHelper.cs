using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace MeboxPrinter
{
    class WebHelper
    {
        /// <summary>
        /// 读取配置
        /// url、秘钥等
        /// </summary>
        public static void readSettings()
        {
            
            string[] keys = ConfigurationSettings.AppSettings.AllKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                //通过Key来索引Value
                string value = ConfigurationSettings.AppSettings[key];
                Console.WriteLine(i.ToString() + ". " + key + " = " + value);
                if (key.Equals("machineCode"))
                {
                    URL_TAIL = "/machineCode/" + value;
                    MACHINE_CODE = value;
                }else if (key.Equals("baseUrl"))
                {
                    BASE_URL = value;
                }else if (key.Equals("downloadPath"))
                {
                    DOWNLOAD_ROAD = value;
                }else if (key.Equals("getQrCodeInterval"))
                {
                    MainForm.getQrCodeInterval = int.Parse(value);
                }else if (key.Equals("getPrintOrderListInterval"))
                {
                    MainForm.getPrintOrderListInterval = int.Parse(value);
                }else if (key.Equals("backToDirectorPanelTimer"))
                {
                    MainForm.backToDirectorPanelInterval = int.Parse(value);
                }else if (key.Equals("paperNum"))
                {
                    MainForm.PAPER_NUM = int.Parse(value);
                    MainForm.paperNum = int.Parse(value);
                }else if (key.Equals("paperOffSet"))
                {
                    MainForm.offset = int.Parse(value);
                }
            }
        }
        private static string BASE_URL="";
        private static string URL_TAIL = "";
        private static string CHECK_PRINT_URL_HEAD = "Print/getPrintListByCode/code/";
        private static string FINISH_PRINT_URL_HEAD = "Print/finishPrintEx/code/";
        private static string MACHINE_CODE = "";
        /// <summary>
        /// 通过打印码获取打印列表
        /// </summary>
        /// <returns>null表示打印码错误</returns>
        public static PrintListResult checkPrintCode(string codeStr)
        {
            //TODO  FOR TEST
            //codeStr = "744304624013602";
            //取得数据
            string url = BASE_URL + CHECK_PRINT_URL_HEAD + codeStr + URL_TAIL;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception) {
                return null;
            }
            Stream myResponseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(myResponseStream);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponseStream.Close();
            response.Close();

            //解析结果
            JObject responseJson = JObject.Parse(result);
            if(responseJson["result"].ToString() == "0")
            {
                return null;
            }else
            {
                PrintListResult printListResult = new PrintListResult();
                printListResult.CodeStr = codeStr;
                JObject data = JObject.Parse(responseJson["data"].ToString());
                JObject info = JObject.Parse(data["info"].ToString());
                printListResult.TotalMili = int.Parse(info["totalMili"].ToString());
                printListResult.TotalNum = int.Parse(info["totalNum"].ToString());
                JArray ja = JArray.Parse(data["list"].ToString());
                List<PrintObject> pos = new List<PrintObject>();
                foreach (JToken token in ja)
                {
                    PrintObject po = new PrintObject();

                    JObject jo = JObject.Parse(token.ToString());
                    po.set(jo["objectId"].ToString(), "objectId");
                    po.set(jo["type"].ToString(), "type");
                    po.set(jo["name"].ToString(), "name");
                    po.set(jo["ext"].ToString(), "ext");
                    JObject paramsJo = JObject.Parse(jo["param"].ToString());
                    if (!po.set(paramsJo["fromPage"].ToString(), "fromPage") ||
                        !po.set(paramsJo["toPage"].ToString(), "toPage") ||
                        !po.set(paramsJo["count"].ToString(), "count") ||
                        !po.set(paramsJo["single"].ToString(), "single") ||
                        !po.set(paramsJo["num"].ToString(), "num") ||
                        !po.set(paramsJo["paperNum"].ToString(), "paperNum") ||
                        !po.set(paramsJo["scale"].ToString(), "scale"))
                        return null;
                    pos.Add(po);
                }
                printListResult.PrintObjects = pos;
                return printListResult;
            }
        }


        private static string DOWNLOAD_URL_HEAD = "Print/downloadPdfFile/id/";
        private static string DOWNLOAD_ROAD = "C:\\Program Files\\mebox\\cache\\";
        /// <summary>
        /// 通过文件id下载文件
        /// </summary>
        /// <returns>false 表明下载失败</returns>
        public static bool downloadFile(PrintObject po)
        {
            string pdfPath = System.Environment.CurrentDirectory + DOWNLOAD_ROAD;
            //确保文件夹路径存在
            if (!Directory.Exists(pdfPath))
            {
                Directory.CreateDirectory(pdfPath);
            }
            //生成文件名
            string fileName = po.Id + ".pdf";
            po.Location = pdfPath + fileName;
            //写入文件
            if (File.Exists(po.Location))
            {
                return true;
            }
            //发送get
            string url = BASE_URL + DOWNLOAD_URL_HEAD + po.Id + URL_TAIL;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }
            Stream stream = response.GetResponseStream();
            FileStream fs = File.Open(po.Location, FileMode.Create);
            stream.CopyTo(fs);
            fs.Flush();
            fs.Close();
            stream.Close();
            response.Close();
            return true;
        }
        /// <summary>
        /// 向服务器汇报打印完成
        /// </summary>
        /// <returns>false 表明汇报失败</returns>
        public static bool reportPrintComplete(string printCode)
        {
            string url = BASE_URL + FINISH_PRINT_URL_HEAD + printCode + URL_TAIL;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }
            Stream myResponseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(myResponseStream);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponseStream.Close();
            response.Close();

            //解析结果
            JObject responseJson = JObject.Parse(result);
            if (int.Parse(responseJson["result"].ToString()) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        private static string GET_QR_CODE_URL_HEAD = "Print/qrUrl/code/";
        /// <summary>
        /// 获取二维码
        /// </summary>
        /// <param name="printCode"></param>
        /// <returns></returns>
        public static string getQrCode()
        {
            string url = BASE_URL + GET_QR_CODE_URL_HEAD + MACHINE_CODE;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return null;
            }
            Stream myResponseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(myResponseStream);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponseStream.Close();
            response.Close();

            //解析结果
            JObject responseJson = JObject.Parse(result);
            return responseJson["result"].ToString();

        }

        private static string GET_PRINT_ORDERS_URL_HEAD = "Print/getPrintOrderListForMachine/machineCode/";
        public static List<PrintOrder> getPrintOrders()
        {
            string url = BASE_URL + GET_PRINT_ORDERS_URL_HEAD + MACHINE_CODE;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return null;
            }
            Stream myResponseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(myResponseStream);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponseStream.Close();
            response.Close();

            //解析结果
            JObject responseJson = JObject.Parse(result);
            if(int.Parse(responseJson["result"].ToString()) == 0)
            {
                return null;
            }else
            {
                JArray ja = JArray.Parse(responseJson["data"].ToString());
                List<PrintOrder> printOrders = new List<PrintOrder>();
                foreach (JToken token in ja)
                {
                    PrintOrder po = new PrintOrder();

                    JObject jo = JObject.Parse(token.ToString());
                    po.PrintCode = jo["printCode"].ToString();
                    po.PayTime = jo["payTime"].ToString();
                    po.TotalMili = int.Parse(jo["totalMili"].ToString());
                    po.DocNum = int.Parse(jo["docNum"].ToString());
                    po.PaperNum = int.Parse(jo["paperNum"].ToString());
                    JArray objectsJson = JArray.Parse(jo["objects"].ToString());
                    List<PrintObject> printObjects = new List<PrintObject>();
                    foreach(JToken t in objectsJson)
                    {
                        JObject o = JObject.Parse(t.ToString());
                        PrintObject printObject = new PrintObject();
                        printObject.Name = o["name"].ToString();
                        printObject.Ext = o["ext"].ToString();
                        printObjects.Add(printObject);

                    }
                    po.PrintObjects = printObjects;
                    printOrders.Add(po);
                }
                return printOrders;
            }

        }
        private static string REPORT_BUG_URL_HEAD = "Print/reportBug/machineCode/";
        public static bool reportBug(string bug)
        {
            string url = BASE_URL + REPORT_BUG_URL_HEAD + MACHINE_CODE+"/bug/"+bug;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }
            Stream myResponseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(myResponseStream);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponseStream.Close();
            response.Close();

            //解析结果
            JObject responseJson = JObject.Parse(result);
            if (int.Parse(responseJson["result"].ToString()) == 0)
            {
                return false;
            }else
            {
                return true;
            }

        }
        private static string ADD_PAPER_URL_HEAD = "Print/completeAddPaper/machineCode/";
        public static bool completeAddPaper(string passwd)
        {
            string url = BASE_URL + ADD_PAPER_URL_HEAD + MACHINE_CODE + "/passwd/" + passwd;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }
            Stream myResponseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(myResponseStream);
            string result = sr.ReadToEnd();
            sr.Close();
            myResponseStream.Close();
            response.Close();

            //解析结果
            JObject responseJson = JObject.Parse(result);
            if (int.Parse(responseJson["result"].ToString()) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        /// <summary>
        /// 清理下载缓存文件夹
        /// </summary>
        public static void clearDownloadFiles()
        {
            //TODO: 
        }

    }

    class PrintListResult
    {
        private string codeStr;
        private int totalMili;
        private int totalNum;//总张数
        private List<PrintObject> printObjects;
        public string CodeStr
        {
            get
            {
                return codeStr;
            }
            set
            {
                codeStr = value;
            }
        }
        public int TotalMili
        {
            get
            {
                return totalMili;
            }
            set
            {
                totalMili = value;
            }
        }

        public int TotalNum
        {
            get
            {
                return totalNum;
            }
            set
            {
                totalNum = value;
            }
        }

        public List<PrintObject> PrintObjects
        {
            set
            {
                printObjects = value;
            }
            get
            {
                return printObjects;
            }
        }

    }


    /// <summary>
    /// 打印对象配置参数
    /// </summary>
    class PrintObject
    {
        private string location;

        private string id;
        private string type;
        private string name;
        private string ext;
        private int fromPage;
        private int toPage;
        private int count;
        private bool single;
        private int num;
        private int paperNum;
        private int scale;
        /// <summary>
        /// 打印文件本地地址，下载完后设置此属性
        /// </summary>
        public string Location
        {
            set 
            {
                location = value;
            }
            get
            {
                return location;
            }
        }
        public string Id
        {
            get
            {
                return id;
            }
        }
        public string Type
        {
            get
            {
                return type;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }
        public string Ext
        {
            get
            {
                return ext;
            }
            set
            {
                this.ext = value;
            }
        }
        public int FromPage
        {
            get
            {
                return fromPage;
            }
        }
        public int ToPage
        {
            get
            {
                return toPage;
            }
        }
        public int Count
        {
            get
            {
                return count;
            }
        }
        public bool Single
        {
            get
            {
                return single;
            }
        }
        public int Num
        {
            get
            {
                return num;
            }
        }
        public int PaperNum
        {
            get
            {
                return paperNum;
            }
        }
        public int Scale
        {
            get { return scale; }
        }


        /// <summary>
        /// 根据数据类型设置数据
        /// </summary>
        /// <returns>false 表明数据有误</returns>
        public bool set(string value, string valueType)
        {
            switch (valueType)
            {
                case "objectId":
                    id = value;
                    break;
                case "type":
                    type = value;
                    break;
                case "name":
                    name = value;
                    break;
                case "ext":
                    ext = value;
                    break;
                case "fromPage":
                    try
                    {
                        fromPage = int.Parse(value);
                    }
                    catch (Exception) { return false; }
                    break;
                case "toPage":
                    try
                    {
                        toPage = int.Parse(value);
                    }
                    catch (Exception) { return false; }
                    break;
                case "count":
                    try
                    {
                        count = int.Parse(value);
                    }
                    catch (Exception) { return false; }
                    break;
                case "single":
                    try
                    {
                        single = bool.Parse(value);
                    }
                    catch (Exception) { return false; }
                    break;
                case "num":
                    try
                    {
                        num = int.Parse(value);
                    }
                    catch (Exception) { return false; }
                    break;
                case "paperNum":
                    try
                    {
                        paperNum = int.Parse(value);
                    }
                    catch (Exception) { return false; }
                    break;
                case "scale":
                    try
                    {
                        scale = int.Parse(value);
                    }
                    catch (Exception) { return false; }
                    break;
                default :
                    return false;
            }


            return true;
        }

    }
    class PrintOrder
    {
        private string printCode;
        private string payTime;
        private int totalMili;
        private int docNum;
        private int paperNum;
        private List<PrintObject> printObjects;
        public string PrintCode
        {
            get
            {
                return this.printCode;
            }
            set
            {
                this.printCode = value;
            }
        }
        public string PayTime
        {
            get
            {
                return this.payTime;
            }
            set
            {
                this.payTime = value;
            }
        }
        public int TotalMili
        {
            get
            {
                return this.totalMili;
            }
            set
            {
                this.totalMili = value;
            }
        }
        public int DocNum
        {
            get
            {
                return this.docNum;
            }
            set
            {
                this.docNum = value;
            }
        }
        public int PaperNum
        {
            get
            {
                return this.paperNum;
            }
            set
            {
                this.paperNum = value;
            }
        }
        public List<PrintObject> PrintObjects
        {
            set
            {
                printObjects = value;
            }
            get
            {
                return printObjects;
            }
        }
    }
}
