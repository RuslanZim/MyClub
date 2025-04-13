using System;
using System.Windows.Forms;

namespace MyClub
{
    public partial class Auth : Form
    {
        public Auth()
        {
            InitializeComponent();
        }

        private void guna2Button2_Click(object sender, EventArgs e) // Кнопка "Вход"
        {
            if (string.IsNullOrWhiteSpace(guna2TextBox6.Text) || string.IsNullOrWhiteSpace(guna2TextBox5.Text))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string login = guna2TextBox6.Text;
            string password = guna2TextBox5.Text;
            PersonalData pd = new PersonalData();

            if (pd.SetPersonalData(login, password))
            {
                // Сохраняем данные текущего пользователя глобально
                PersonalData.Current = pd;

                // Открываем главную форму
                Form1 mainForm = new Form1();
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

