using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;         
using System.Drawing;    
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;



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
            _model = new PersonalData();  // или оставляем null и обрабатываем отдельно
            _isNew = true;
            Text = "Новый пользователь";
            guna2Button2.Text = "Создать";    // кнопка «Сохранить» превращается в «Создать»
        }

        private void AllProfile_Load(object sender, EventArgs e)
        {
            if (!_isNew)
                LoadPersonalData();
        }

        private void LoadPersonalData()
        {
            // Получаем данные текущего пользователя из глобального свойства
            if (_model == null)
            {
                MessageBox.Show("Данные пользователя не загружены!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            guna2TextBox10.Text = _model.Login;
            guna2TextBox9.Text = _model.Password;
            guna2TextBox7.Text = _model.Role;
            guna2TextBox2.Text = _model.LastName;
            guna2TextBox1.Text = _model.FirstName;
            guna2TextBox3.Text = _model.FatherName;
            guna2TextBox6.Text = _model.DateBirth.HasValue
                ? _model.DateBirth.Value.ToString("yyyy-MM-dd")
                : "";
            guna2TextBox5.Text = _model.PhoneNumber;
            guna2TextBox4.Text = _model.Email;

            // Загрузка фото
            // Освободим предыдущую картинку
            pictureBox2.Image?.Dispose();

            if (_model.Photo != null && _model.Photo.Length > 0)
            {
                // Считаем поток и сразу клонируем в независимый Bitmap
                using (var ms = new MemoryStream(_model.Photo))
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
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(newLogin)) missing.Add("Логин");
            if (string.IsNullOrWhiteSpace(newPassword)) missing.Add("Пароль");
            if (string.IsNullOrWhiteSpace(newRole)) missing.Add("Роль");
            if (string.IsNullOrWhiteSpace(newLastName)) missing.Add("Фамилия");
            if (string.IsNullOrWhiteSpace(newFirstName)) missing.Add("Имя");

            if (missing.Count > 0)
            {
                MessageBox.Show(
                    "Не заполнены обязательные поля:\n" + string.Join(", ", missing),
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Подтверждение
            if (MessageBox.Show(
                    _isNew
                       ? "Создать нового пользователя?"
                       : "Сохранить все изменения?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                ) != DialogResult.Yes)
                return;


            var db = new DB();
            bool ok = false;

            if (_isNew)
            {
                 ok = db.RegisterUser(
                    newLogin, newPassword, newRole, 
                    newEmail, newLastName, newFirstName,
                    newFatherName, newDOB, newPhone, _model.Photo
                );
            }
            else
            {
                 ok = db.SaveUserData(
                     _model.UserId,
                    newLogin, newPassword, newRole,
                    newLastName, newFirstName, newFatherName,
                    newDOB, newPhone, newEmail, _model.Photo
                );
            }
            if (!ok)
            {
                MessageBox.Show("Ошибка при сохранении данных!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Перезагрузка текущей модели
            _model.SetPersonalDataById(_model.UserId);

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
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
                    if (dlg.ShowDialog() != DialogResult.OK) return;

                    byte[] fileData = File.ReadAllBytes(dlg.FileName);

                    if (_isNew)
                    {
                        _model.UpdatePhoto(fileData);
                        using (var ms = new MemoryStream(fileData))
                            pictureBox2.Image = Image.FromStream(ms);

                        MessageBox.Show("Фото загружено. Оно будет сохранено при создании пользователя.",
                                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // для уже существующего пользователя — обновляем сразу в БД
                        var db = new DB();
                        if (db.UpdateUserPhoto(_model.UserId, fileData))
                        {
                            _model.UpdatePhoto(fileData);
                            using (var ms = new MemoryStream(fileData))
                                pictureBox2.Image = Image.FromStream(ms);

                            MessageBox.Show("Фото успешно обновлено!",
                                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при обновлении фото.",
                                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}
