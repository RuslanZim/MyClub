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
    public partial class Sections : Form
    {
        public Sections()
        {
            InitializeComponent();

            // Настраиваем flowLayoutPanel внутри guna2GroupBox1
            flowLayoutPanelSections.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanelSections.WrapContents = false;
            flowLayoutPanelSections.AutoScroll = true;
            flowLayoutPanelSections.Dock = DockStyle.Fill;
            flowLayoutPanelSections.BackColor = Color.White;
            flowLayoutPanelSections.BorderStyle = BorderStyle.None;
            flowLayoutPanelSections.Padding = new Padding(10);
            flowLayoutPanelSections.Margin = new Padding(0);

            guna2TextBox1.TextChanged += (s, e) => LoadSections();
        }

        private void Sections_Load(object sender, EventArgs e)
        {
            // Заполняем тренеров
            var trainers = db.GetAllTrainers(); // List<PersonalData>
            guna2ComboBox2.DataSource = trainers;
            guna2ComboBox2.DisplayMember = "LastName";
            guna2ComboBox2.ValueMember = "UserId";
            guna2ComboBox2.SelectedIndex = -1;

            LoadSections();
        }
        private void LoadSections()
        {
            flowLayoutPanelSections.Controls.Clear();
            var db = new DB();
            // Читаем текст вида спорта
            string filterSport = guna2TextBox1.Text.Trim();
            if (string.IsNullOrEmpty(filterSport)) filterSport = null;

            var sections = db.GetAllSections(filterSport);

            foreach (var s in sections)
            {
                var card = new SectionCard(s);
                card.EditClicked += id => OpenEdit(id);
                card.DeleteClicked += id => {
                    db.DeleteSection(id);
                    LoadSections();
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
            string sport = guna2TextBox1.Text.Trim();
            string name = guna2TextBox9.Text.Trim();
            string description = guna2TextBox7.Text.Trim();
            int? trainerId = guna2ComboBox2.SelectedItem is PersonalData pd
                                    ? pd.UserId
                                    : (int?)null;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название секции", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var db = new DB();
            int newId = db.CreateSection(name, description, trainerId, sport);
            // если у вас сигнатура CreateSection без параметра sport, добавьте его:
            // public int CreateSection(string name, string desc, int? trainerId, string sport)
            // и сохраняйте в БД соответствующее поле.

            if (newId <= 0)
            {
                MessageBox.Show("Не удалось создать секцию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            guna2TextBox9.Clear();
            guna2TextBox7.Clear();
            // оставляем вид спорта для быстрого создания похожих
            guna2ComboBox2.SelectedIndex = -1;

            LoadSections();
        }

        private void guna2Button2_Click(object sender, EventArgs e)//изменить
        {
            using (var dlg = new SectionEditForm(sectionId))
                if (dlg.ShowDialog() == DialogResult.OK)
                    LoadSections(); 
        }

        private void guna2Button3_Click(object sender, EventArgs e)//удалить
        {

        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)//вид спорта
        {

        }

        
    }
}
