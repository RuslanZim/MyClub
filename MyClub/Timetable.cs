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
using static Guna.UI2.WinForms.Suite.Descriptions;
using static System.Net.Mime.MediaTypeNames;

namespace MyClub
{
    public partial class Timetable : Form
    {

        private int? _selectedEventId;
        private bool _suppressFilter;
        private FlowLayoutPanel _flow;
        public Timetable()
        {
            InitializeComponent();

            // привязка событий — только здесь
            this.Load += Timetable_Load;
            txtName.SelectedIndexChanged += OnFilterChanged;
            comboBoxTrainer.SelectedIndexChanged += OnFilterChanged;
            txtSportFilter.SelectedIndexChanged += OnFilterChanged;
            cmbLocation.SelectedIndexChanged += OnFilterChanged;

            btnNew.Click += BtnNew_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnUpdate.Click += (s, e) => RefreshTimetableDashboard();
        }


        #region — Инициализация контролов —

        private void Timetable_Load(object sender, EventArgs e)
        {
            InitControls();
            RefreshTimetableDashboard();
        }

        private void InitControls()
        {
            var db = new DB();

            // чтобы отключить срабатывание OnFilterChanged при установке SelectedIndex
            _suppressFilter = true;

            // 1) Названия секций
            var sections = db.GetAllSections(null, null)
                             .Select(s => s.Name)
                             .Distinct()
                             .ToList();
            sections.Insert(0, "Все");
            txtName.Items.Clear();
            txtName.Items.AddRange(sections.ToArray());
            txtName.SelectedIndex = 0;

            // 2) Тренеры
            var trainers = db.GetAllTrainers();
            trainers.Insert(0, new PersonalData { UserId = 0, LastName = "Все" });
            comboBoxTrainer.DataSource = null;
            comboBoxTrainer.DataSource = trainers;
            comboBoxTrainer.DisplayMember = nameof(PersonalData.LastName);
            comboBoxTrainer.ValueMember = nameof(PersonalData.UserId);
            comboBoxTrainer.SelectedIndex = 0;

            // 3) Виды спорта
            var sports = db.GetAllSections(null, null)
                           .Select(s => s.Sport)
                           .Distinct()
                           .ToList();
            sports.Insert(0, "Все");
            txtSportFilter.Items.Clear();
            txtSportFilter.Items.AddRange(sports.ToArray());
            txtSportFilter.SelectedIndex = 0;

            // 4) Места из Events
            var locations = db.GetDistinctEventLocations().ToList();
            locations.Insert(0, "Все");
            cmbLocation.Items.Clear();
            cmbLocation.Items.AddRange(locations.ToArray());
            cmbLocation.SelectedIndex = 0;


            // 5) Фильтр по дате
            datePickerDate.Format = DateTimePickerFormat.Short;
            datePickerDate.Value = DateTime.Today;

            // 6) Создаём один FlowLayoutPanel внутри guna2Panel1
            _flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
            };
            // **НИКОГДА больше не трогаем guna2Panel1.Controls.Clear()**
            guna2Panel1.Controls.Clear();
            guna2Panel1.Controls.Add(_flow);

            _suppressFilter = false;
        }

        #endregion

        #region — Обновление расписания —

        private void RefreshTimetableDashboard()
        {
            // очищаем только карточки, а не весь контейнер
            _flow.Controls.Clear();
            _selectedEventId = null;

            // читаем фильтры и переводим "Все" → null
            string sectionFilter = txtName.SelectedItem as string;
            if (sectionFilter == "Все") sectionFilter = null;

            int? trainerFilter = comboBoxTrainer.SelectedValue as int?;
            if (trainerFilter == 0) trainerFilter = null;

            string sportFilter = txtSportFilter.SelectedItem as string;
            if (sportFilter == "Все") sportFilter = null;

            string locationFilter = cmbLocation.SelectedItem as string;
            if (locationFilter == "Все") locationFilter = null;

            DateTime? datatime = datePickerDate.Value.Date;

            // получаем и сортируем
            var entries = new DB()
                .GetEvents(sectionFilter, trainerFilter, sportFilter, locationFilter, datatime)
                ?? new List<Event>();
            var sorted = entries
                .OrderBy(ev => ev.Date)
                .ThenBy(ev => ev.StartTime)
                .ToList();

            BindTimetableList(sorted);
        }


