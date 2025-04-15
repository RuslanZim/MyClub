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

            // Загрузка фото
            if (PersonalData.Current.Photo != null && PersonalData.Current.Photo.Length > 0)
            {
                using (var ms = new System.IO.MemoryStream(PersonalData.Current.Photo))
                {
                    pictureBox1.Image = System.Drawing.Image.FromStream(ms);
                }
            }
            else
            {
                pictureBox1.Image = null; 
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e) // Сохранение логина и пароля
        {
            // Открываем новую форму для смены логина/пароля
            using (var changeForm = new ChangeCredentialsForm())
            {
                // Передаём в новую форму текущий логин — чтобы отобразить его по умолчанию
                changeForm.CurrentLogin = PersonalData.Current.Login;

                // Запускаем форму модально
                DialogResult result = changeForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // Если пользователь нажал "OK" и всё прошло успешно,
                    // перезагрузим поля, чтобы отобразить изменённый логин/пароль
                    guna2TextBox10.Text = PersonalData.Current.Login;
                    guna2TextBox9.Text = PersonalData.Current.Password;

                    MessageBox.Show("Логин/пароль успешно изменены!",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e) // Сохранение профиля
        {
            string newEmail = guna2TextBox4.Text;
            string newLastName = guna2TextBox2.Text;
            string newFirstName = guna2TextBox1.Text;
            string newFatherName = guna2TextBox3.Text;
            DateTime? newDateBirth = null;
            string newPhoneNumber = guna2TextBox5.Text;

            // Проверяем, что поля не пусты
            if (string.IsNullOrEmpty(newEmail) || string.IsNullOrEmpty(newLastName) ||
                string.IsNullOrEmpty(newFirstName) || string.IsNullOrEmpty(newFatherName))
            {
                MessageBox.Show("Все поля необходимо заполнить",
                    "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Проверяем корректность даты рождения
            if (!string.IsNullOrEmpty(guna2TextBox6.Text) && DateTime.TryParse(guna2TextBox6.Text, out DateTime dateBirth))
            {
                newDateBirth = dateBirth;
            }
            // Спрашиваем подтверждение
            DialogResult result = MessageBox.Show("Изменить профиль?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                // Обновляем данные в БД
                DB db = new DB();
                bool success = db.UpdateUserProfile(PersonalData.Current.UserId,
                    newLastName, newFirstName, newFatherName, newDateBirth, newPhoneNumber, newEmail); 
                if (success)
                {
                    // Обновляем локальный объект PersonalData через публичный метод
                    PersonalData.Current.UpdateProfile(newLastName, newFirstName, newFatherName, newDateBirth, newPhoneNumber, newEmail);
                    MessageBox.Show("Данные успешно обновлены!",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении данных пользователя.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void label4_Click(object sender, EventArgs e) //Сменить фото
        {
            // Диалог выбора изображения
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Фильтр для облегчения поиска картинок
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Считываем выбранный файл в массив байтов
                        byte[] fileData = System.IO.File.ReadAllBytes(openFileDialog.FileName);

                        // Обновляем в БД
                        DB db = new DB();
                        bool success = db.UpdateUserPhoto(PersonalData.Current.UserId, fileData);
                        if (success)
                        {
                            // Локально обновляем PersonalData и PictureBox
                            PersonalData.Current.UpdatePhoto(fileData);

                            using (var ms = new System.IO.MemoryStream(fileData))
                            {
                                pictureBox1.Image = System.Drawing.Image.FromStream(ms);
                            }

                            MessageBox.Show("Фото успешно обновлено!",
                                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при обновлении фото.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка чтения файла: " + ex.Message,
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

    }
}
