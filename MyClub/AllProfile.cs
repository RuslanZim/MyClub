using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;         // для MemoryStream
using System.Drawing;    // для Image и Bitmap
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace MyClub
{
    public partial class AllProfile : Form
    {
        public AllProfile()
        {
            InitializeComponent();
        }

        private void AllProfile_Load(object sender, EventArgs e)
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
            guna2TextBox7.Text = PersonalData.Current.Role;
            guna2TextBox2.Text = PersonalData.Current.LastName;
            guna2TextBox1.Text = PersonalData.Current.FirstName;
            guna2TextBox3.Text = PersonalData.Current.FatherName;
            guna2TextBox6.Text = PersonalData.Current.DateBirth.HasValue
                ? PersonalData.Current.DateBirth.Value.ToString("yyyy-MM-dd")
                : "";
            guna2TextBox5.Text = PersonalData.Current.PhoneNumber;
            guna2TextBox4.Text = PersonalData.Current.Email;

            // Загрузка фото
            // Освободим предыдущую картинку
            pictureBox2.Image?.Dispose();

            if (PersonalData.Current.Photo != null && PersonalData.Current.Photo.Length > 0)
            {
                // Считаем поток и сразу клонируем в независимый Bitmap
                using (var ms = new MemoryStream(PersonalData.Current.Photo))
                using (var img = Image.FromStream(ms))
                {
                    pictureBox2.Image = new Bitmap(img);
                }
            }
            else
            {
                pictureBox2.Image = null;
                MessageBox.Show("Фото отсутствует.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void guna2Button2_Click(object sender, EventArgs e) // Сохранение профиля
        {
            // Считываем значения из полей
            string newLogin = guna2TextBox10.Text;
            string newPassword = guna2TextBox9.Text;
            string newRole = guna2TextBox7.Text;
            string newLastName = guna2TextBox2.Text;
            string newFirstName = guna2TextBox1.Text;
            string newFatherName = guna2TextBox3.Text;
            string newEmail = guna2TextBox4.Text;
            string newPhone = guna2TextBox5.Text;

            DateTime? newDOB = null;
            if (DateTime.TryParse(guna2TextBox6.Text, out DateTime dt))
                newDOB = dt;

            // Валидация
            if (string.IsNullOrWhiteSpace(newLogin) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(newRole) ||
                string.IsNullOrWhiteSpace(newLastName) ||
                string.IsNullOrWhiteSpace(newFirstName))
            {
                MessageBox.Show("Поля Логин, Пароль, Роль, Имя и Фамилия обязательны.",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Сохранить все изменения?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var db = new DB();

            bool ok = db.SaveUserData(
                PersonalData.Current.UserId,
                newLogin, newPassword, newRole,
                newLastName, newFirstName, newFatherName,
                newDOB, newPhone, newEmail, PersonalData.Current.Photo
            );

            if (!ok)
            {
                MessageBox.Show("Ошибка при сохранении данных!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Перезагрузка текущей модели
            PersonalData.Current.SetPersonalDataById(PersonalData.Current.UserId);

            MessageBox.Show("Данные успешно сохранены!", "Успех",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Теперь сменим форму
            if (this.TopLevelControl is Form1 mainForm)
            {
                this.Close();
                mainForm.OpenForm(new Users());
            }
            else
            {
                this.Close();
            }
        }
         

        private void label4_Click_1(object sender, EventArgs e)
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
                                pictureBox2.Image = System.Drawing.Image.FromStream(ms);
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
