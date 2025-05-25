using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MyClub
{
    public partial class Sections : Form
    {
        private int? _selectedSectionId;
        private bool _suppressFilter;

        public Sections()
        {
            InitializeComponent();

            // привязка событий — только здесь
            txtSportFilter.TextChanged += OnFilterChanged;
            comboBoxTrainer.SelectedIndexChanged += OnFilterChanged;
            btnCreate.Click += BtnCreate_Click;
            btnEdit.Click += BtnEdit_Click;
            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        private void Sections_Load(object sender, EventArgs e)
        {
            InitControls();
            RefreshSectionsDashboard();
        }

        #region — Инициализация  —

        private void InitControls()
        {
            // flow direction
            flowLayoutPanelSections.FlowDirection = FlowDirection.TopDown;

            // тренеры
            var trainers = new DB().GetAllTrainers();
            comboBoxTrainer.DataSource = trainers;
            comboBoxTrainer.DisplayMember = nameof(PersonalData.LastName);
            comboBoxTrainer.ValueMember = nameof(PersonalData.UserId);
            comboBoxTrainer.SelectedIndex = -1;

            // прячем Save в начале
            btnSave.Visible = false;
        }

        #endregion

        #region — Обновление панели —

        private void RefreshSectionsDashboard()
        {
            // очистка
            flowLayoutPanelSections.Controls.Clear();
            _selectedSectionId = null;

            // фильтр
            string sportFilter = string.IsNullOrWhiteSpace(txtSportFilter.Text)
                ? null
                : txtSportFilter.Text.Trim();
            int? trainerFilter = comboBoxTrainer.SelectedValue as int?;

            // загрузка из БД
            var sections = new DB().GetAllSections(sportFilter, trainerFilter)
                           ?? new List<Section>();

            // биндим карточки
            BindSectionsList(sections);
        }

        #endregion

        #region — Привязка списка —

        private void BindSectionsList(IEnumerable<Section> sections)
        {
            foreach (var s in sections)
            {
                var card = new SectionCard(s)
                {
                    BorderColor = Color.FromArgb(67, 96, 130)
                };
                card.Click += (snd, ea) =>
                {
                    // сбросить все
                    foreach (Control c in flowLayoutPanelSections.Controls)
                        if (c is Guna2GroupBox gb)
                            gb.BorderColor = Color.FromArgb(67, 96, 130);

                    // выделить и запомнить
                    card.BorderColor = Color.Orange;
                    _selectedSectionId = s.SectionId;
                };
                flowLayoutPanelSections.Controls.Add(card);
            }
        }

        #endregion

        #region — Команды (Создать/Изменить/Удалить) —

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            // чтение полей
            var sport = txtSportFilter.Text.Trim();
            var name = txtName.Text.Trim();
            var desc = txtDescription.Text.Trim();
            int? trainerId = comboBoxTrainer.SelectedValue as int?;

            // валидация
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название секции", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // создаём
            int newId = new DB().CreateSection(name, desc, trainerId, sport);
            if (newId <= 0)
            {
                MessageBox.Show("Не удалось создать секцию", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // очистка полей (фильтр оставляем)
            txtName.Clear();
            txtDescription.Clear();
            comboBoxTrainer.SelectedIndex = -1;

            RefreshSectionsDashboard();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedSectionId == null)
            {
                MessageBox.Show("Сначала выберите секцию кликом по карточке.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sec = new DB().GetSectionById(_selectedSectionId.Value);
            if (sec == null) return;

            // выключаем фильтрацию, чтобы пользователь мог править поля
            _suppressFilter = true;

            // заполняем поля
            txtSportFilter.Text = sec.Sport;
            txtName.Text = sec.Name;
            txtDescription.Text = sec.Description;
            if (sec.TrainerId.HasValue)
                comboBoxTrainer.SelectedValue = sec.TrainerId.Value;
            else
                comboBoxTrainer.SelectedIndex = -1;

            // режим редактирования
            btnSave.Visible = true;
            btnCreate.Visible = false;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_selectedSectionId == null) return;

            // чтение полей
            var sport = txtSportFilter.Text.Trim();
            var name = txtName.Text.Trim();
            var desc = txtDescription.Text.Trim();
            object val = comboBoxTrainer.SelectedValue;
            int? trainerId = (val == null || val == DBNull.Value)
                             ? (int?)null
                             : Convert.ToInt32(val);

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Название секции не может быть пустым", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            new DB().UpdateSection(_selectedSectionId.Value, name, desc, trainerId, sport);

            // выходим из режима редактирования
            _suppressFilter = false;
            _selectedSectionId = null;
            btnSave.Visible = false;
            btnCreate.Visible = true;

            RefreshSectionsDashboard();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedSectionId == null)
            {
                MessageBox.Show("Сначала выберите секцию кликом по карточке.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранную секцию?", "Подтвердите",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            new DB().DeleteSection(_selectedSectionId.Value);
            _selectedSectionId = null;
            RefreshSectionsDashboard();
        }

        #endregion

        #region — Обработчик фильтра —

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (_suppressFilter) return;
            RefreshSectionsDashboard();
        }

        #endregion
    }
}

