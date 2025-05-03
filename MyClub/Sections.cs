using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Forms;

namespace MyClub
{
    public partial class Sections : Form
    {
        private int? _selectedSectionId = null;
        private bool _suppressFilter = false;

        public Sections()
        {
            InitializeComponent();
            flowLayoutPanelSections.FlowDirection = FlowDirection.TopDown;
            guna2TextBox1.TextChanged += FilterTextChanged;



        }

        private void Sections_Load(object sender, EventArgs e)
        {
             // Заполнить список тренеров
            var trainers = new DB().GetAllTrainers();
            guna2ComboBox2.DataSource = trainers;
            guna2ComboBox2.DisplayMember = "LastName";
            guna2ComboBox2.ValueMember = "UserId";
            guna2ComboBox2.SelectedIndex = -1;

            // Спрятать кнопку «Сохранить»
            guna2Button4.Visible = false;

            LoadSections();
        }

        private void LoadSections()
        {
            flowLayoutPanelSections.Controls.Clear();
            _selectedSectionId = null;

            var db = new DB();
            string sport = string.IsNullOrWhiteSpace(guna2TextBox1.Text)
                           ? null
                           : guna2TextBox1.Text.Trim();
            var sections = db.GetAllSections(sport);
            foreach (var s in sections)
            {
                var card = new SectionCard(s)
                {
                    BorderColor = Color.FromArgb(67, 96, 130)
                };
                card.Click += (sender, e) =>
                {
                    // Сброс рамок у всех карточек
                    foreach (Control c in flowLayoutPanelSections.Controls)
                        if (c is Guna2GroupBox gb)
                            gb.BorderColor = Color.FromArgb(67, 96, 130);

                    // Подсветим выбранную
                    card.BorderColor = Color.Orange;
                    _selectedSectionId = s.SectionId;
                };
                flowLayoutPanelSections.Controls.Add(card);
            }
        }


        private void flowLayoutPanelSections_Paint(object sender, PaintEventArgs e)//flowLayoutPanelSections
        {

        }


        private void guna2TextBox9_TextChanged(object sender, EventArgs e)//название
        {

        }

        private void guna2TextBox7_TextChanged(object sender, EventArgs e)//описание
        {

        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)//тренер
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)//создать
        {
            // Собираем поля
            var sport = guna2TextBox1.Text.Trim();
            var name = guna2TextBox9.Text.Trim();
            var desc = guna2TextBox7.Text.Trim();

            // 2. Корректное чтение тренера:
            var selectedTrainer = guna2ComboBox2.SelectedItem as PersonalData;
            int? trainerId = selectedTrainer?.UserId;


            MessageBox.Show(
              selectedTrainer == null
                ? "Никакой тренер не выбран"
                : $"Выбран тренер: {selectedTrainer.LastName} (ID={selectedTrainer.UserId})"
            );


            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название секции", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var db = new DB();
            int newId = db.CreateSection(name, desc, trainerId, sport);
            if (newId <= 0)
            {
                MessageBox.Show("Не удалось создать секцию", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Очистка полей (поля вида спорта оставляем)
            guna2TextBox9.Clear();
            guna2TextBox7.Clear();
            guna2ComboBox2.SelectedIndex = -1;

            LoadSections();
        }

        private void guna2Button2_Click(object sender, EventArgs e)//изменить
        {
            if (_selectedSectionId == null)
            {
                MessageBox.Show("Сначала выберите секцию кликом по карточке.",
                                "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            // Вытаскиваем из БД
            var db = new DB();
            var sec = db.GetSectionById(_selectedSectionId.Value);
            if (sec == null) return;

            // временно отписываем фильтр
            guna2TextBox1.TextChanged -= FilterTextChanged;

            // Заполняем поля (те же, что для создания)
            guna2TextBox1.Text = sec.Sport;
            guna2TextBox9.Text = sec.Name;
            guna2TextBox7.Text = sec.Description;
            guna2ComboBox2.SelectedValue = sec.TrainerId;

            if (sec.TrainerId.HasValue)
                guna2ComboBox2.SelectedValue = sec.TrainerId.Value;
            else
                guna2ComboBox2.SelectedIndex = -1;

            // Показываем кнопку Сохранить, прячем Создать
            guna2Button4.Visible = true;
            guna2Button1.Visible = false;
        }

        // Обработчик фильтра, вынесенный в именованный метод:
        private void FilterTextChanged(object s, EventArgs e)
        {
            if (_suppressFilter) return;   // если мы в режиме редактирования — не фильтруем
            LoadSections();
        }

        private void guna2Button3_Click(object sender, EventArgs e)//удалить
        {
            if (_selectedSectionId == null)
            {
                MessageBox.Show("Сначала выберите секцию кликом по карточке.",
                                "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранную секцию?", "Подтвердите",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            new DB().DeleteSection(_selectedSectionId.Value);
            _selectedSectionId = null;
            LoadSections();
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)//вид спорта
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e) //Сохранить
        {
            if (_selectedSectionId == null) return;

            var sport = guna2TextBox1.Text.Trim();
            var name = guna2TextBox9.Text.Trim();
            var desc = guna2TextBox7.Text.Trim();
            object val = guna2ComboBox2.SelectedValue;
            int? trainerId = (val == null || val == DBNull.Value)
                             ? (int?)null
                             : Convert.ToInt32(val);

            // Логика валидации (необязательно)
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Название секции не может быть пустым", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            new DB().UpdateSection(_selectedSectionId.Value, name, desc, trainerId, sport);

            // Выходим из режима редактирования
            _suppressFilter = false;
            _selectedSectionId = null;

            // Вернуть кнопки в исходное состояние
            guna2Button4.Visible = false;
            guna2Button1.Visible = true;
            _selectedSectionId = null;

            LoadSections();
        }
    }
}
