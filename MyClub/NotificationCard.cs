using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace MyClub
{
    internal class NotificationCard : Guna2GroupBox
    {
        private readonly int _notificationId;

        public NotificationCard(Notification notification, PersonalData author)
        {
            _notificationId = notification.NotificationId;

            // Сам контейнер-карточка
            Name = "NotificationCard";
            Text = "";
            Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            ForeColor = Color.White;
            FillColor = Color.FromArgb(28, 40, 54);
            CustomBorderColor = BorderColor = Color.FromArgb(67, 96, 130);
            BorderRadius = 10;
            Size = new Size(894, 279);
            Cursor = Cursors.Hand;
            Margin = new Padding(0, 0, 0, 10); // между карточками

            // Заголовок уведомления
            var NotificationTitle = new Label
            {
                Name = "NotificationTitle",
                Text = notification.Title,
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ForeColor,
                BackColor = FillColor,
                Location = new Point(14, 55)
            };

            // Тело уведомления
            var txtBody = new Guna2TextBox
            {
                Name = "txtBody",
                DefaultText = notification.Body,
                ReadOnly = true,
                Multiline = true,
                AcceptsReturn = true,
                WordWrap = true,
                ScrollBars = ScrollBars.Vertical,
                BorderColor = BorderColor,
                BorderRadius = 10,
                FillColor = Color.FromArgb(23, 31, 48),
                BackColor = Color.FromArgb(28, 40, 54),
                Font = new Font("Segoe UI", 9F),
                Location = new Point(18, 96),   
                Size = new Size(863, 99),
                ForeColor = ForeColor
            };

            txtBody.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtBody.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);

            // Подпись "Дата создания"
            var dateLabel = new Label
            {
                Name = "date",
                Text = "Дата создания",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.75F, FontStyle.Bold),
                ForeColor = ForeColor,
                BackColor = FillColor,
                Location = new Point(578, 205)
            };

            // Сам селектор даты
            var datePicker = new Guna2DateTimePicker
            {
                Name = "guna2DateTimePicker",
                Format = DateTimePickerFormat.Long,
                Value = notification.CreatedAt,
                Font = new Font("Segoe UI", 9F),
                FillColor = Color.FromArgb(30, 42, 58),
                Location = new Point(581, 224),
                Size = new Size(300, 37)

            };

            // Аватар автора
            var picAvatar = new Guna2CirclePictureBox
            {
                Name = "picAvatar",
                Size = new Size(60, 60),
                Location = new Point(18, 201),
                BackColor = FillColor,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            picAvatar.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            if (author.Photo != null && author.Photo.Length > 0)
            {
                using (var ms = new MemoryStream(author.Photo))
                    picAvatar.Image = Image.FromStream(ms);
            }

            // ФИО автора
            var lblAuthor = new Label
            {
                Name = "lblAuthor",
                Text = $"{author.LastName} {author.FirstName.Substring(0, 1)}.",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ForeColor,
                BackColor = FillColor,
                Location = new Point(98, 218)
            };

            // Добавляем всё в карточку
            Controls.Add(NotificationTitle);
            Controls.Add(txtBody);
            Controls.Add(dateLabel);
            Controls.Add(datePicker);
            Controls.Add(picAvatar);
            Controls.Add(lblAuthor);
        }
    }
}
