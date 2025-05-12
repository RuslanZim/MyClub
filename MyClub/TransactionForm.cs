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
    public partial class TransactionForm : Form
    {
        private readonly Transaction _model;
        private readonly bool _isNew;
        private List<PersonalData> _users;
        public TransactionForm()
        {
            InitializeComponent();
            _isNew = true;
        }
        public TransactionForm(Transaction model)
        {
            InitializeComponent();
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _isNew = false;
            Text = _isNew ? "Новая транзакция" : $"Транзакция #{model.TransactionId}";
        }

        private void TransactionForm_Load(object sender, EventArgs e)
        {
            // 1) Тип операции
            comboOperation.Items.Clear();
            comboOperation.Items.AddRange(new[] { "Доход", "Расход" });

            // 2) Получаем список всех пользователей
            _users = PersonalData.GetAllUsers();

            // 3) Собираем ФИО и настраиваем автодополнение
            var names = _users
                .Select(u => $"{u.LastName} {u.FirstName}")
                .ToArray();

            var ac = new AutoCompleteStringCollection();
            ac.AddRange(names);

            comboUser.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboUser.AutoCompleteSource = AutoCompleteSource.CustomSource;
            comboUser.AutoCompleteCustomSource = ac;
            comboUser.PlaceholderText = "Начните вводить ФИО...";

            if (!_isNew)
                FillFromModel();
        }

        private void FillFromModel()
        {
            // Дата
            datePicker.Value = _model.Date;

            // Тип
            comboOperation.SelectedItem = _model.OperationType;

            // Сумма
            numAmount.Text = _model.Amount.ToString("F2");

            // Комментарий
            txtComment.Text = _model.Comment;

            // подставим ФИО из модели
            var pd = _users.FirstOrDefault(u => u.UserId == _model.UserId);
            comboUser.Text = pd == null
                ? ""
                : $"{pd.LastName} {pd.FirstName}";

            // ID транзакции
            transactionId.Text = _model.TransactionId.ToString();
        }
        


        private void btnSave_Click(object sendr, EventArgs e)
        {
            // 1) Валидация
            if (comboOperation.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип операции.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(numAmount.Text.Trim(), out var amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму.", "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

          

            // 3) Найдём пользователя по введённому ФИО
            var fullName = comboUser.Text.Trim();
            var user = _users.FirstOrDefault(u =>
                $"{u.LastName} {u.FirstName}".Equals(fullName, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                MessageBox.Show(
                    "Пользователь не найден.\n" +
                    "Выберите из подсказок ФИО, или оставьте поле пустым, если не нужно.",
                    "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4) Собираем остальные поля
            var date = datePicker.Value.Date;
            var type = comboOperation.SelectedItem.ToString();
            var comment = txtComment.Text.Trim();
            int? selectedUserId = user.UserId;

            // 5) Сохраняем через DB
            var db = new DB();
            bool ok = _isNew
                ? db.CreateTransaction(date, type, amount, comment, selectedUserId)
                : db.UpdateTransaction(_model.TransactionId, date, type, amount, comment, selectedUserId);

            if (!ok)
            {
                MessageBox.Show("Не удалось сохранить транзакцию.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;


            // Теперь сменим форму
            if (this.TopLevelControl is Form1 mainForm)
            {
                this.Close();
                mainForm.OpenForm(new Finance());
            }
            else
            {
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (this.TopLevelControl is Form1 mainForm)
            {
                this.Close();
                mainForm.OpenForm(new Finance());
            }
        }

        
    }
}
