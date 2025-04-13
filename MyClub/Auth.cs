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
    public partial class Auth : Form
    {
        public Auth()
        {
            InitializeComponent();
        }
            
        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2Button2_Click(object sender, EventArgs e) //кнопка входа
        {
            // Проверяем, что поля не пустые
            if (string.IsNullOrWhiteSpace(guna2TextBox6.Text) || string.IsNullOrWhiteSpace(guna2TextBox5.Text))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            var login = guna2TextBox6.Text;
            var password = guna2TextBox5.Text;
            var PD = new PersonalData();

            // Проверяем, что логин и пароль соответствуют данным в базе
            if (PD.SetPersonalData(login, password))
            {
                var mainForm = new Form1();
                mainForm.Show();
                this.Hide();
            }
            else 
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void guna2CircleButton1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