        #endregion

        #region — Привязка списка —

        private void BindTimetableList(IEnumerable<Event> entries)
        {
            _flow.Controls.Clear();
            foreach (var ev in entries)
            {
                var card = new TimetableCard(ev)
                {
                    AutoSize = false,
                    Size = new Size(250, 200),   // ширина=250, высота=150 — подберите под себя
                    Margin = new Padding(8),
                };
                card.Click += (snd, args) => {
                    // сбросить выделение у всех
                    foreach (var c in _flow.Controls.OfType<TimetableCard>())
                        c.SetSelected(false);
                    card.SetSelected(true);
                    _selectedEventId = ev.EventId;
                };
                _flow.Controls.Add(card);
            }
        }

        #endregion

        #region — Команды (Создать/Изменить/Удалить) —

        private void BtnNew_Click(object sender, EventArgs e)
        {
            using (var dlg = new TimetableForm())
            {
                if (this.TopLevelControl is Form1 mainForm)
                {
                    mainForm.OpenForm(new TimetableForm());
                }
            }

        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedEventId == null)
            {
                MessageBox.Show("Сначала выберите запись кликом по карточке.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var entry = new DB().GetEventById(_selectedEventId.Value);
            if (entry == null) return;

            using (var dlg = new TimetableForm(entry))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    RefreshTimetableDashboard();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedEventId == null)
            {
                MessageBox.Show("Выберите мероприятие из списка.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Удалить выбранное мероприятие?", "Подтвердите",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                if (new DB().DeleteEvent(_selectedEventId.Value))
                    RefreshTimetableDashboard();
                else
                    MessageBox.Show("Не удалось удалить запись.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region — Обработчик фильтра —

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (_suppressFilter) return;
            RefreshTimetableDashboard();
        }

        #endregion
    }

    // Пользовательский контрол для одной карточки расписания
    public class TimetableCard : Guna2GroupBox
    {
        public int EntryId { get; }
        private readonly Color _defaultBorder = Color.FromArgb(200, 200, 200);
        private readonly Color _selectedBorder = Color.Orange;

        public TimetableCard(Event ev)
        {
            EntryId = ev.EventId;

            // Панель-рамка
            this.BorderRadius = 8;
            this.BorderThickness = 2;
            this.BorderColor = _defaultBorder;
            this.FillColor = Color.White;
            this.Padding = new Padding(12);
            this.Margin = new Padding(8);
            this.AutoSize = true;


            // Таблица внутри карточки
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 4,
            };
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // 1) Заголовок
            var lblTitle = new Label
            {
                Text = ev.Title,
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(49, 69, 96),
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 6)
            };
            table.Controls.Add(lblTitle, 0, 0);

            // 2) Время
            var end = ev.EndTime ?? ev.StartTime;
            var lblWhen = new Label
            {
                Text = $"{ev.Date:dd.MM.yyyy}, {ev.StartTime:hh\\:mm}–{end:hh\\:mm}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 6)
            };
            table.Controls.Add(lblWhen, 0, 1);

            // 3) Место + ответственный
            var pnlInfo = new FlowLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
            };
            var lblWhere = new Label
            {
                Text = $"📍 {ev.Location}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(67, 96, 130),
                AutoSize = true,
            };
            var lblWho = new Label
            {
                Text = $"👤 {ev.ResponsibleName}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(67, 96, 130),
                AutoSize = true,
                Margin = new Padding(16, 0, 0, 0)
            };
            pnlInfo.Controls.Add(lblWhere);
            pnlInfo.Controls.Add(lblWho);
            table.Controls.Add(pnlInfo, 0, 2);

            // 4) Описание
            if (!string.IsNullOrWhiteSpace(ev.Description))
            {
                var lblDesc = new Label
                {
                    Text = ev.Description,
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.DarkSlateGray,
                    AutoSize = true,
                    MaximumSize = new Size(0, 0),
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0, 6, 0, 0)
                };
                table.Controls.Add(lblDesc, 0, 3);
            }

            this.Controls.Add(table);
        }

        /// <summary>
        /// Вызывайте при клике, чтобы показать выделение.
        /// </summary>
        public void SetSelected(bool selected)
        {
            BorderColor = selected ? _selectedBorder : _defaultBorder;
        }
    }
}


