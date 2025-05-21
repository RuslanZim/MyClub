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
    public partial class SubscriptionForm : Form
    {

        private readonly UserSubscription _model;
        private readonly bool _isNew;
        private List<PersonalData> _users;
        private List<SubscriptionType> _subTypes;

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
            // 1) Пользователи для автокомплита
            _users = PersonalData.GetAllUsers();
            var names = _users.Select(u => $"{u.LastName} {u.FirstName}").ToArray();
            var ac = new AutoCompleteStringCollection();
            ac.AddRange(names);
            comboUser.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboUser.AutoCompleteSource = AutoCompleteSource.CustomSource;
            comboUser.AutoCompleteCustomSource = ac;
            comboUser.PlaceholderText = "Начните вводить ФИО...";

            // 2) Типы подписок
            _subTypes = new DB().GetSubscriptionTypes();
            comboOperation.DataSource = _subTypes;
            comboOperation.DisplayMember = nameof(SubscriptionType.Name);
            comboOperation.ValueMember = nameof(SubscriptionType.SubscriptionTypeId);
            comboOperation.SelectedIndex = -1;

            if (!_isNew)
                FillFromModel();
        }

        private void FillFromModel()
        {
            // ID подписки (только для показа)
            subscriptionId.Text = _model.UserSubscriptionId.ToString();

            // Пользователь
            var pd = _users.FirstOrDefault(u => u.UserId == _model.UserId);
            comboUser.Text = pd == null
                ? ""
                : $"{pd.LastName} {pd.FirstName}";

            // Тип подписки
            comboOperation.SelectedValue = _model.SubscriptionTypeId;

            // Даты
            guna2DateTimePicker1.Value = _model.StartDate;
            datePicker.Value = _model.EndDate;

            // Автопродление
            guna2CheckBoxIsActive.Checked = _model.IsActive;
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            // Валидация пользователя
            var fullName = comboUser.Text.Trim();
            var user = _users.FirstOrDefault(u =>
                $"{u.LastName} {u.FirstName}"
                .Equals(fullName, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден. Выберите из подсказок.",
                                "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Валидация типа подписки
            if (comboOperation.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип подписки.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int typeId = (int)comboOperation.SelectedValue;

            // Даты
            var start = guna2DateTimePicker1.Value.Date;
            var end = datePicker.Value.Date;
            if (end < start)
            {
                MessageBox.Show("Дата окончания должна быть не раньше даты начала.","Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Автопродление
            bool autoRenew = guna2CheckBoxIsActive.Checked;

            // Сохраняем
            var db = new DB();
            bool ok = _isNew
                ? db.CreateUserSubscription(user.UserId, typeId, start, end, autoRenew)
                : db.UpdateUserSubscription(_model.UserSubscriptionId, start, end, autoRenew);

            if (!ok)
            {
                MessageBox.Show("Не удалось сохранить подписку.", "Ошибка",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;

            // Возвращаемся в финансовую панель
            if (TopLevelControl is Form1 mainForm)
            {
                Close();
                mainForm.OpenForm(new Finance());
            }
            else
            {
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Отмена — просто закрыть и вернуться
            if (TopLevelControl is Form1 mainForm)
            {
                Close();
                mainForm.OpenForm(new Finance());
            }
            else
            {
                Close();
            }
        }

        private void comboOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(_isNew && comboOperation.SelectedItem is SubscriptionType st)
            {
                var today = DateTime.Today;
                guna2DateTimePicker1.Value = today;
                datePicker.Value = today.AddDays(st.DurationDays);
            }
        }
    }
}
