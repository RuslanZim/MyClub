using Guna.UI2.WinForms;
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace MyClub
{
    public partial class Profile : Form
    {
        public Profile()
        {
            InitializeComponent();

            // привязка событий — только здесь
            this.Load += Profile_Load;
            btnChangeCredentials.Click += BtnChangeCredentials_Click;
            btnSaveProfile.Click += BtnSaveProfile_Click;
            btnCancel.Click += BtnCancel_Click;
            btnChangePhoto.Click += BtnChangePhoto_Click;
        }

        #region — Инициализация контролов —

        private void Profile_Load(object sender, EventArgs e)
        {
            InitControls();
            LoadProfileData();
        }

        private void InitControls()
        {
            // Дата рождения — заменить TextBox на Guna2DateTimePicker
            datePicker.Format = DateTimePickerFormat.Short;

            // Изображение профиля
            pictureBoxAvatar.SizeMode = PictureBoxSizeMode.Zoom;
        }

        #endregion

        #region — Загрузка данных —

        private void LoadProfileData()
        {
            var user = PersonalData.Current;
            if (user == null)
            {
                MessageBox.Show("Данные пользователя не загружены!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Текстовые поля
            txtLogin.Text = user.Login;
            txtPassword.Text = user.Password;
            txtLastName.Text = user.LastName;
            txtFirstName.Text = user.FirstName;
            txtFatherName.Text = user.FatherName;
            txtPhone.Text = user.PhoneNumber;
            txtEmail.Text = user.Email;

            // Дата рождения
            if (user.DateBirth.HasValue)
                datePicker.Value = user.DateBirth.Value;
            else
                datePicker.Value = DateTime.Today;

            // Фото
            if (user.Photo != null && user.Photo.Length > 0)
            {
                using (var ms = new MemoryStream(user.Photo))
                    pictureBoxAvatar.Image = Image.FromStream(ms);
            }
            else
            {
                pictureBoxAvatar.Image = null;
            }
        }

        #endregion

        #region — Команды пользователя —

        private void BtnChangeCredentials_Click(object sender, EventArgs e)
        {
            using (var dlg = new ChangeCredentialsForm())
            {
                dlg.CurrentLogin = PersonalData.Current.Login;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // перезагрузить только логин/пароль
                    txtLogin.Text = PersonalData.Current.Login;
                    txtPassword.Text = PersonalData.Current.Password;
                    MessageBox.Show("Логин/пароль успешно изменены!", "Успех",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            // чтение из контролов
            var newLast = txtLastName.Text.Trim();
            var newFirst = txtFirstName.Text.Trim();
            var newFather = txtFatherName.Text.Trim();
            var newEmail = txtEmail.Text.Trim();
            var newPhone = txtPhone.Text.Trim();
            DateTime? newDOB = datePicker.Value;

            // валидация
            if (string.IsNullOrEmpty(newLast) ||
                string.IsNullOrEmpty(newFirst) ||
                string.IsNullOrEmpty(newFather) ||
                string.IsNullOrEmpty(newEmail))
            {
                MessageBox.Show("Заполните все обязательные поля.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Сохранить изменения профиля?", "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes)
                return;

            // обновление в БД
            var db = new DB();
            bool ok = db.UpdateUserProfile(
                PersonalData.Current.UserId,
                newLast, newFirst, newFather,
                newDOB, newPhone, newEmail
            );

            if (ok)
            {
                // обновляем модель
                PersonalData.Current.UpdateProfile(
                    newLast, newFirst, newFather,
                    newDOB, newPhone, newEmail
                );

                MessageBox.Show("Данные профиля сохранены.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении профиля.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChangePhoto_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    byte[] data = File.ReadAllBytes(dlg.FileName);
                    var db = new DB();
                    if (db.UpdateUserPhoto(PersonalData.Current.UserId, data))
                    {
                        PersonalData.Current.UpdatePhoto(data);
                        using (var ms = new MemoryStream(data))
                            pictureBoxAvatar.Image = Image.FromStream(ms);

                        MessageBox.Show("Фото обновлено.", "Успех",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось сохранить фото.", "Ошибка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при чтении файла: " + ex.Message,
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        private void BtnCancel_Click(object sender, EventArgs e)
        {

            this.Close();
            if (this.TopLevelControl is Form1 mainForm)
            {
                mainForm.OpenForm(new Users()); 
            }
        }
    }
}
