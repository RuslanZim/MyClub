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
    public partial class ChangeCredentialsForm : Form
    {
        public string CurrentLogin { get; set; }
        public ChangeCredentialsForm()
        {
            InitializeComponent();
        }

        bool draging = false;
        Point dragCursorPoint;
        Point dragFormPoint;

        private void ChangeCredentialsForm_Load(object sender, EventArgs e)
        {
            guna2TextBox10.Text = CurrentLogin;
        }

        private void iconButton13_Click(object sender, EventArgs e) 
        {
            this.Close();
        }


        private void iconButton12_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void guna2Button1_Click(object sender, EventArgs e) //кнопка Ок
        {
            string oldPasswordInMemory = PersonalData.Current.Password; // Текущий пароль из объекта
            string oldPasswordInput = guna2TextBox9.Text;     // старый пароль, введённый пользователем
            string newLogin = guna2TextBox10.Text;                   // Новый (или тот же) логин
            string newPassword = guna2TextBox1.Text;          // Новый пароль
            string confirmPassword = guna2TextBox2.Text;  // Подтверждение

            // 1. Проверяем старый пароль
            if (string.IsNullOrEmpty(oldPasswordInput))
            {
                MessageBox.Show("Введите текущий пароль!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (oldPasswordInput != oldPasswordInMemory)
            {
                MessageBox.Show("Текущий пароль введён неверно!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 2. Проверяем новый пароль
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Введите новый пароль и его подтверждение!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3. Обновляем в БД
            DB db = new DB();
            bool success = db.UpdateUserLoginPassword(PersonalData.Current.UserId, newLogin, newPassword);
            if (!success)
            {
                MessageBox.Show("Ошибка при обновлении логина/пароля в базе.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 4. При успехе — обновляем локальные данные
            PersonalData.Current.UpdateCredentials(newLogin, newPassword);

            // 5. Возвращаемся в форму Profile с DialogResult = OK
            this.DialogResult = DialogResult.OK;
            this.Close();
        }


        private void panel1_MouseDown_1(object sender, MouseEventArgs e)
        {
            draging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void panel1_MouseUp_1(object sender, MouseEventArgs e)
        {
            draging = false;
        }

        private void panel1_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (draging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

    }
}
