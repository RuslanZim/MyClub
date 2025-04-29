using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Windows.Forms;

namespace MyClub
{
    public partial class Form1 : Form
    {
       
        public Form1()
        {
            InitializeComponent();
        }

        bool draging = false;
        Point dragCursorPoint;
        Point dragFormPoint;

        private Color activeBackgrountColor=Color.FromArgb(49,69,96);
        private Color activeForegrountColor = Color.FromArgb(244,146,12);

        private Color defaultBackgrountColor = Color.FromArgb(30, 42, 58);
        private Color defaultForegrountColor = Color.FromArgb(240, 240, 240);

        private Form activeForm = null;

        private void OpenForm(Form childForm)
        {
            if (activeForm != null)
            {
                activeForm.Close();
            }
            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panel3.Controls.Add(childForm);
            panel3.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }
        
        private void SetButtonColors(IconButton button, Color backColor, Color foreColor)
        {
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.IconColor = foreColor;
        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            IconButton activeButton= (IconButton)sender;
            SetButtonColors(activeButton, activeBackgrountColor, activeForegrountColor);

            leftPanel1.Visible = true;

            SetButtonColors(iconButton2, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton3, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton4, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton5, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton6, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton7, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton8, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton9, defaultBackgrountColor, defaultForegrountColor);

            leftPanel2.Visible = false;
            leftPanel3.Visible = false;
            leftPanel4.Visible = false;
            leftPanel5.Visible = false;
            leftPanel6.Visible = false;
            leftPanel7.Visible = false;
            leftPanel8.Visible = false;
            leftPanel9.Visible = false;

            OpenForm(new Profile());
        }

        private void iconButton2_Click(object sender, EventArgs e)
        {
            IconButton activeButton = (IconButton)sender;
            SetButtonColors(activeButton, activeBackgrountColor, activeForegrountColor);

            leftPanel2.Visible = true;

            SetButtonColors(iconButton1, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton3, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton4, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton5, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton6, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton7, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton8, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton9, defaultBackgrountColor, defaultForegrountColor);

            leftPanel1.Visible = false;
            leftPanel3.Visible = false;
            leftPanel4.Visible = false;
            leftPanel5.Visible = false;
            leftPanel6.Visible = false;
            leftPanel7.Visible = false;
            leftPanel8.Visible = false;
            leftPanel9.Visible = false;

            OpenForm(new Users());
        }

        private void iconButton3_Click(object sender, EventArgs e)
        {
            IconButton activeButton = (IconButton)sender;
            SetButtonColors(activeButton, activeBackgrountColor, activeForegrountColor);

            leftPanel3.Visible = true;

            SetButtonColors(iconButton1, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton2, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton4, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton5, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton6, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton7, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton8, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton9, defaultBackgrountColor, defaultForegrountColor);

            leftPanel1.Visible = false;
            leftPanel2.Visible = false;
            leftPanel4.Visible = false;
            leftPanel5.Visible = false;
            leftPanel6.Visible = false;
            leftPanel7.Visible = false;
            leftPanel8.Visible = false;
            leftPanel9.Visible = false;
        }

        private void iconButton4_Click(object sender, EventArgs e)
        {
            IconButton activeButton = (IconButton)sender;
            SetButtonColors(activeButton, activeBackgrountColor, activeForegrountColor);

            leftPanel4.Visible = true;

            SetButtonColors(iconButton1, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton2, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton3, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton5, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton6, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton7, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton8, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton9, defaultBackgrountColor, defaultForegrountColor);

            leftPanel1.Visible = false;
            leftPanel2.Visible = false;
            leftPanel3.Visible = false;
            leftPanel5.Visible = false;
            leftPanel6.Visible = false;
            leftPanel7.Visible = false;
            leftPanel8.Visible = false;
            leftPanel9.Visible = false;
        }

        private void iconButton5_Click(object sender, EventArgs e)
        {
            IconButton activeButton = (IconButton)sender;
            SetButtonColors(activeButton, activeBackgrountColor, activeForegrountColor);

            leftPanel5.Visible = true;

            SetButtonColors(iconButton1, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton2, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton3, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton4, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton6, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton7, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton8, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton9, defaultBackgrountColor, defaultForegrountColor);

            leftPanel1.Visible = false;
            leftPanel2.Visible = false;
            leftPanel3.Visible = false;
            leftPanel4.Visible = false;
            leftPanel6.Visible = false;
            leftPanel7.Visible = false;
            leftPanel8.Visible = false;
            leftPanel9.Visible = false;
        }

        private void iconButton6_Click(object sender, EventArgs e)
        {
            IconButton activeButton = (IconButton)sender;
            SetButtonColors(activeButton, activeBackgrountColor, activeForegrountColor);

            leftPanel6.Visible = true;

            SetButtonColors(iconButton1, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton2, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton3, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton4, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton5, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton7, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton8, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton9, defaultBackgrountColor, defaultForegrountColor);

            leftPanel1.Visible = false;
            leftPanel2.Visible = false;
            leftPanel3.Visible = false;
            leftPanel4.Visible = false;
            leftPanel5.Visible = false;
            leftPanel7.Visible = false;
            leftPanel8.Visible = false;
            leftPanel9.Visible = false;
        }

        private void iconButton7_Click(object sender, EventArgs e)
        {
            IconButton activeButton = (IconButton)sender;
            SetButtonColors(activeButton, activeBackgrountColor, activeForegrountColor);

            leftPanel7.Visible = true;

            SetButtonColors(iconButton1, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton2, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton3, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton4, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton5, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton6, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton8, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton9, defaultBackgrountColor, defaultForegrountColor);

            leftPanel1.Visible = false;
            leftPanel2.Visible = false;
            leftPanel3.Visible = false;
            leftPanel4.Visible = false;
            leftPanel5.Visible = false;
            leftPanel6.Visible = false;
            leftPanel8.Visible = false;
            leftPanel9.Visible = false;
        }

        private void iconButton8_Click(object sender, EventArgs e)
        {
            IconButton activeButton = (IconButton)sender;
            SetButtonColors(activeButton, activeBackgrountColor, activeForegrountColor);

            leftPanel8.Visible = true;

            SetButtonColors(iconButton1, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton2, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton3, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton4, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton5, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton6, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton7, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton9, defaultBackgrountColor, defaultForegrountColor);

            leftPanel1.Visible = false;
            leftPanel2.Visible = false;
            leftPanel3.Visible = false;
            leftPanel4.Visible = false;
            leftPanel5.Visible = false;
            leftPanel6.Visible = false;
            leftPanel7.Visible = false;
            leftPanel9.Visible = false;
        }

        private void iconButton9_Click(object sender, EventArgs e)
        {
            IconButton activeButton = (IconButton)sender;
            SetButtonColors(activeButton, activeBackgrountColor, activeForegrountColor);

            leftPanel9.Visible = true;

            SetButtonColors(iconButton1, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton2, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton3, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton4, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton5, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton6, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton7, defaultBackgrountColor, defaultForegrountColor);
            SetButtonColors(iconButton8, defaultBackgrountColor, defaultForegrountColor);

            leftPanel1.Visible = false;
            leftPanel2.Visible = false;
            leftPanel3.Visible = false;
            leftPanel4.Visible = false;
            leftPanel5.Visible = false;
            leftPanel6.Visible = false;
            leftPanel7.Visible = false;
            leftPanel8.Visible = false;
        }

        private void iconButton13_Click(object sender, EventArgs e)
        {
           Application.Exit();
        }

        private void iconButton12_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void iconButton10_Click(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            draging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            draging= false; 
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (draging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
           
        }

        
    }

   
    

}
