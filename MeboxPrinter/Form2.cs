using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Printer
{
    public partial class Form2 : Form
    {
        Boolean m_IsFullScreen = false;
        public Form2()
        {
            this.KeyPreview = true;
            InitializeComponent();
            MessageBox.Show(this.label1.ForeColor+"", "About");
        }
        private void Form2_KeyDown(object sender, KeyEventArgs e)
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
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
