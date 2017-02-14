using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Drawing;
using System.IO;
using PdfPrintingNet;

namespace MeboxPrinter
{
    class PrintHelper
    {
        public static void printFile(PrintObject po)
        {
            if (po.ToPage < po.FromPage) return;
            switch (po.Scale)
            {
                case 1: printFileWithoutZoomOut(po); return;
                case 2: printFileWithZoomOut(po, 1, 2); return;
                case 4: printFileWithZoomOut(po, 2, 2); return;
                case 9: printFileWithZoomOut(po, 3, 3); return;
                default: return;
            }
        }

        /// <summary>
        /// 非缩印
        /// </summary>
        private static void printFileWithoutZoomOut(PrintObject po)
        {
            PdfPrint pdfPrint = new PdfPrint("hello", "world");
            pdfPrint.Pages = po.FromPage + "-" + po.ToPage;
            pdfPrint.Copies = (short)po.Count;
            pdfPrint.PrintInColor = false;
            pdfPrint.PaperSize = PAPER_SIZE;
            pdfPrint.DocumentPrintPageBegin += PdfPrint_DocumentPrintPageBegin;
            pdfPrint.DocumentPrintPageEnd += PdfPrint_DocumentPrintPageEnd;
            PdfPrint.Status status = pdfPrint.Print(po.Location);
        }

        private static void PdfPrint_DocumentPrintPageEnd(object sender, PdfPrint.DocumentPrintPageEventArgs e)
        {
            Console.WriteLine("end print page from pdf_print");
        }

        private static void PdfPrint_DocumentPrintPageBegin(object sender, PdfPrint.DocumentPrintPageEventArgs e)
        {
            Console.WriteLine("start print page from pdf_print");
            MainForm.paperNum--;
        }

        /// <summary>
        /// A4宽度 单位为1/100英寸
        /// </summary>
        public static readonly int A4_WIDTH = 827;
        /// <summary>
        /// A4高度 单位为1/100英寸
        /// </summary>
        public static readonly int A4_HEIGHT = 1169;
        public static readonly PaperSize PAPER_SIZE = new PaperSize("A4", A4_WIDTH, A4_HEIGHT);
        public static readonly int DPI = 300;
        /// <summary>
        /// 缩印
        /// </summary>
        /// <param name="x">每行多少页</param>
        /// <param name="y">每列多少页</param>
        private static void printFileWithZoomOut(PrintObject po, int x, int y)
        {
            string location = po.Location;
            int fromPage = po.FromPage;
            int toPage = po.ToPage;
            int copies = po.Count;
            PdfPrint pdfPrint = new PdfPrint("hello", "world");

            //生成图片
            int pageCount = toPage - fromPage + 1;
            Bitmap[] pages = new Bitmap[pageCount];

            int j = 0;
            for (int pageNum = fromPage; pageNum <= toPage; pageNum++)
            {
                pages[j] = pdfPrint.GetBitmapFromPdfPage(location, pageNum, DPI, DPI);
                j++;
            }

            //打印图片
            int zoomWidth = A4_WIDTH / x;
            int zoomHeight = A4_HEIGHT / y;
            List<ZoomPrinter> zps = new List<ZoomPrinter>();
            for (int k = 0; k < pages.Length; k += x * y)
            {
                ZoomPrinter zp = new ZoomPrinter(pages, k, x, y, zoomWidth, zoomHeight);
                zps.Add(zp);
            }
            //按份数打印
            for (int i = 0; i < copies; i++)
            {
                foreach (ZoomPrinter zp in zps)
                {
                    zp.print();
                }
            }
            //释放bmp资源
            foreach (Bitmap page in pages)
            {
                page.Dispose();
            }
                

        }

        /// <summary>
        /// 缩印辅助对象
        /// </summary>
        class ZoomPrinter
        {
            private Bitmap[] bitmaps;
            private int k, x, y;
            private int zoomWidth, zoomHeight;
            /// <summary>
            /// </summary>
            /// <param name="bitmaps">全部页</param>
            /// <param name="k">缩印页起始页码</param>
            /// <param name="x">行缩印页数</param>
            /// <param name="y">列缩印页数</param>
            /// <param name="zoomWidth">缩印后宽</param>
            /// <param name="zoomHeight">缩印后高</param>
            public ZoomPrinter(Bitmap[] bitmaps, int k, int x, int y, int zoomWidth, int zoomHeight)
            {
                this.bitmaps = bitmaps;
                this.k = k;
                this.x = x;
                this.y = y;
                this.zoomWidth = zoomWidth;
                this.zoomHeight = zoomHeight;
            }

            public void print()
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintController = new StandardPrintController();
                printDocument.PrintPage += printDocument_PrintPage;
                printDocument.Print();
                printDocument.Dispose();
            }

            /// <summary>
            /// 将单页的pdf缩小绘制到打印graphic上
            /// </summary>
            private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
            {
                Console.WriteLine("print page");
                MainForm.paperNum--;
                e.PageSettings.Color = false;
                e.PageSettings.PaperSize = PAPER_SIZE;
                for (int j = 0; j < y; j++)
                {
                    for (int i = 0; i < x; i++)
                    {
                        int index = k + j * x + i;
                        if(index == bitmaps.Length)
                        {
                            break;
                        }
                        e.Graphics.DrawImage(bitmaps[index], i * zoomWidth, j * zoomHeight, zoomWidth, zoomHeight);
                    }
                }
            }
        }

    }

}
