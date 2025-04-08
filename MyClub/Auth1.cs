using Guna.UI2.WinForms;
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
    public partial class Auth1 : Form
    {
        // Создаём Guna2ShadowForm, чтобы добавить тень вокруг формы
        private Guna2ShadowForm shadowForm;

        public Auth1()
        {
            InitializeComponent();

            // Инициализируем тень
            shadowForm = new Guna2ShadowForm();
            shadowForm.SetShadowForm(this);

            // Общие настройки формы
            this.Text = "Авторизация - Спортивный клуб";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Можно задать фон (например, светлый):
            this.BackColor = System.Drawing.Color.FromArgb(247, 247, 247);

            // Создаём эллипс для закругления краёв формы (опционально)
            Guna2Elipse formElipse = new Guna2Elipse();
            formElipse.TargetControl = this;
            formElipse.BorderRadius = 12;

            // Пример добавления Guna2Panel для шапки (header)
            var headerPanel = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                FillColor = System.Drawing.Color.FromArgb(25, 118, 210), // Primary color
            };
            this.Controls.Add(headerPanel);

            // Пример лейбла в шапке
            var headerLabel = new Label
            {
                Text = "Добро пожаловать!",
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold),
                Dock = DockStyle.Left,
                Padding = new Padding(10, 10, 0, 0)
            };
            headerPanel.Controls.Add(headerLabel);

            // Пример Guna2Button (кнопка "Войти")
            var btnLogin = new Guna2Button
            {
                Text = "Войти",
                Size = new System.Drawing.Size(120, 40),
                Location = new System.Drawing.Point(100, 150),
                FillColor = System.Drawing.Color.FromArgb(25, 118, 210),
                BorderRadius = 6,
            };
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            // Пример Guna2TextBox (поле ввода логина)
            var txtLogin = new Guna2TextBox
            {
                PlaceholderText = "Логин",
                Size = new System.Drawing.Size(200, 36),
                Location = new System.Drawing.Point(100, 80),
                BorderRadius = 6,
                BorderColor = System.Drawing.Color.Gray
            };
            this.Controls.Add(txtLogin);

            // Пример Guna2TextBox (поле ввода пароля)
            var txtPassword = new Guna2TextBox
            {
                PlaceholderText = "Пароль",
                Size = new System.Drawing.Size(200, 36),
                Location = new System.Drawing.Point(100, 120),
                BorderRadius = 6,
                BorderColor = System.Drawing.Color.Gray,
                PasswordChar = '•'
            };
            this.Controls.Add(txtPassword);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            // Логика проверки логина/пароля и переход в главное окно
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }
    }
}
