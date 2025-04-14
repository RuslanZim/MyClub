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

        private void guna2Button1_Click(object sender, EventArgs e) // Сохранение логина и пароля
        {
            string newLogin = guna2TextBox10.Text;
            string newPassword = guna2TextBox9.Text;

            // Проверяем, что поля не пусты
            if (string.IsNullOrEmpty(newLogin) || string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Все поля необходимо заполнить",
                    "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Спрашиваем подтверждение
            DialogResult result = MessageBox.Show("Изменить логин и пароль?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                // Обновляем данные в БД
                DB db = new DB();
                bool success = db.UpdateUserLoginPassword(PersonalData.Current.UserId, newLogin, newPassword);

                if (success)
                {
                    // Обновляем локальный объект PersonalData через публичный метод
                    PersonalData.Current.UpdateCredentials(newLogin, newPassword);

                    MessageBox.Show("Данные успешно обновлены!",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении данных пользователя.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Действие было отменено!", "Уведомление",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
