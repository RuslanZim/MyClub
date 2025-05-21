using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyClub
{
    public partial class SubscriptionForm : Form
    {
        // Текущий объект и флаг нового
        private readonly UserSubscription _model;
        private readonly bool _isNew;

        // Кэшированные данные
        private List<PersonalData> _users;
        private List<SubscriptionType> _subTypes;

        // Тексты сообщений
        private const string ErrUserNotFound = "Пользователь не найден. Выберите из подсказок.";
        private const string ErrSelectType = "Выберите тип подписки.";
        private const string ErrDateMismatch = "Дата окончания должна быть не раньше даты начала.";
        private const string ErrSaveFailed = "Не удалось сохранить подписку.";

        public SubscriptionForm()
        {
            InitializeComponent();
            _isNew = true;
            Text = "Новая подписка";
        }

        public SubscriptionForm(UserSubscription model)
        {
            InitializeComponent();
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _isNew = false;
            Text = $"Подписка #{model.UserSubscriptionId}";
        }

        private void SubscriptionForm_Load(object sender, EventArgs e)
        {
            try
            {
                // 1) Загружаем пользователей и настраиваем автокомплит
                _users = PersonalData.GetAllUsers();
                var names = _users.Select(u => $"{u.LastName} {u.FirstName}").ToArray();
                var ac = new AutoCompleteStringCollection();
                ac.AddRange(names);
                comboUser.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboUser.AutoCompleteSource = AutoCompleteSource.CustomSource;
                comboUser.AutoCompleteCustomSource = ac;
                comboUser.PlaceholderText = "Начните вводить ФИО...";

                // 2) Загружаем типы подписок
                _subTypes = new DB().GetSubscriptionTypes() ?? new List<SubscriptionType>();
                comboOperation.DataSource = _subTypes;
                comboOperation.DisplayMember = nameof(SubscriptionType.Name);
                comboOperation.ValueMember = nameof(SubscriptionType.SubscriptionTypeId);
                comboOperation.SelectedIndex = -1;

                // 3) Если редактирование — заполняем поля
                if (!_isNew) FillFromModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке данных: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error
                );
                CloseToFinance();
            }
        }

        private void FillFromModel()
        {
            subscriptionId.Text = _model.UserSubscriptionId.ToString();

            var pd = _users.FirstOrDefault(u => u.UserId == _model.UserId);
            comboUser.Text = pd == null
                ? ""
                : $"{pd.LastName} {pd.FirstName}";

            comboOperation.SelectedValue = _model.SubscriptionTypeId;
            guna2DateTimePicker1.Value = _model.StartDate;
            datePicker.Value = _model.EndDate;
            guna2CheckBoxIsActive.Checked = _model.IsActive;
        }

        private void comboOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            // При создании — автозаполняем даты
            if (_isNew && comboOperation.SelectedItem is SubscriptionType st)
            {
                var today = DateTime.Today;
                guna2DateTimePicker1.Value = today;
                datePicker.Value = today.AddDays(st.DurationDays);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1) Валидируем пользователя
            var fullName = comboUser.Text.Trim();
            var user = _users.FirstOrDefault(u =>
                $"{u.LastName} {u.FirstName}"
                .Equals(fullName, StringComparison.OrdinalIgnoreCase)
            );
            if (user == null)
            {
                MessageBox.Show(ErrUserNotFound, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2) Валидируем тип подписки
            if (comboOperation.SelectedItem == null)
            {
                MessageBox.Show(ErrSelectType, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int typeId = (int)comboOperation.SelectedValue;

            // 3) Валидируем даты
            var start = guna2DateTimePicker1.Value.Date;
            var end = datePicker.Value.Date;
            if (end < start)
            {
                MessageBox.Show(ErrDateMismatch, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool autoRenew = guna2CheckBoxIsActive.Checked;

            // 4) Сохраняем
            try
            {
                var db = new DB();
                bool ok = _isNew
                    ? db.CreateUserSubscription(user.UserId, typeId, start, end, autoRenew)
                    : db.UpdateUserSubscription(_model.UserSubscriptionId, start, end, autoRenew);

                if (!ok)
                {
                    MessageBox.Show(ErrSaveFailed, "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при сохранении: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error
                );
                return;
            }

            DialogResult = DialogResult.OK;
            CloseToFinance();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CloseToFinance();
        }

        /// <summary>
        /// Закрываем эту форму и открываем финансовый дашборд.
        /// </summary>
        private void CloseToFinance()
        {
            if (this.TopLevelControl is Form1 mainForm)
            {
                this.Close();
                mainForm.OpenForm(new Finance());
            }
        }
    }
}
