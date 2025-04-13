using Guna.UI2.WinForms;
using System;
using System.Windows.Forms;

namespace MyClub
{
    public partial class Profile : Form
    {
        public Profile()
        {
            InitializeComponent();
        }

        private void Profile_Load_1(object sender, EventArgs e)
        {
            LoadPersonalData();
        }

        private void LoadPersonalData()
        {
            // Получаем данные текущего пользователя из глобального свойства
            if (PersonalData.Current == null)
            {
                MessageBox.Show("Данные пользователя не загружены!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            guna2TextBox10.Text = PersonalData.Current.Login;
            guna2TextBox9.Text = PersonalData.Current.Password;
            guna2TextBox2.Text = PersonalData.Current.LastName;
            guna2TextBox1.Text = PersonalData.Current.FirstName;
            guna2TextBox3.Text = PersonalData.Current.FatherName;
            guna2TextBox6.Text = PersonalData.Current.DateBirth.HasValue
                ? PersonalData.Current.DateBirth.Value.ToString("yyyy-MM-dd")
                : "";
            guna2TextBox5.Text = PersonalData.Current.PhoneNumber;
            guna2TextBox4.Text = PersonalData.Current.Email;
        }

        
    }
}
