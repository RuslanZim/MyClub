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


        private void guna2CircleButton1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void guna2CheckBox1_CheckedChanged(object sender, EventArgs e)//кнопка показать пароль
        {
            if (guna2CheckBox1.Checked)
            {
                guna2TextBox2.UseSystemPasswordChar = false;
                guna2TextBox3.UseSystemPasswordChar = false;
            }
            else
            {
                guna2TextBox2.UseSystemPasswordChar = true;
                guna2TextBox3.UseSystemPasswordChar = true;
            }
        

        }

        private void guna2CheckBox2_CheckedChanged(object sender, EventArgs e)//кнопка показать пароль
        {
            if (guna2CheckBox2.Checked)
            {
                guna2TextBox5.UseSystemPasswordChar = false;
            }
            else
            {
                guna2TextBox5.UseSystemPasswordChar = true;
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)//кнопка регстрация
        {
            string regLogin = guna2TextBox1.Text.Trim();
            string regPassword = guna2TextBox2.Text;
            string regConfirm = guna2TextBox3.Text;

            // Проверяем, что поля не пусты
            if (!RegistrationValidator.ValidateNonEmpty(regLogin, regPassword, regConfirm))
            {
                MessageBox.Show("Все поля регистрации должны быть заполнены.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Валидируем пароль
            if (!RegistrationValidator.ValidatePassword(regPassword, regConfirm, out string error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Другие поля, которые не вводятся пользователем, задаем по умолчанию
            string role = "Пользователь";
            string email = "";
            string lastName = "";
            string firstName = "";
            string fatherName = "";
            DateTime? dateBirth = null;
            string phone = "";
            byte[] photo = null;

            DB db = new DB();
            bool registered = db.RegisterUser(regLogin, regPassword, role, email, lastName, firstName, fatherName, dateBirth, phone, photo);
            if (registered)
            {
                MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти в систему.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                guna2TextBox1.Clear();
                guna2TextBox2.Clear();
                guna2TextBox3.Clear();
            }
            else
            {
                MessageBox.Show("Ошибка регистрации. Возможно, этот логин уже занят.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

