using Guna.Charts.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MyClub
{
    public partial class Finance : Form
    {
        private List<SubscriptionType> _subTypes;

        public Finance()
        {
            InitializeComponent();
            // привязка событий
            guna2ComboBox1.SelectedIndexChanged += ComboChanged;
            guna2ComboBox2.SelectedIndexChanged += ComboChanged;
            guna2ComboBox3.SelectedIndexChanged += ComboChanged;
            guna2ComboBox4.SelectedIndexChanged += ComboChanged;
            buttonUpdate.Click += ButtonUpdate_Click;
            btnNew.Click += BtnNew_Click;
            buttonChange.Click += ButtonChange_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        private void Finance_Load(object sender, EventArgs e)
        {
            InitComboBoxes();
            LoadSubscriptionTypes();
            RefreshDashboard();
        }

        private void InitComboBoxes()
        {
            // период
            guna2ComboBox1.Items.Clear();
            guna2ComboBox1.Items.AddRange(new object[]
            {
                "За всё время","День","Неделя","Месяц","Квартал","Полугодие","Год"
            });
            guna2ComboBox1.SelectedIndex = 3;

            // операции
            guna2ComboBox2.Items.Clear();
            guna2ComboBox2.Items.AddRange(new object[] { "Все", "Доход", "Расход" });
            guna2ComboBox2.SelectedIndex = 0;

            // режим
            guna2ComboBox4.Items.Clear();
            guna2ComboBox4.Items.AddRange(new object[] { "Транзакции", "Подписки" });
            guna2ComboBox4.SelectedIndex = 0;
        }

        private void LoadSubscriptionTypes()
        {
            _subTypes = new DB().GetSubscriptionTypes() ?? new List<SubscriptionType>();
            // "Все"
            _subTypes.Insert(0, new SubscriptionType { SubscriptionTypeId = 0, Name = "Все" });

            guna2ComboBox3.DataSource = null;
            guna2ComboBox3.DataSource = _subTypes;
            guna2ComboBox3.DisplayMember = nameof(SubscriptionType.Name);
            guna2ComboBox3.ValueMember = nameof(SubscriptionType.SubscriptionTypeId);
            guna2ComboBox3.SelectedIndex = 0;
        }

        private void ComboChanged(object sender, EventArgs e)
        {
            RefreshDashboard();
        }

        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            if (guna2ComboBox4.Text == "Транзакции")
            {
                SetTransactionLabels();
                RefreshTransactionDashboard();
            }
            else
            {
                SetSubscriptionLabels();
                RefreshSubscriptionDashboard();
            }
        }

        #region — Транзакции —

        private void SetTransactionLabels()
        {
            guna2HtmlLabel0.Text = "Баланс";
            guna2HtmlLabel1.Text = "Доход";
            guna2HtmlLabel2.Text = "Расход";
            guna2HtmlLabel3.Text = "Итог";
        }

        private void RefreshTransactionDashboard()
        {
            DateTime from, to;
            GetDateRange(out from, out to);

            var db = new DB();
            var txs = db.GetTransactions(from, to);

            // статистика
            decimal balance = db.GetCurrentBalance();
            decimal income = txs.Where(t => t.OperationType == "Доход").Sum(t => t.Amount);
            decimal expense = txs.Where(t => t.OperationType == "Расход").Sum(t => t.Amount);

            balanslabel.Text = string.Format("{0:N2} ₽", balance);
            incomelabel.Text = string.Format("{0:N2} ₽", income);
            expenseslabel.Text = string.Format("{0:N2} ₽", expense);
            netprofitlabel.Text = string.Format("{0:N2} ₽", income - expense);

            // графики
            UpdateBalanceChart(txs, from);
            UpdatePieChart(txs, guna2ComboBox2.Text);

            BindTransactionsGrid(txs);
        }

        private void BindTransactionsGrid(IEnumerable<Transaction> txs)
        {
            var users = PersonalData.GetAllUsers()
                .ToDictionary(u => u.UserId, u => u.LastName + " " + u.FirstName);

            var view = txs.Select(t => new
            {
                t.TransactionId,
                Дата = t.Date,
                ТипОперации = t.OperationType,
                Сумма = t.Amount,
                Комментарий = t.Comment,
                Пользователь = users.ContainsKey(t.UserId ?? -1)
                                ? users[t.UserId ?? -1]
                                : "(неизвестно)"
            }).ToList();

            guna2DataGridView1.DataSource = view;
        }

        #endregion

        #region — Подписки —

        private void SetSubscriptionLabels()
        {
            guna2HtmlLabel0.Text = "Активных";
            guna2HtmlLabel1.Text = "Истёкших";
            guna2HtmlLabel2.Text = "Всего";
            guna2HtmlLabel3.Text = "";
        }

        private void RefreshSubscriptionDashboard()
        {
            DateTime from, to;
            GetDateRange(out from, out to);

            if (guna2ComboBox3.DataSource == null)
            {
                ClearSubscriptionView();
                return;
            }

            int sel = (int)guna2ComboBox3.SelectedValue;
            int? typeFilter = sel == 0 ? (int?)null : sel;

            var subs = new DB()
                .GetUserSubscriptions(from, to, typeFilter)
                ?? new List<UserSubscription>();

            if (!subs.Any())
            {
                ClearSubscriptionView();
                return;
            }

            RenderSubscriptionStats(subs);
            DrawSubscriptionPieChart(subs);
            DrawSubscriptionAreaChart(subs, from, to);
            BindSubscriptionGrid(subs);
        }

        private void ClearSubscriptionView()
        {
            guna2DataGridView1.DataSource = null;
            gunaChart1.Datasets.Clear();
            gunaChart2.Datasets.Clear();
            balanslabel.Text = incomelabel.Text = expenseslabel.Text = netprofitlabel.Text = "0";
        }

        private void RenderSubscriptionStats(List<UserSubscription> subs)
        {
            int total = subs.Count;
            int active = subs.Count(s => s.EndDate >= DateTime.Today && s.IsActive);
            int expired = total - active;

            balanslabel.Text = active.ToString();
            incomelabel.Text = expired.ToString();
            expenseslabel.Text = total.ToString();
            netprofitlabel.Text = "";
        }

        private void DrawSubscriptionPieChart(IEnumerable<UserSubscription> subs)
        {
            var pieData = subs
                .GroupBy(s => _subTypes.First(t => t.SubscriptionTypeId == s.SubscriptionTypeId).Name)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            var pts = new LPointCollection();
            foreach (var p in pieData)
                pts.Add(new LPoint { Y = p.Count, Label = p.Type });

            var ds = new GunaPieDataset
            {
                Label = "Подписки",
                FillColors = new ColorCollection { Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Gray },
                DataPoints = pts
            };

            gunaChart2.Datasets.Clear();
            gunaChart2.Datasets.Add(ds);
        }

        private void DrawSubscriptionAreaChart(IEnumerable<UserSubscription> subs, DateTime from, DateTime to)
        {
            var daily = new List<Tuple<string, int>>();
            for (var d = from; d <= to; d = d.AddDays(1))
            {
                int cnt = subs.Count(s => s.StartDate <= d && s.EndDate >= d);
                if (cnt > 0)
                    daily.Add(Tuple.Create(d.ToString("dd.MM"), cnt));
            }

            var pts = new LPointCollection();
            foreach (var t in daily)
                pts.Add(new LPoint { Y = t.Item2, Label = t.Item1 });

            var ds = new GunaAreaDataset
            {
                Label = "Активные подписки",
                FillColor = Color.FromArgb(50, 67, 96, 130),
                BorderColor = Color.FromArgb(200, 67, 96, 130),
                ShowLine = true,
                DataPoints = pts
            };

            gunaChart1.Datasets.Clear();
            gunaChart1.Datasets.Add(ds);
        }

        private void BindSubscriptionGrid(IEnumerable<UserSubscription> subs)
        {
            var users = PersonalData.GetAllUsers()
                .ToDictionary(u => u.UserId, u => u.LastName + " " + u.FirstName);

            var view = subs.Select(s => new
            {
                s.UserSubscriptionId,
                Пользователь = users.ContainsKey(s.UserId) ? users[s.UserId] : "(?)",
                Тип = _subTypes.First(t => t.SubscriptionTypeId == s.SubscriptionTypeId).Name,
                Начало = s.StartDate,
                Конец = s.EndDate,
                Активна = s.IsActive ? "Да" : "Нет"
            }).ToList();

            guna2DataGridView1.DataSource = view;
        }

        #endregion

        #region — Графики транзакций —

        private void UpdateBalanceChart(IEnumerable<Transaction> txs, DateTime from)
        {
            var dates = txs.Select(t => t.Date).Distinct().OrderBy(d => d).ToList();
            decimal run = new DB().GetStartingBalance(from);

            gunaAreaDataset1.DataPoints.Clear();
            foreach (var d in dates)
            {
                run += txs.Where(t => t.Date == d)
                          .Sum(t => t.OperationType == "Доход" ? t.Amount : -t.Amount);
                var pt = new LPoint { Y = (double)run };
                gunaAreaDataset1.DataPoints.Add(pt);
            }

            gunaAreaDataset1.FillColor = Color.FromArgb(50, 67, 96, 130);
            gunaAreaDataset1.BorderColor = Color.FromArgb(200, 67, 96, 130);
            gunaAreaDataset1.ShowLine = true;
            gunaAreaDataset1.Label = "Баланс";

            gunaChart1.Datasets.Clear();
            gunaChart1.Datasets.Add(gunaAreaDataset1);
        }

        private void UpdatePieChart(IEnumerable<Transaction> txs, string filter)
        {
            var slice = txs
                .Where(t => filter == "Все" || t.OperationType == filter)
                .GroupBy(t => t.Comment)
                .Select(g => new
                {
                    Cat = string.IsNullOrWhiteSpace(g.Key) ? "(без описания)" : g.Key,
                    Sum = g.Sum(t => t.OperationType == "Доход" ? t.Amount : -t.Amount)
                })
                .Where(x => x.Sum != 0)
                .ToList();

            var pts = new LPointCollection();
            foreach (var s in slice)
                pts.Add(new LPoint { Y = (double)s.Sum, Label = s.Cat });

            var ds = new GunaPieDataset
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
            gunaChart2.Datasets.Add(ds);
        }

        #endregion

        #region — Управление датами —

        private void GetDateRange(out DateTime from, out DateTime to)
        {
            to = DateTime.Today;
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
        }

        #endregion

        #region — Кнопки New/Change/Delete —

        private void BtnNew_Click(object sender, EventArgs e)
        {
            var main = this.TopLevelControl as Form1;
            if (main == null) return;

            if (guna2ComboBox4.Text == "Транзакции")
                main.OpenForm(new TransactionForm());
            else
                main.OpenForm(new SubscriptionForm());
        }

        private void ButtonChange_Click(object sender, EventArgs e)
        {
            if (guna2DataGridView1.CurrentRow == null) return;
            var main = this.TopLevelControl as Form1;
            if (main == null) return;

            if (guna2ComboBox4.Text == "Транзакции")
            {
                int id = (int)guna2DataGridView1.CurrentRow.Cells["TransactionId"].Value;
                var tx = new DB().GetTransactionById(id);
                if (tx != null) main.OpenForm(new TransactionForm(tx));
            }
            else
            {
                int id = (int)guna2DataGridView1.CurrentRow.Cells["UserSubscriptionId"].Value;
                var sub = new DB().GetUserSubscriptionById(id);
                if (sub != null) main.OpenForm(new SubscriptionForm(sub));
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (guna2DataGridView1.CurrentRow == null) return;
            var db = new DB();
            if (guna2ComboBox4.Text == "Транзакции")
            {
                int txId=Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["TransactionId"].Value);
                if (MessageBox.Show($"Удалить транзакцию #{txId}?",
                    "Подтвердите", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    db.DeleteTransaction(txId);
            }
            else
            {
                int subId = Convert.ToInt32(guna2DataGridView1.CurrentRow.Cells["UserSubscriptionId"].Value);
                if (MessageBox.Show($"Удалить подписку #{subId}?",
                    "Подтвердите", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    db.DeleteUserSubscription(subId);
            }
            RefreshDashboard();
        }

        #endregion
    }
}
