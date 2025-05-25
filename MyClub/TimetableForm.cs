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
    public partial class TimetableForm : Form
    {
        private readonly Event _model;
        private readonly bool _isNew;
        private List<PersonalData> _trainers;

        private const string ErrTitleMissing = "Введите название мероприятия.";
        private const string ErrSaveFailed = "Не удалось сохранить мероприятие.";
        public TimetableForm()
        {
            InitializeComponent();
            _isNew = true;
            Text = "Новое мероприятие";

            // привязка событий
            this.Load += TimetableForm_Load;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        public TimetableForm(Event model)
        {
            InitializeComponent();
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _isNew = false;
            Text = $"Мероприятие #{model.EventId}";

            // привязка событий
            this.Load += TimetableForm_Load;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        #region — Инициализация контролов —

        private void TimetableForm_Load(object sender, EventArgs e)
        {
            try
            {
                InitControls();
                if (!_isNew)
                    FillFromModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки формы: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                CloseToTimetable();
            }
        }

        private void InitControls()
        {
            var db = new DB();

            // 1) Тренеры
            _trainers = db.GetAllTrainers();
            comboBoxTrainer.DataSource = _trainers;
            comboBoxTrainer.DisplayMember = nameof(PersonalData.LastName);
            comboBoxTrainer.ValueMember = nameof(PersonalData.UserId);
            comboBoxTrainer.SelectedIndex = -1;

            // 2) Дата
            datePickerDate.Format = DateTimePickerFormat.Short;

            // 3) Время начала
            timePickerStart.Format = DateTimePickerFormat.Time;
            timePickerStart.ShowUpDown = true;

            // 4) Время окончания
            timePickerEnd.Format = DateTimePickerFormat.Time;
            timePickerEnd.ShowUpDown = true;
        }

        #endregion

        #region — Заполнение при редактировании —

        private void FillFromModel()
        {
            txtTitle.Text = _model.Title;
            txtDescription.Text = _model.Description;
            datePickerDate.Value = _model.Date;
            timePickerStart.Value = DateTime.Today.Add(_model.StartTime);
            if (_model.EndTime.HasValue)
                timePickerEnd.Value = DateTime.Today.Add(_model.EndTime.Value);

            txtLocation.Text = _model.Location;

            if (_model.ResponsibleId.HasValue)
                comboBoxTrainer.SelectedValue = _model.ResponsibleId.Value;
        }

        #endregion

        #region — Сохранение / Отмена —

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 1) Заголовок
            var title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show(ErrTitleMissing, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2) Дата и время
            var date = datePickerDate.Value.Date;
            var startTime = timePickerStart.Value.TimeOfDay;
            var endTime = timePickerEnd.Value.TimeOfDay;

            // 3) Место
            var location = txtLocation.Text.Trim();

            // 4) Ответственный
            int? respId = comboBoxTrainer.SelectedItem == null
                ? (int?)null
                : (int?)comboBoxTrainer.SelectedValue;

            // 5) Описание
            var desc = txtDescription.Text.Trim();

            try
            {
                var db = new DB();
                bool ok = _isNew
                    ? db.CreateEvent(title, desc, date, startTime, endTime, location, respId)
                    : db.UpdateEvent(_model.EventId, title, desc, date, startTime, endTime, location, respId);

                if (!ok)
                {
                    MessageBox.Show(ErrSaveFailed, "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            CloseToTimetable();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            CloseToTimetable();
        }

        private void CloseToTimetable()
        {
            if (this.TopLevelControl is Form1 mainForm)
            {
                this.Close();
                mainForm.OpenForm(new Timetable());
            }
        }

        #endregion
    }
}
    

