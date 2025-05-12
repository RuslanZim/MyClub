using Guna.Charts.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;


namespace MyClub
{
    public partial class Finance : Form
    {
        public Finance()
        {
            InitializeComponent();
        }

        private void Finance_Load(object sender, EventArgs e)
        {
            // 1) Период
            guna2ComboBox1.Items.Clear();
            guna2ComboBox1.Items.AddRange(new object[]
            {
                "За всё время", "День", "Неделя",
                "Месяц", "Квартал", "Полугодие", "Год"
            });
            guna2ComboBox1.SelectedIndex = 3; // «Месяц» по умолчанию

            // 2) Фильтр по типу операции
            guna2ComboBox2.Items.Clear();
            guna2ComboBox2.Items.AddRange(new object[] { "Все", "Доход", "Расход" });
            guna2ComboBox2.SelectedIndex = 0;

            // И сразу отрисовываем
            RefreshDashboard();
        }
        private void RefreshDashboard()
        {// 1) Диапазон
            DateTime to = DateTime.Today, from;
            switch (guna2ComboBox1.Text)
            {
                case "За всё время": from = new DateTime(2000, 1, 1); break;
                case "День": from = to; break;
                case "Неделя": from = to.AddDays(-7); break;
                case "Месяц": from = to.AddMonths(-1); break;
                case "Квартал": from = to.AddMonths(-3); break;
                case "Полугодие": from = to.AddMonths(-6); break;
                case "Год": from = to.AddYears(-1); break;
                default: from = to.AddMonths(-1); break;
            }

            var db = new DB();
            var txs = db.GetTransactions(from, to);

            // 2) Итоги
            decimal balance = db.GetCurrentBalance();
            decimal income = txs.Where(t => t.OperationType == "Доход").Sum(t => t.Amount);
            decimal expense = txs.Where(t => t.OperationType == "Расход").Sum(t => t.Amount);
            decimal net = income - expense;

            balanslabel.Text = $"{balance:N2} ₽";
            incomelabel.Text = $"{income:N2} ₽";
            expenseslabel.Text = $"{expense:N2} ₽";
            netprofitlabel.Text = $"{net:N2} ₽";

            // 3) Графики
            UpdateBalanceChart(txs, from);
            UpdatePieChart(txs, guna2ComboBox2.Text);

            // Получим список всех пользователей и соберём словарь: UserId → ФИО
            var users = PersonalData.GetAllUsers();
            var userDict = users.ToDictionary(
                u => u.UserId,
                u => $"{u.LastName} {u.FirstName}"
            );

            // Сформируем проекцию для DataGridView, подставив вместо UserId человекочитаемое поле UserName
            var view = txs
                .Select(t => new
                {
                    t.TransactionId,
                    t.Date,
                    t.OperationType,
                    t.Amount,
                    t.Comment,
                    UserName = userDict.TryGetValue(t.UserId ?? -1, out var fio) ? fio : "(неизвестно)"
                })
                .ToList();

            // 3) **Привязка к DataGridView**
            var grid = guna2DataGridView1;
            grid.DataSource = view; ;
            // Переименовываем заголовки колонок на русский
            if (grid.Columns["Date"] != null) grid.Columns["Date"].HeaderText = "Дата";
            if (grid.Columns["OperationType"] != null) grid.Columns["OperationType"].HeaderText = "Тип операции";
            if (grid.Columns["Amount"] != null) grid.Columns["Amount"].HeaderText = "Сумма";
            if (grid.Columns["Comment"] != null) grid.Columns["Comment"].HeaderText = "Комментарий";
            if (grid.Columns["UserName"] != null) grid.Columns["UserName"].HeaderText = "Пользователь";


        }

        private void UpdateBalanceChart(IEnumerable<Transaction> txs, DateTime from)
        {
           // 1) Собираем уникальные даты и считаем накопительный баланс
    var dates = txs.Select(t => t.Date).Distinct().OrderBy(d => d).ToList();
            decimal run = new DB().GetStartingBalance(from);

            // 2) Очищаем старые точки
            gunaAreaDataset1.DataPoints.Clear();

            // 3) Для каждой даты создаём LPoint и пушим в DataPoints
            foreach (var d in dates)
            {
                run += txs
                    .Where(t => t.Date == d)
                    .Sum(t => t.OperationType == "Доход" ? t.Amount : -t.Amount);

                var pt = new LPoint
                {
                    Y = (double)run,           // само значение
                };
                gunaAreaDataset1.DataPoints.Add(pt);
            }

            // 4) Настраиваем цвета/линию/метку
            gunaAreaDataset1.FillColor = Color.FromArgb(50, 67, 96, 130);
            gunaAreaDataset1.BorderColor = Color.FromArgb(200, 67, 96, 130);
            gunaAreaDataset1.ShowLine = true;
            gunaAreaDataset1.Label = "Баланс";

            // 5) Перерисовываем chart
            gunaChart1.Datasets.Clear();
            gunaChart1.Datasets.Add(gunaAreaDataset1);
        }

    

        private void UpdatePieChart(IEnumerable<Transaction> txs, string filter)
        {
            // по комментариям
            var slice = txs
                .Where(t => filter == "Все" || t.OperationType == filter)
                .GroupBy(t => t.Comment)
                .Select(g => new {
                    Cat = string.IsNullOrWhiteSpace(g.Key) ? "(без описания)" : g.Key,
                    Sum = g.Sum(t => t.OperationType == "Доход" ? t.Amount : -t.Amount)
                })
                .Where(x => x.Sum != 0)
                .ToList();

            var pts = new LPointCollection();
            foreach (var s in slice)
            {
                pts.Add(new LPoint { Y = (double)s.Sum, Label = s.Cat });
            }

            var pie = new GunaPieDataset
            {
                Label = "Категории",
                FillColors = new ColorCollection {
                    Color.FromArgb(255,99,132),
                    Color.FromArgb(255,159,64),
                    Color.FromArgb(255,205,86),
                    Color.FromArgb( 75,192,192),
                    Color.FromArgb(153,102,255),
                    Color.FromArgb(255,159,64)
                },
                DataPoints = pts
            };

            gunaChart2.Datasets.Clear();
            gunaChart2.Datasets.Add(pie);
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshDashboard();
        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshDashboard();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            if (this.TopLevelControl is Form1 mainForm)
            {
                mainForm.OpenForm(new TransactionForm());
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            RefreshDashboard();
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            if (guna2DataGridView1.CurrentRow == null) return;

            // 1) Считаем ID из скрытой колонки
            int id = (int)guna2DataGridView1.CurrentRow.Cells["TransactionId"].Value;

            // 2) Подтягиваем полный объект из БД
            var db = new DB();
            var tx = db.GetTransactionById(id);
            if (tx == null)
            {
                MessageBox.Show("Транзакция не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this.TopLevelControl is Form1 mainForm)
            {
                mainForm.OpenForm(new TransactionForm(tx));
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {

            if (guna2DataGridView1.CurrentRow == null) return;
            var tx = (Transaction)guna2DataGridView1.CurrentRow.DataBoundItem;
            if (MessageBox.Show($"Удалить транзакцию #{tx.TransactionId}?", "Подтвердите",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            if (new DB().DeleteTransaction(tx.TransactionId))
                RefreshDashboard();
            else
                MessageBox.Show("Ошибка при удалении.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
       
        }
    }
}
