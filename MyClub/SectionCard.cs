using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;


namespace MyClub
{
    internal class SectionCard : Guna2GroupBox
    {
        private readonly int _sectionId;

        public SectionCard(Section section)
        {
            _sectionId = section.SectionId;

            // Настройки GroupBox
            Text = section.Name;
            Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            ForeColor = Color.White;
            FillColor = Color.FromArgb(28, 40, 54);
            CustomBorderColor = Color.FromArgb(67, 96, 130);
            BorderColor = Color.FromArgb(67, 96, 130);
            BorderRadius = 10;
            Size = new Size(567, 279);
            Cursor = Cursors.Hand;
            Margin = new Padding(3);

            // Вид спорта
            var lblSport = new Label
            {
                Text = $"Вид: {section.Sport}",
                AutoSize = true,
                ForeColor = ForeColor,
                BackColor = FillColor,
                Location = new Point(14, 72)
            };

            // Описание
            var txtDesc = new Guna2TextBox
            {
                DefaultText = section.Description,
                ReadOnly = true,
                Multiline = true,
                BorderColor = CustomBorderColor,
                BorderRadius = 10,
                FillColor = Color.FromArgb(23, 31, 48),
                Font = new Font("Segoe UI", 9F),
                Location = new Point(18, 116),
                Size = new Size(540, 65),
                ForeColor = ForeColor,
                HoverState = { BorderColor = Color.FromArgb(94, 148, 255) },
                DisabledState =
                {
                    BorderColor         = Color.FromArgb(208, 208, 208),
                    FillColor           = Color.FromArgb(226, 226, 226),
                    ForeColor           = Color.FromArgb(138, 138, 138),
                    PlaceholderForeColor= Color.FromArgb(138, 138, 138)
                },
                FocusedState = { BorderColor = Color.FromArgb(94, 148, 255) },
                PlaceholderText = ""
            };

            // ФИО тренера
            var lblTrainer = new Label
            {
                Text = section.TrainerName,
                AutoSize = true,
                ForeColor = ForeColor,
                BackColor = FillColor,
                Location = new Point(98, 240)
            };

            // Аватар тренера
            var imgTrainer = new Guna2CirclePictureBox
            {
                Size = new Size(60, 60),
                Location = new Point(18, 201),
                ShadowDecoration = { Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle },
                /* Image =  здесь ваша картинка или placeholder */
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = FillColor


            };

            if (section.TrainerPhoto != null && section.TrainerPhoto.Length > 0)
            {
                using (var ms = new System.IO.MemoryStream(section.TrainerPhoto))
                {
                    imgTrainer.Image = System.Drawing.Image.FromStream(ms);
                }
            }
            

            Controls.Add(imgTrainer);

            // Добавляем все контролы
            Controls.Add(txtDesc);
            Controls.Add(lblSport);
            Controls.Add(lblTrainer);
            Controls.Add(imgTrainer);

        }
    }
}

