using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyClub
{
    public partial class Profile : Form
    {

        private DB db;
        public Profile()
        {
            db = new DB();
            InitializeComponent();
        }

        private void Profile_Load(object sender, EventArgs e)
        {
            Load_PersonalData(); // Загрузка данных профиля
        }

        private void Load_PersonalData()
        {
            guna2TextBox2.Text = PersonalData.LastName;
            guna2TextBox1.Text = PersonalData.FirstName;
            guna2TextBox3.Text = PersonalData.FatherName;
            guna2TextBox6.Text = PersonalData.DateBirth.ToString();
            guna2TextBox5.Text = PersonalData.PhoneNumber;
            guna2TextBox10.Text = PersonalData.Login;
            guna2TextBox9.Text = PersonalData.Password;
            guna2TextBox4.Text = PersonalData.Email;
        }
    }
}
