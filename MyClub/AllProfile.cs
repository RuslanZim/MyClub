using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MyClub
{
    public partial class AllProfile : Form
    {
        private readonly PersonalData _model;
        private readonly bool _isNew;

        public AllProfile(PersonalData model)
        {
            InitializeComponent();
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _isNew = false;
        }

        public AllProfile()
        {
            InitializeComponent();
            _model = new PersonalData();
            _isNew = true;
            Text = "Новый пользователь";
            btnSave.Text = "Создать";
        }

        private void AllProfile_Load(object sender, EventArgs e)
        {
            InitControls();
            if (!_isNew)
                LoadModelToForm();
        }

        #region — Инициализация —

        private void InitControls()
        {
            // События
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;
            btnUploadPhoto.Click += BtnUploadPhoto_Click;

            // Настройка DateTimePicker
            datePicker.Format = DateTimePickerFormat.Custom;
            datePicker.CustomFormat = "yyyy-MM-dd";
        }

        #endregion

        #region — Загрузка модели в форму —

        private void LoadModelToForm()
        {
            txtLogin.Text = _model.Login;
            txtPassword.Text = _model.Password;
            txtRole.Text = _model.Role;
            txtLastName.Text = _model.LastName;
            txtFirstName.Text = _model.FirstName;
            txtFatherName.Text = _model.FatherName;
            txtEmail.Text = _model.Email;
            txtPhone.Text = _model.PhoneNumber;

            if (_model.DateBirth.HasValue)
                datePicker.Value = _model.DateBirth.Value;
            else
                datePicker.Checked = false;

            LoadPhoto(_model.Photo);
        }

        #endregion

        #region — Работа с фото —

        private void LoadPhoto(byte[] data)
        {
            pictureBoxPhoto.Image?.Dispose();
            pictureBoxPhoto.Image = null;

            if (data != null && data.Length > 0)
            {
                using (var ms = new MemoryStream(data))
                    pictureBoxPhoto.Image = new Bitmap(Image.FromStream(ms));
            }
        }

        private void BtnUploadPhoto_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = "Изображения|*.jpg;*.png;*.bmp" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var bytes = File.ReadAllBytes(dlg.FileName);
                LoadPhoto(bytes);
                _model.UpdatePhoto(bytes);
            }
        }

        #endregion

        #region — Валидация —

        private bool ValidateInputs(out string message)
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(txtLogin.Text)) missing.Add("Логин");
            if (string.IsNullOrWhiteSpace(txtPassword.Text)) missing.Add("Пароль");
            if (string.IsNullOrWhiteSpace(txtRole.Text)) missing.Add("Роль");
            if (string.IsNullOrWhiteSpace(txtLastName.Text)) missing.Add("Фамилия");
            if (string.IsNullOrWhiteSpace(txtFirstName.Text)) missing.Add("Имя");

            if (missing.Count > 0)
            {
                message = "Не заполнены обязательные поля:\n" + string.Join(", ", missing);
                return false;
            }

            message = null;
            return true;
        }

        #endregion

        #region — Сохранение профиля —

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string error;
            if (!ValidateInputs(out error))
            {
                MessageBox.Show(error, "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                    _isNew ? "Создать нового пользователя?" : "Сохранить изменения?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question
                ) != DialogResult.Yes) return;

            // Считываем поля
            var login = txtLogin.Text.Trim();
            var password = txtPassword.Text;
            var role = txtRole.Text.Trim();
            var lastName = txtLastName.Text.Trim();
            var firstName = txtFirstName.Text.Trim();
            var fatherName = txtFatherName.Text.Trim();
            var email = txtEmail.Text.Trim();
            var phone = txtPhone.Text.Trim();
            DateTime? dob = datePicker.Checked ? (DateTime?)datePicker.Value.Date : null;
            var photo = (_model.Photo != null ? _model.Photo : null);

            bool ok;
            var db = new DB();
            try
            {
                if (_isNew)
                {
                    ok = db.RegisterUser(
                        login, password, role,
                        email, lastName, firstName, fatherName,
                        dob, phone, photo
                    );
                }
                else
                {
                    ok = db.SaveUserData(
                        _model.UserId,
                        login, password, role,
                        lastName, firstName, fatherName,
                        dob, phone, email, photo
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обращении к БД:\n{ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ok)
            {
                MessageBox.Show("Не удалось сохранить данные.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Обновляем модель из БД
            _model.SetPersonalDataById(_model.UserId);

            MessageBox.Show("Данные сохранены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        #endregion

        #region — Отмена —

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}
