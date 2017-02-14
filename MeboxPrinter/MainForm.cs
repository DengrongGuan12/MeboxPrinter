using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeboxPrinter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.KeyPreview = true;
            InitializeComponent();
            initAutoSize();
            initFullScreenEvent();
            initTime();
            initCodeInputPaint();
            showCodeLabels = new Label[] { 
                codeInput1, codeInput2, codeInput3, codeInput4, codeInput5,
                codeInput6, codeInput7, codeInput8, codeInput9, codeInput10,
                codeInput11, codeInput12, codeInput13, codeInput14, codeInput15
            };
            showPasswdLabels = new Label[]
            {
                passwdLabel1,passwdLabel2,passwdLabel3,passwdLabel4,passwdLabel5,passwdLabel6
            };
            WebHelper.readSettings();
            initListView();
            initPrintOrderListView();
            initScanPanelTimer();
            initGetPrinterStateTimer();
            showDirectorPanel();

        }
        private Timer timerForHeadPanel = new Timer();
        string[] Day = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
        #region 按F11全屏
        private Boolean m_IsFullScreen = false;
        private void MainForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                m_IsFullScreen = !m_IsFullScreen;
                if (m_IsFullScreen)
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                }
                e.Handled = true;
            }else if(e.KeyCode == Keys.F1)
            {
                showDirectorPanel();
            }
        }
        private void fullScreen()
        {
            this.m_IsFullScreen = true;
            this.FormBorderStyle = FormBorderStyle.None;
        }
        #endregion

        #region setting部分click、Paint方法
        private void settingPanel_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel1.ClientRectangle,
           Color.Gray, 1, ButtonBorderStyle.Dashed, //左边
           Color.White, 0, ButtonBorderStyle.Solid, //上边
           Color.DimGray, 0, ButtonBorderStyle.Solid, //右边
           Color.DimGray, 0, ButtonBorderStyle.Solid);//底边

        }

        private void repairBtn_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("设备保修", "About");
            showDirectorPanel();
            hideDirectorPanel();
            reportBugPanel.Visible = true;
            reportBugComboBox.SelectedIndex = 0;
        }

        private void aboutBtn_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("关于米盒", "About");
            showDirectorPanel();
            hideDirectorPanel();
            aboutMeboxPanel.Visible = true;
        }

        private void helpBtn_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("使用帮助", "About");
            showDirectorPanel();
            hideDirectorPanel();
            helpPanel.Visible = true;
        }

        private void contactBtn_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("联系我们", "About");
            showDirectorPanel();
            hideDirectorPanel();
            contactUsPanel.Visible = true;
        }
        #endregion

        #region 导航页面逻辑
        /// <summary>
        /// 导航页面显示
        /// </summary>
        private void showDirectorPanel()
        {
            while (true)
            {
                if(codeInput.Length == 0)
                {
                    break;
                }
                inputDelete();
            }
            releaseThread();
            this.textBox1.Clear();
            scanPanel.Visible = false;
            printOrderListPanel.Visible = false;
            codeInputPanel.Visible = false;
            printingPanel.Visible = false;
            aboutMeboxPanel.Visible = false;
            contactUsPanel.Visible = false;
            reportBugPanel.Visible = false;
            noPaperPanel.Visible = false;
            helpPanel.Visible = false;
            backButton.Visible = false;
            this.headPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(127)))), ((int)(((byte)(109)))));
            titleLabel.Visible = true;
            this.dateLabel.ForeColor = System.Drawing.Color.White;
            this.dayOfWeekLabel.ForeColor = System.Drawing.Color.White;
            this.timeLabel.ForeColor = System.Drawing.Color.White;
            directorPanel.Visible = true;
            headPanel.Visible = true;
            bottomPanel.Visible = true;
            settingPanel.Visible = true;
        }

        private void releaseThread()
        {
            if (getQrCodeTimer != null)
            {
                getQrCodeTimer.Stop();
            }
            if (getPrintListTimer != null)
            {
                getPrintListTimer.Stop();
            }
            if (backToDirectorTimer != null)
            {
                backToDirectorTimer.Stop();
            }
        }

        private void hideDirectorPanel()
        {
            directorPanel.Visible = false;
            titleLabel.Visible = false;
            backButton.Visible = true;
            this.dateLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(127)))), ((int)(((byte)(109)))));
            this.dayOfWeekLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(127)))), ((int)(((byte)(109)))));
            this.timeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(127)))), ((int)(((byte)(109)))));
            this.headPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(251)))), ((int)(((byte)(247)))));

        }
        private void scanDirectorBtn_Click(object sender, EventArgs e)
        {
            hideDirectorPanel();
            showScanPanel();
        }
        private void inputDirectorBtn_Click(object sender, EventArgs e)
        {
            hideDirectorPanel();
            showCodeInputPanel();
            //showPrintingPanel();
        }

        /// <summary>
        /// 初始化键盘全屏事件
        /// </summary>
        private void initFullScreenEvent()
        {
            this.KeyDown += MainForm_KeyDown;
        }

        private void initTime()
        {
            this.timerForHeadPanel_Tick(null,null);
            this.timerForHeadPanel.Interval = 60000;
            this.timerForHeadPanel.Tick += new System.EventHandler(this.timerForHeadPanel_Tick);
            this.timerForHeadPanel.Start();
        }

        private void timerForHeadPanel_Tick(object sender, EventArgs e)
        {
            string week = Day[Convert.ToInt32(DateTime.Now.DayOfWeek.ToString("d"))].ToString();
            int year = System.DateTime.Now.Year;
            int month = System.DateTime.Now.Month;
            int date = System.DateTime.Now.Day;
            int hour = System.DateTime.Now.Hour;
            int minue = System.DateTime.Now.Minute;
            string minuteStr = minue+"";
            if(minue <=9)
            {
                minuteStr = "0" + minue;
            }
            this.dayOfWeekLabel.Text = week;
            this.dateLabel.Text = year + "年" + month + "月" + date + "日";
            this.timeLabel.Text = hour + ":" + minuteStr;

        }
        AutoSizeFormClass asc = new AutoSizeFormClass();
        private void initAutoSize()
        {
            this.Load += Form_Load;
            this.SizeChanged += Form_SizeChanged;
            //this.WindowState = FormWindowState.Maximized;
        }
        //2. 为窗体添加Load事件，并在其方法Form1_Load中，调用类的初始化方法，记录窗体和其控件的初始位置和大小
        private void Form_Load(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);
            this.WindowState = (System.Windows.Forms.FormWindowState)(2);
            fullScreen();
        }
        //3.为窗体添加SizeChanged事件，并在其方法Form1_SizeChanged中，调用类的自适应方法，完成自适应
        private void Form_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
            //this.WindowState = (System.Windows.Forms.FormWindowState)(2);//记录完控件的初始位置和大小后，再最大化
            //fullScreen();
        }
        #endregion

        #region 输入码展示绘制相关方法
        /// <summary>
        /// 初始化输入码展示绘制
        /// </summary>
        private void initCodeInputPaint()
        {
            codeInput1.Paint += codeInput1_Paint;
            codeInput2.Paint += codeInput2_Paint;
            codeInput3.Paint += codeInput3_Paint;
            codeInput4.Paint += codeInput4_Paint;
            codeInput5.Paint += codeInput5_Paint;
            codeInput6.Paint += codeInput6_Paint;
            codeInput7.Paint += codeInput7_Paint;
            codeInput8.Paint += codeInput8_Paint;
            codeInput9.Paint += codeInput9_Paint;
            codeInput10.Paint += codeInput10_Paint;
            codeInput11.Paint += codeInput11_Paint;
            codeInput12.Paint += codeInput12_Paint;
            codeInput13.Paint += codeInput13_Paint;
            codeInput14.Paint += codeInput14_Paint;
            codeInput15.Paint += codeInput15_Paint;
        }

        private void DrawRect(Graphics graphics, Label label)
        {
            float X = float.Parse(label.Width.ToString()) - 1;
            float Y = float.Parse(label.Height.ToString()) - 1;
            Pen pen = new Pen(Color.FromArgb(150, Color.Gray), 1);
            pen.DashStyle = DashStyle.Solid;
            graphics.DrawRectangle(pen, 0, 0, X, Y);
        }

        private void DrawRoundRect(Graphics graphics, Label label)
        {
            float X = float.Parse(label.Width.ToString()) - 1;
            float Y = float.Parse(label.Height.ToString()) - 1;
            PointF[] points = {
                new PointF(2,     0),
                new PointF(X-2,   0),
                new PointF(X-1,   1),
                new PointF(X,     2),
                new PointF(X,     Y-2),
                new PointF(X-1,   Y-1),
                new PointF(X-2,   Y),
                new PointF(2,     Y),
                new PointF(1,     Y-1),
                new PointF(0,     Y-2),
                new PointF(0,     2),
                new PointF(1,     1)
            };
            GraphicsPath path = new GraphicsPath();
            path.AddLines(points);
            Pen pen = new Pen(Color.FromArgb(150, Color.Gray), 1);
            pen.DashStyle = DashStyle.Solid;
            graphics.DrawPath(pen, path);
        }

        private void DrawTopLeftBottomLeftRoundRect(Graphics graphics, Label label)
        {
            float X = float.Parse(label.Width.ToString()) - 1;
            float Y = float.Parse(label.Height.ToString()) - 1;
            PointF[] points = {
                new PointF(2,     0),
                new PointF(X-2,   0),
                new PointF(X,     0),
                new PointF(X,     2),
                new PointF(X,     Y-2),
                new PointF(X,     Y),
                new PointF(X-2,   Y),
                new PointF(2,     Y),
                new PointF(1,     Y-1),
                new PointF(0,     Y-2),
                new PointF(0,     2),
                new PointF(1,     1)
            };
            GraphicsPath path = new GraphicsPath();
            path.AddLines(points);
            Pen pen = new Pen(Color.FromArgb(150, Color.Gray), 1);
            pen.DashStyle = DashStyle.Solid;
            graphics.DrawPath(pen, path);
        }

        private void DrawTopRightBottomRightRoundRect(Graphics graphics, Label label)
        {
            float X = float.Parse(label.Width.ToString()) - 1;
            float Y = float.Parse(label.Height.ToString()) - 1;
            PointF[] points = {
                new PointF(2,     0),
                new PointF(X-2,   0),
                new PointF(X-1,   1),
                new PointF(X,     2),
                new PointF(X,     Y-2),
                new PointF(X-1,   Y-1),
                new PointF(X-2,   Y),
                new PointF(2,     Y),
                new PointF(0,     Y),
                new PointF(0,     Y-2),
                new PointF(0,     2),
                new PointF(0,     0)
            };
            GraphicsPath path = new GraphicsPath();
            path.AddLines(points);
            Pen pen = new Pen(Color.FromArgb(150, Color.Gray), 1);
            pen.DashStyle = DashStyle.Solid;
            graphics.DrawPath(pen, path);
        }

        private void codeInput1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawTopLeftBottomLeftRoundRect(e.Graphics, codeInput1);
        }
        private void codeInput2_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawRect(e.Graphics, codeInput2);
        }
        private void codeInput3_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawRect(e.Graphics, codeInput3);
        }
        private void codeInput4_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawRect(e.Graphics, codeInput4);
        }
        private void codeInput5_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawTopRightBottomRightRoundRect(e.Graphics, codeInput5);
        }
        private void codeInput6_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawTopLeftBottomLeftRoundRect(e.Graphics, codeInput6);
        }
        private void codeInput7_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawRect(e.Graphics, codeInput7);
        }
        private void codeInput8_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawRect(e.Graphics, codeInput8);
        }
        private void codeInput9_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawRect(e.Graphics, codeInput9);
        }
        private void codeInput10_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawTopRightBottomRightRoundRect(e.Graphics, codeInput10);
        }
        private void codeInput11_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawTopLeftBottomLeftRoundRect(e.Graphics, codeInput11);
        }
        private void codeInput12_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawRect(e.Graphics, codeInput12);
        }
        private void codeInput13_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawRect(e.Graphics, codeInput12);
        }
        private void codeInput14_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawRect(e.Graphics, codeInput12);
        }
        private void codeInput15_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            DrawTopRightBottomRightRoundRect(e.Graphics, codeInput15);
        }
        #endregion

        #region 输入码键入逻辑
        private void key1_Click(object sender, EventArgs e)
        {
            inputKey("1");
        }

        private void key2_Click(object sender, EventArgs e)
        {
            inputKey("2");
        }

        private void key3_Click(object sender, EventArgs e)
        {
            inputKey("3");
        }

        private void key4_Click(object sender, EventArgs e)
        {
            inputKey("4");
        }

        private void key5_Click(object sender, EventArgs e)
        {
            inputKey("5");
        }

        private void key6_Click(object sender, EventArgs e)
        {
            inputKey("6");
        }

        private void key7_Click(object sender, EventArgs e)
        {
            inputKey("7");
        }

        private void key8_Click(object sender, EventArgs e)
        {
            inputKey("8");
        }

        private void key9_Click(object sender, EventArgs e)
        {
            inputKey("9");
        }

        private void key0_Click(object sender, EventArgs e)
        {
            inputKey("0");
        }

        private void keyD_Click(object sender, EventArgs e)
        {
            inputDelete();
        }
        /// <summary>
        /// 输入码长度
        /// </summary>
        private static readonly int CODE_LENGTH = 15;
        /// <summary>
        /// 当前输入码
        /// </summary>
        private string codeInput="";
        /// <summary>
        /// 展示输入码的标签列表
        /// </summary>
        private Label[] showCodeLabels;
        /// <summary>
        /// 删除键以外键键入调用方法
        /// </summary>
        private void inputKey(string key)
        {
            if (codeInput.Length == CODE_LENGTH)
            {
                return;
            }
            showCodeLabels[codeInput.Length].Text = key;
            codeInput += key;

            //输入完成时
            if (codeInput.Length == CODE_LENGTH)
            {
                inputComplete();
            }
        }
        /// <summary>
        /// 删除键键入调用方法
        /// </summary>
        private void inputDelete() 
        {
            if (codeInput.Length == 0)
            {
                return;
            }
            showCodeLabels[codeInput.Length - 1].Text = "";
            codeInput = codeInput.Substring(0, codeInput.Length - 1);
        }
        
        /// <summary>
        /// 输入完成，开始按码检查等业务逻辑
        /// </summary>
        private void inputComplete()
        {
            if (printing)
            {
                MessageBox.Show("有文件正在打印，请您耐心等待!", "温馨提示");
                return;
            }
            //根据输入码检索获取打印列表以及打印设置
            PrintListResult printListResult = WebHelper.checkPrintCode(codeInput);
            if (printListResult == null)
            {
                codeInputWrong();
                return;
            }
            if (printListResult.TotalNum > paperNum - offset)
            {
                showNoPaperPanel();
                return;
            }
            showPrintingPanel(printListResult);
            return;

        }
        /// <summary>
        /// 输入码错误
        /// </summary>
        private void codeInputWrong()
        {
            //TODO
            MessageBox.Show("打印码不存在!", "错误");

        }
        /// <summary>
        /// 下载出现问题
        /// </summary>
        private void downloadWrong()
        {
            //TODO
            MessageBox.Show("下载文件失败!", "错误");
        }




        /// <summary>
        /// 输入打印码页面显示
        /// </summary>
        private void showCodeInputPanel()
        {
            codeInput = "";
            codeInputPanel.Visible = true;
        }
        #endregion

        #region 绘制打印页面订单详情
        private void initListView()
        {
            this.printObjectListView.View = View.Details;
            this.printObjectListView.SmallImageList = imageList1;
            this.printObjectListView.Columns.Add("",50, HorizontalAlignment.Center); //一步添加 
            this.printObjectListView.Columns.Add("",340, HorizontalAlignment.Left); //一步添加 
        }
        private void refreshListView(List<PrintObject> pos)
        {
            this.printObjectListView.Items.Clear();
            this.printObjectListView.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度  

            foreach (PrintObject po in pos)
            {
                ListViewItem lvi = new ListViewItem();

                //lvi.ImageIndex = i;     //通过与imageList绑定，显示imageList中第i项图标  

                lvi.Text = po.Ext;

                lvi.SubItems.Add(po.Name);
                lvi.SubItems[0].ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(127)))), ((int)(((byte)(109)))));
                lvi.SubItems[0].Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));

                lvi.SubItems[1].ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(127)))), ((int)(((byte)(109)))));
                lvi.SubItems[1].Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                lvi.UseItemStyleForSubItems = false;

                this.printObjectListView.Items.Add(lvi);
            }

            this.printObjectListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。  
        }
        #endregion

        #region 二维码页面逻辑
        /// <summary>
        /// 扫码页面显示
        /// </summary>
        private void showScanPanel()
        {
            scanPanel.Visible = true;
            startScanPanelTimer();
        }
        public static int getQrCodeInterval = 0;
        public static int getPrintOrderListInterval = 0;
        public static int backToDirectorPanelInterval = 0;
        private void initScanPanelTimer()
        {
            backToDirectorTimer = new Timer();
            backToDirectorTimer.Interval = backToDirectorPanelInterval;
            backToDirectorTimer.Tick += new System.EventHandler(this.backToDirectorPanel_Tick);
            getQrCodeTimer = new Timer();
            getQrCodeTimer.Interval = getQrCodeInterval;//获取二维码
            getQrCodeTimer.Tick += new System.EventHandler(this.getQrCode);
            getPrintListTimer = new Timer();
            getPrintListTimer.Interval = getPrintOrderListInterval;
            getPrintListTimer.Tick += new System.EventHandler(this.getPrintOrderList);
        }

        private void startScanPanelTimer()
        {
            getQrCode(null,null);
            backToDirectorTimer.Start();
            getQrCodeTimer.Start();
            getPrintListTimer.Start();
        }

        private void getQrCode(object sender, EventArgs e)
        {
            Console.WriteLine("get qr code");
            string qrCode = WebHelper.getQrCode();
            //Console.WriteLine(qrCode);
            Bitmap bmp = QRHelper.getQrCodeImg(qrCode);
            if(bmp == null)
            {
                MessageBox.Show("网络错误!");
                this.getQrCodeTimer.Stop();
                return;
            }
            this.qrPictureBox.Image = bmp;
        }

        private void getPrintOrderList(object sender, EventArgs e)
        {
            Console.WriteLine("get print order list");
            getPrintListTimer.Enabled = false;
            List<PrintOrder> printOrders = WebHelper.getPrintOrders();
            if(printOrders != null)
            {
                if(printOrders.Count == 0)
                {
                    MessageBox.Show("您还没有可打印的订单!");
                    return;
                }
                if(printOrders.Count == 1)
                {
                    codeInput = printOrders[0].PrintCode;
                    releaseThread();
                    scanPanel.Visible = false;
                    confirmPrintBtn_Click(null,null);
                    return;
                }
                showPrintOrderListPanel(printOrders);
                return;
            }
            getPrintListTimer.Enabled = true;
        }
        #endregion
        #region 绘制订单列表页面
        private void showPrintOrderListPanel(List<PrintOrder> printOrders)
        {
            releaseThread();
            scanPanel.Visible = false;
            refreshPrintOrderListView(printOrders);
            codeInput = printOrders[0].PrintCode;
            this.printOrderListView.Items[0].BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(222)))), ((int)(((byte)(210)))));
            this.printOrderListView.Items[0].ForeColor = Color.Gray;
            foreach (ListViewItem.ListViewSubItem l in this.printOrderListView.Items[0].SubItems)
            {
                l.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(222)))), ((int)(((byte)(210)))));
                l.ForeColor = Color.Gray;
            }
            printOrderListPanel.Visible = Visible;
        }
        private void initPrintOrderListView()
        {
            this.printOrderListView.View = View.Details;
            this.printOrderListView.SmallImageList = imageList1;
            this.printOrderListView.Columns.Add("打印码", 180, HorizontalAlignment.Center); //一步添加 
            this.printOrderListView.Columns.Add("总计米粒", 100, HorizontalAlignment.Center); //一步添加        
            this.printOrderListView.Columns.Add("文件数", 80, HorizontalAlignment.Center); //一步添加 
            this.printOrderListView.Columns.Add("总张数", 80, HorizontalAlignment.Center); //一步添加 
            this.printOrderListView.Columns.Add("支付时间", 170, HorizontalAlignment.Center); //一步添加 
            this.printOrderListView.Columns.Add("文件列表", 890, HorizontalAlignment.Left); //一步添加 
            this.printOrderListView.ItemSelectionChanged += PrintOrderListView_ItemSelectionChanged;
            this.printOrderListView.ColumnWidthChanging += PrintOrderListView_ColumnWidthChanging;
            //printOrderListView.OwnerDraw = true;
            //printOrderListView.DrawItem += new
            //DrawListViewItemEventHandler(printOrderListView_DrawItem);
            //printOrderListView.DrawSubItem += new
            //    DrawListViewSubItemEventHandler(printOrderListView_DrawSubItem);
            //printOrderListView.DrawColumnHeader += new
            //    DrawListViewColumnHeaderEventHandler(printOrderListView_DrawColumnHeader);

        }
        // Draws the backgrounds for entire ListView items.
        private void printOrderListView_DrawItem(object sender,
            DrawListViewItemEventArgs e)
        {
            if ((e.State & ListViewItemStates.Selected) != 0)
            {
                // Draw the background and focus rectangle for a selected item.
                e.Graphics.FillRectangle(Brushes.Maroon, e.Bounds);
                e.DrawFocusRectangle();
            }
            else
            {
                // Draw the background for an unselected item.
                using (LinearGradientBrush brush =
                    new LinearGradientBrush(e.Bounds, Color.Orange,
                    Color.Maroon, LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }

            // Draw the item text for views other than the Details view.
            if (printOrderListView.View != View.Details)
            {
                e.DrawText();
            }
        }

        // Draws subitem text and applies content-based formatting.
        private void printOrderListView_DrawSubItem(object sender,
            DrawListViewSubItemEventArgs e)
        {
            TextFormatFlags flags = TextFormatFlags.Left;

            using (StringFormat sf = new StringFormat())
            {
                // Store the column text alignment, letting it default
                // to Left if it has not been set to Center or Right.
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        flags = TextFormatFlags.HorizontalCenter;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        flags = TextFormatFlags.Right;
                        break;
                }

                // Draw the text and background for a subitem with a 
                // negative value. 
                double subItemValue;
                if (e.ColumnIndex > 0 && Double.TryParse(
                    e.SubItem.Text, NumberStyles.Currency,
                    NumberFormatInfo.CurrentInfo, out subItemValue) &&
                    subItemValue < 0)
                {
                    // Unless the item is selected, draw the standard 
                    // background to make it stand out from the gradient.
                    if ((e.ItemState & ListViewItemStates.Selected) == 0)
                    {
                        e.DrawBackground();
                    }

                    // Draw the subitem text in red to highlight it. 
                    e.Graphics.DrawString(e.SubItem.Text,
                        printOrderListView.Font, Brushes.Red, e.Bounds, sf);

                    return;
                }

                // Draw normal text for a subitem with a nonnegative 
                // or nonnumerical value.
                e.DrawText(flags);
            }
        }


        // Draws column headers.
        private void printOrderListView_DrawColumnHeader(object sender,
            DrawListViewColumnHeaderEventArgs e)
        {
            using (StringFormat sf = new StringFormat())
            {
                // Store the column text alignment, letting it default
                // to Left if it has not been set to Center or Right.
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                }

                // Draw the standard header background.
                e.DrawBackground();

                // Draw the header text.
                using (Font headerFont =
                            new Font("宋体", 10, FontStyle.Bold))
                {
                    e.Graphics.DrawString(e.Header.Text, headerFont,
                        Brushes.Black, e.Bounds, sf);
                }
            }
            return;
        }

        private void PrintOrderListView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            // 如果调整的不是第一列,就不管了 
            //if (e.ColumnIndex > 0) return;
            // 取消掉正在调整的事件 
            e.Cancel = true;
            // 把新宽度恢复到之前的宽度 
            e.NewWidth = this.printOrderListView.Columns[e.ColumnIndex].Width;
        }

        private void PrintOrderListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (this.printOrderListView.SelectedItems.Count > 0)
            {
                Console.WriteLine("count > 0");
                codeInput = this.printOrderListView.SelectedItems[0].SubItems[0].Text;
                foreach (ListViewItem lvi in this.printOrderListView.Items)
                {
                    lvi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251))))); ;
                    lvi.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(127)))), ((int)(((byte)(109)))));
                    foreach (ListViewItem.ListViewSubItem l in lvi.SubItems)
                    {
                        l.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251))))); ;
                        l.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(127)))), ((int)(((byte)(109)))));
                    }
                }
                for (int i = 0; i < 6; i++)
                {
                    this.printOrderListView.SelectedItems[0].SubItems[i].BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(222)))), ((int)(((byte)(210)))));
                    this.printOrderListView.SelectedItems[0].SubItems[i].ForeColor = Color.Gray;
                }
                this.printOrderListView.SelectedItems[0].Selected = false;
            }
        }

        private void refreshPrintOrderListView(List<PrintOrder> list)
        {
            this.printOrderListView.Items.Clear();
            this.printOrderListView.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度  

            foreach (PrintOrder po in list)
            {
                ListViewItem lvi = new ListViewItem();

                //lvi.ImageIndex = i;     //通过与imageList绑定，显示imageList中第i项图标  

                lvi.Text = po.PrintCode;

                lvi.SubItems.Add(po.TotalMili.ToString());
                lvi.SubItems.Add(po.DocNum.ToString());
                lvi.SubItems.Add(po.PaperNum.ToString());
                lvi.SubItems.Add(po.PayTime);
                string files = "";
                foreach(PrintObject o in po.PrintObjects)
                {
                    files += o.Name + "." + o.Ext + ";\r\n";
                }
                lvi.SubItems.Add(files);
                for(int i = 0; i < 4; i++)
                {
                    lvi.SubItems[i].ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(127)))), ((int)(((byte)(109)))));
                    lvi.SubItems[i].Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                }
                for(int i = 4; i < 6; i++)
                {
                    lvi.SubItems[i].ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(127)))), ((int)(((byte)(109)))));
                    lvi.SubItems[i].Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                }
                lvi.UseItemStyleForSubItems = false;

                this.printOrderListView.Items.Add(lvi);
            }

            this.printOrderListView.EndUpdate();  //结束数据处理，UI界面一次性绘制。  
        }

        private void backToDirectorPanel_Tick(object sender, EventArgs e)
        {
            showDirectorPanel();
        }

        Timer getQrCodeTimer = null;
        Timer backToDirectorTimer = null;
        Timer getPrintListTimer = null;
        #endregion

        private void backButton_Click(object sender, EventArgs e)
        {
            if (printing)
            {
                WebHelper.reportPrintComplete(codeInput);
            }
            showDirectorPanel();
        }
        
        #region 下载打印罗辑
        /// <summary>
        /// 
        /// </summary>
        /// <param name="printListResult"></param>
        private void showPrintingPanel(PrintListResult printListResult)
        {
            this.miliLabel.Text = printListResult.TotalMili.ToString()+"米粒";
            this.paperNumLabel.Text = "共"+printListResult.TotalNum.ToString() + "页";
            refreshListView(printListResult.PrintObjects);
            this.codeInputPanel.Visible = false;
            this.printOrderListPanel.Visible = false;
            this.printingPanel.Visible = true;
            startPrint(printListResult);
            //initBackToDirectorPanelTimer(60000*2);
        }

        private void startPrint(PrintListResult printListResult)
        {
            System.Threading.Thread fThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Printing));
            fThread.Start(printListResult);
        }

        private void completeBtn_Click(object sender, EventArgs e)
        {
            if (printing)
            {
                WebHelper.reportPrintComplete(codeInput);
            }
            showDirectorPanel();
        }
        private bool printing = false;
        private void Printing(Object printListResult)
        {
            printing = true;
            PrintListResult p = (PrintListResult)printListResult;
            codeInput = p.CodeStr;
            foreach (PrintObject po in p.PrintObjects)
            {
                SetTextMesssage("正在下载文件 " + po.Name + " ...\r\n");
                //下载打印列表文件
                if (!WebHelper.downloadFile(po))
                {
                    downloadWrong();
                    continue;
                }
                //打印文件
                SetTextMesssage("文件 " + po.Name + " 下载完成! \r\n");
                SetTextMesssage("正在打印文件 " + po.Name + " ...\r\n");
                PrintHelper.printFile(po);
            }
            SetTextMesssage("打印完成,请取走您的文件!");
            //汇报打印完毕
            WebHelper.reportPrintComplete(p.CodeStr);
            printing = false;
            return;
        }
        private delegate void SetPos(string vinfo);//代理

        private void SetTextMesssage(string vinfo)
        {
            if (this.InvokeRequired)
            {
                SetPos setpos = new SetPos(SetTextMesssage);
                this.Invoke(setpos, new object[] { vinfo });
            }
            else
            {
                this.textBox1.AppendText(vinfo);
            }
        }
        #endregion

        private void confirmPrintBtn_Click(object sender, EventArgs e)
        {
            if (codeInput.Equals(""))
            {
                MessageBox.Show("请先选择一项再进行打印","温馨提示");
            }else
            {
                if (printing)
                {
                    MessageBox.Show("有文件正在打印，请您耐心等待!", "温馨提示");
                    return;
                }
                PrintListResult printListResult = WebHelper.checkPrintCode(codeInput);
                if (printListResult == null)
                {
                    codeInputWrong();
                    return;
                }
                if(printListResult.TotalNum > paperNum - offset)
                {
                    showNoPaperPanel();
                    return;
                }
                showPrintingPanel(printListResult);
            }
        }

        private void commitBugBtn_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(reportBugComboBox.Text);
            if (WebHelper.reportBug(reportBugComboBox.Text))
            {
                MessageBox.Show("提交成功!");
            }else
            {
                MessageBox.Show("提交失败!");
            }
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    Printer printer = new Printer("Pantum P2600 Series");
        //    MessageBox.Show(printer.getStatus().ToString(),"状态");
        //}

        private void initGetPrinterStateTimer()
        {
            Timer getPrinterStateTimer = new Timer();
            getPrinterStateTimer.Interval = 1000;
            getPrinterStateTimer.Tick += new System.EventHandler(this.getPrinterState);
            getPrinterStateTimer.Start();
        }
        private bool isOutOfPaper = false;

        /// <summary>
        /// 记录每个打印任务大概的执行时间
        /// </summary>
        private Dictionary<string, int> printJobTime = new Dictionary<string, int>();

        /// <summary>
        /// 记录每个打印任务的开始时间
        /// </summary>
        private Dictionary<string, DateTime> printJobStart = new Dictionary<string, DateTime>();
        private void getPrinterState(object sender, EventArgs e)
        {
            //Console.WriteLine("检测打印机状态...");
            LocalPrintServer ps = new LocalPrintServer(); // 打印服务器
            PrintQueue queue = ps.DefaultPrintQueue; // 获取默认打印队列
            queue.Refresh(); //更新 PrintQueue 对象的属性
            PrintJobInfoCollection jobInfoCollection = queue.GetPrintJobInfoCollection();
            foreach (PrintSystemJobInfo printJobInfo in jobInfoCollection)
            {
                if (isOutOfPaper)
                {
                    printJobInfo.Cancel();
                    continue;
                }
                Console.WriteLine("当前打印作业 {0} , {1} 已经打印了: {2} 页.",
                    printJobInfo.Name,
                    printJobInfo.JobName,
                    printJobInfo.NumberOfPagesPrinted);
                if (printJobInfo.IsPaperOut)
                {
                    //Console.WriteLine("out of paper");
                    MessageBox.Show("没纸啦!");
                    printJobInfo.Cancel();
                    isOutOfPaper = true;
                }
                //if ((printJobInfo.JobStatus & PrintJobStatus.PaperOut) == PrintJobStatus.PaperOut)
                //{
                //    Console.WriteLine("The printer is out of paper of the size required by the job. Have user add paper.");
                //}

            }
        }

        #region 密码键入罗辑
        public static int PAPER_NUM = 0;
        public static int paperNum = 0;
        public static int offset = 10;
        private Label[] showPasswdLabels;
        private string passwdInput = "";
        private static readonly int PASSWD_LENGTH = 6;

        private void inputPasswd(string key)
        {
            if (passwdInput.Length == PASSWD_LENGTH)
            {
                return;
            }
            showPasswdLabels[passwdInput.Length].Text = key;
            passwdInput += key;
        }
        /// <summary>
        /// 删除键键入调用方法
        /// </summary>
        private void passwdDelete()
        {
            if (passwdInput.Length == 0)
            {
                return;
            }
            showPasswdLabels[passwdInput.Length - 1].Text = "";
            passwdInput = passwdInput.Substring(0, passwdInput.Length - 1);
        }
        private void new_BackButton_Click(object sender, EventArgs e)
        {
            //showNoPaperPanel();
            this.codeInputPanel.Visible = false;
            this.printOrderListPanel.Visible = false;
            this.contactUsPanel.Visible = false;
            this.aboutMeboxPanel.Visible = false;
            this.reportBugPanel.Visible = false;
            this.helpPanel.Visible = false;
            noPaperPanel.Visible = true;
        }

        private void showNoPaperPanel()
        {
            this.codeInputPanel.Visible = false;
            this.printOrderListPanel.Visible = false;
            backButton.Click -= new EventHandler(backButton_Click);// 注销一次click事件
            backButton.Click += new EventHandler(new_BackButton_Click);
            noPaperPanel.Visible = true;
        }

        private void hideNoPaperPanel()
        {
            while(passwdInput.Length > 0)
            {
                passwdDelete();
            }
            backButton.Click -= new EventHandler(new_BackButton_Click);
            backButton.Click += new EventHandler(backButton_Click);
            noPaperPanel.Visible = false;
            showDirectorPanel();
        }

        private void passwdBtn0_Click(object sender, EventArgs e)
        {
            inputPasswd("0");
        }

        private void passwdBtn1_Click(object sender, EventArgs e)
        {
            inputPasswd("1");
        }

        private void passwdBtn2_Click(object sender, EventArgs e)
        {
            inputPasswd("2");
        }

        private void passwdBtn3_Click(object sender, EventArgs e)
        {
            inputPasswd("3");
        }

        private void passwdBtn4_Click(object sender, EventArgs e)
        {
            inputPasswd("4");
        }

        private void passwdBtn5_Click(object sender, EventArgs e)
        {
            inputPasswd("5");
        }

        private void passwdBtn6_Click(object sender, EventArgs e)
        {
            inputPasswd("6");
        }

        private void passwdBtn7_Click(object sender, EventArgs e)
        {
            inputPasswd("7");
        }

        private void passwdBtn8_Click(object sender, EventArgs e)
        {
            inputPasswd("8");
        }

        private void passwdBtn9_Click(object sender, EventArgs e)
        {
            inputPasswd("9");
        }

        private void passwdBtnD_Click(object sender, EventArgs e)
        {
            passwdDelete();
        }
        #endregion

        private void addPaperBtn_Click(object sender, EventArgs e)
        {
            if(passwdInput.Length < PASSWD_LENGTH)
            {
                MessageBox.Show("请输入完整!","警告");
                return;
            }
            if (WebHelper.completeAddPaper(passwdInput))
            {
                paperNum = PAPER_NUM;
                hideNoPaperPanel();
            }else
            {
                MessageBox.Show("机器验证码错误!","警告");
            }
        }
    }
}
