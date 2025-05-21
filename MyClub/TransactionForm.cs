using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MyClub
{
    public partial class TransactionForm : Form
    {
        // Модель и флаг новой транзакции
        private readonly Transaction _model;
        private readonly bool _isNew;
        // Кэш пользователей
        private List<PersonalData> _users;

        // Тексты сообщений
        private const string ErrSelectType = "Выберите тип операции.";
        private const string ErrInvalidAmount = "Введите корректную сумму.";
        private const string ErrUserNotFound = "Пользователь не найден. Выберите из подсказок или оставьте поле пустым.";
        private const string ErrSaveFailed = "Не удалось сохранить транзакцию.";

        public TransactionForm()
        {
            InitializeComponent();
            _isNew = true;
            Text = "Новая транзакция";
        }

        public TransactionForm(Transaction model)
        {
            InitializeComponent();
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _isNew = false;
            Text = $"Транзакция #{model.TransactionId}";
        }

        private void TransactionForm_Load(object sender, EventArgs e)
        {
            try
            {
                // 1) Настройка типа операции
                comboOperation.Items.Clear();
                comboOperation.Items.AddRange(new[] { "Доход", "Расход" });

                // 2) Загружаем пользователей и настраиваем автокомплит
                _users = PersonalData.GetAllUsers();
                var names = _users
                    .Select(u => $"{u.LastName} {u.FirstName}")
                    .ToArray();

                var ac = new AutoCompleteStringCollection();
                ac.AddRange(names);
                comboUser.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboUser.AutoCompleteSource = AutoCompleteSource.CustomSource;
                comboUser.AutoCompleteCustomSource = ac;
                comboUser.PlaceholderText = "Начните вводить ФИО...";

                // 3) Если редактирование — заполняем поля
                if (!_isNew)
                    FillFromModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке формы: {ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error
                );
                CloseToFinance();
            }
        }

        private void FillFromModel()
        {
            // Дата
            datePicker.Value = _model.Date;
            // Тип операции
            comboOperation.SelectedItem = _model.OperationType;
            // Сумма
            numAmount.Text = _model.Amount.ToString("F2");
            // Комментарий
            txtComment.Text = _model.Comment;
            // Пользователь
            var pd = _users.FirstOrDefault(u => u.UserId == _model.UserId);
            comboUser.Text = pd != null
                ? $"{pd.LastName} {pd.FirstName}"
                : string.Empty;
            // Показ ID транзакции
            transactionId.Text = _model.TransactionId.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1) Тип операции
            if (comboOperation.SelectedItem == null)
            {
                MessageBox.Show(ErrSelectType, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string type = comboOperation.SelectedItem.ToString();

            // 2) Сумма
            decimal amount;
            if (!decimal.TryParse(numAmount.Text.Trim(), out amount) || amount <= 0)
            {
                MessageBox.Show(ErrInvalidAmount, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3) Пользователь (опционально)
            int? selectedUserId = null;
            var fullName = comboUser.Text.Trim();
            if (!string.IsNullOrEmpty(fullName))
            {
                var user = _users.FirstOrDefault(u =>
                    $"{u.LastName} {u.FirstName}"
                        .Equals(fullName, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    MessageBox.Show(ErrUserNotFound, "Внимание",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                selectedUserId = user.UserId;
            }

            // 4) Остальные поля
            var date = datePicker.Value.Date;
            var comment = txtComment.Text.Trim();

            // 5) Сохраняем через DB
            try
            {
                var db = new DB();
                bool ok = _isNew
                    ? db.CreateTransaction(date, type, amount, comment, selectedUserId)
                    : db.UpdateTransaction(_model.TransactionId, date, type, amount, comment, selectedUserId);

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
        /// Закрывает эту форму и возвращает на дашборд Finance.
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
