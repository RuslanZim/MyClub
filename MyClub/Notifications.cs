using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace MyClub
{
    public partial class Notifications : Form
    {
        private int? _selectedNotificationId;

        public Notifications()
        {
            InitializeComponent();

            // привязка событий
            btnNew.Click += BtnNew_Click;
            btnEdit.Click += BtnEdit_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        private void Notifications_Load(object sender, EventArgs e)
        {
            RefreshNotifications();
        }

        #region — Обновление списка —

        private void RefreshNotifications()
        {
            pnlContainer.Controls.Clear();
            _selectedNotificationId = null;

            var db = new DB();
            var notifications = db.GetAllNotifications() ?? new List<Notification>();

            foreach (var n in notifications)
            {
                // получить автора
                var author = new PersonalData();
                author.SetPersonalDataById(n.AuthorUserId);

                var card = new NotificationCard(n, author)
                {
                    BorderColor = Color.FromArgb(67, 96, 130)
                };

                card.Click += (s, ev) =>
                {
                    // сбросить все
                    foreach (Control c in pnlContainer.Controls)
                        if (c is Guna2GroupBox gb)
                            gb.BorderColor = Color.FromArgb(67, 96, 130);

                    // выделить и запомнить
                    card.BorderColor = Color.Orange;
                    _selectedNotificationId = n.NotificationId;
                };

                pnlContainer.Controls.Add(card);
            }
        }

        #endregion

        #region — Команды Создать/Изменить/Обновить/Удалить —

        private void BtnNew_Click(object sender, EventArgs e)
        {
            using (var form = new NotificationForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                    RefreshNotifications();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedNotificationId == null)
            {
                MessageBox.Show("Сначала выберите уведомление кликом по карточке.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var db = new DB();
            var notification = db.GetNotificationById(_selectedNotificationId.Value);
            if (notification == null) return;

            using (var form = new NotificationForm(notification))
            {
                if (form.ShowDialog() == DialogResult.OK)
                    RefreshNotifications();
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            RefreshNotifications();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedNotificationId == null)
            {
                MessageBox.Show("Сначала выберите уведомление кликом по карточке.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранное уведомление?", "Подтвердите",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            new DB().DeleteNotification(_selectedNotificationId.Value);
            RefreshNotifications();
        }

        #endregion


    }
}
