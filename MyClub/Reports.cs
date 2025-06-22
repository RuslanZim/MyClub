using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Guna.UI2.WinForms;
using Guna.Charts.WinForms;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing.Imaging;


namespace MyClub
{
    public partial class Reports : Form
    {
        public Reports()
        {
            InitializeComponent();
            this.Load += Reports_Load;
            btnGenerate.Click += (s, e) => GenerateReport();
            btnExportExcel.Click += (s, e) => ExportToExcel(dgvReport);
            btnExportPdf.Click += (s, e) => ExportToPdf(dgvReport);
        }

        private void Reports_Load(object sender, EventArgs e)
        {
            comboReportType.Items.Clear();
            comboInterval.Items.Clear();

            comboReportType.Items.AddRange(new[] {
                "Баланс", "Доход/Расход", "Абонементы", "События", "Инвентарь"
            });
            comboReportType.SelectedIndex = 0;

            comboInterval.Items.AddRange(new[] {
                "День", "Неделя", "Месяц", "Квартал", "Год", "Все", "Пользовательский диапазон"
            });
            comboInterval.SelectedIndex = 5;

            datePickerFrom.Value = DateTime.Today.AddDays(-30);
            datePickerTo.Value = DateTime.Today;

            GenerateReport();
        }

        private void GenerateReport()
        {
            var db = new DB();
            string type = comboReportType.SelectedItem.ToString();
            string period = comboInterval.SelectedItem.ToString();

            // 1) вычисляем диапазон from..to
            DateTime to = DateTime.Today;
            DateTime from;

            switch (period)
            {
                case "День":
                    from = to.AddDays(-1);
                    break;
                case "Неделя":
                    from = to.AddDays(-7);
                    break;
                case "Месяц":
                    from = to.AddMonths(-1);
                    break;
                case "Квартал":
                    from = to.AddMonths(-3);
                    break;
                case "Год":
                    from = to.AddYears(-1);
                    break;
                case "Пользовательский диапазон":
                    from = datePickerFrom.Value.Date;
                    to = datePickerTo.Value.Date;
                    break;
                case "Все":
                default:
                    from = new DateTime(2000, 1, 1);
                    break;
            }

            // 2) очистка UI
            dgvReport.DataSource = null;
            chartBalance.Visible = (type != "Инвентарь");
            chartBalance.Datasets.Clear();

            // 3) строим отчёт
            if (type == "Баланс")
                DrawBalance(db, from, to);
            else if (type == "Доход/Расход")
                DrawIncomeExpense(db, from, to);
            else if (type == "Абонементы")
                DrawSubscriptions(db, from, to);
            else if (type == "События")
                DrawEvents(db, from, to);
            else // "Инвентарь"
                dgvReport.DataSource = db.GetAllInventory();

            chartBalance.Update();
        }

        private void DrawBalance(DB db, DateTime from, DateTime to)
        {
            decimal start = db.GetStartingBalance(from);
            decimal curr = db.GetCurrentBalance();
            dgvReport.DataSource = new[] {
                new { Показатель = $"Баланс на {from:dd.MM.yyyy}", Значение = start },
                new { Показатель = "Текущий баланс",            Значение = curr  }
            }.ToList();

            var daily = db.GetTransactions(from, to)
                          .GroupBy(t => t.Date.Date)
                          .OrderBy(g => g.Key)
                          .Select(g => new {
                              Date = g.Key,
                              Delta = g.Sum(t => t.OperationType == "Доход" ? t.Amount : -t.Amount)
                          })
                          .ToList();

            var ds = new GunaAreaDataset
            {
                Label = "Сальдо",
                FillColor = Color.FromArgb(50, 67, 96, 130),
                BorderColor = Color.FromArgb(200, 67, 96, 130),
                ShowLine = true,
                DataPoints = new LPointCollection()
            };

            decimal running = start;
            foreach (var r in daily)
            {
                running += r.Delta;
                ds.DataPoints.Add(new LPoint
                {
                    Y = (double)running,
                    Label = r.Date.ToString("dd.MM")
                });
            }

            chartBalance.Datasets.Add(ds);
        }

        private void DrawIncomeExpense(DB db, DateTime from, DateTime to)
        {
            var table = db.GetTransactions(from, to)
                          .GroupBy(t => t.Date.Date)
                          .OrderBy(g => g.Key)
                          .Select(g => new {
                              Date = g.Key,
                              Income = g.Where(x => x.OperationType == "Доход").Sum(x => x.Amount),
                              Expense = g.Where(x => x.OperationType == "Расход").Sum(x => x.Amount)
                          })
                          .ToList();

            dgvReport.DataSource = table
                .Select(r => new {
                    Дата = r.Date,
                    Доход = r.Income,
                    Расход = r.Expense
                })
                .ToList();

            var incDs = new GunaBarDataset { Label = "Доход", DataPoints = new LPointCollection() };
            var expDs = new GunaBarDataset { Label = "Расход", DataPoints = new LPointCollection() };

            foreach (var r in table)
            {
                string lbl = r.Date.ToString("dd.MM");
                incDs.DataPoints.Add(new LPoint { Y = (double)r.Income, Label = lbl });
                expDs.DataPoints.Add(new LPoint { Y = (double)r.Expense, Label = lbl });
            }

            chartBalance.Datasets.Add(incDs);
            chartBalance.Datasets.Add(expDs);
        }

        private void DrawSubscriptions(DB db, DateTime from, DateTime to)
        {
            var subs = db.GetUserSubscriptions(from, to, null);
            dgvReport.DataSource = subs;

            var table = subs
                .GroupBy(s => s.StartDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList();

            var ds = new GunaLineDataset { Label = "Новые подписки", DataPoints = new LPointCollection() };
            foreach (var r in table)
                ds.DataPoints.Add(new LPoint { Y = (double)r.Count, Label = r.Date.ToString("dd.MM") });

            chartBalance.Datasets.Add(ds);
        }

        private void DrawEvents(DB db, DateTime from, DateTime to)
        {
            var evs = db.GetEvents(null, null, null, null, null)
                        .Where(e => e.Date.Date >= from && e.Date.Date <= to)
                        .ToList();
            dgvReport.DataSource = evs;

            var table = evs
                .GroupBy(e => e.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList();

            var ds = new GunaLineDataset { Label = "События", DataPoints = new LPointCollection() };
            foreach (var r in table)
                ds.DataPoints.Add(new LPoint { Y = (double)r.Count, Label = r.Date.ToString("dd.MM") });

            chartBalance.Datasets.Add(ds);
        }

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel(dgvReport);
        }

        private void BtnExportPdf_Click(object sender, EventArgs e)
        {
            ExportToPdf(dgvReport);
        }

        private void ExportToExcel(DataGridView dgv)
        {
            using (var dlg = new SaveFileDialog { Filter = "Excel Workbook|*.xlsx", FileName = "Report.xlsx" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Отчёт");

                    // 1) Заголовки
                    for (int col = 0; col < dgv.Columns.Count; col++)
                        ws.Cell(1, col + 1).Value = dgv.Columns[col].HeaderText;

                    // 2) Данные таблицы
                    for (int row = 0; row < dgv.Rows.Count; row++)
                    {
                        if (dgv.Rows[row].IsNewRow) continue;
                        for (int col = 0; col < dgv.Columns.Count; col++)
                        {
                            ws.Cell(row + 2, col + 1).Value =
                                dgv.Rows[row].Cells[col].Value?.ToString() ?? "";
                        }
                    }

                    ws.Columns().AdjustToContents();

                    // 3) Снимок графика
                    using (var msImg = new MemoryStream())
                    {
                        var bmp = new Bitmap(chartBalance.Width, chartBalance.Height);
                        // вот здесь явно указываем System.Drawing.Rectangle
                        chartBalance.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));
                        bmp.Save(msImg, ImageFormat.Png);
                        msImg.Position = 0;

                        var pic = ws.AddPicture(msImg)
                                    .MoveTo(ws.Cell(dgv.Rows.Count + 4, 1))
                                    .Scale(0.5);
                    }

                    wb.SaveAs(dlg.FileName);
                    MessageBox.Show("Экспорт в Excel выполнен успешно.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ExportToPdf(DataGridView dgv)
        {
            using (var dlg = new SaveFileDialog { Filter = "PDF Document|*.pdf", FileName = "Report.pdf" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                var pdfTable = new PdfPTable(dgv.Columns.Count) { WidthPercentage = 100 };

                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, "CP1251", BaseFont.EMBEDDED);
                var font = new iTextSharp.text.Font(bf, 10);

                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    var cell = new PdfPCell(new Phrase(col.HeaderText, font))
                    {
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    pdfTable.AddCell(cell);
                }
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;
                    foreach (DataGridViewCell gridCell in row.Cells)
                    {
                        var text = gridCell.Value?.ToString() ?? "";
                        var cell = new PdfPCell(new Phrase(text, font))
                        {
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        pdfTable.AddCell(cell);
                    }
                }

                using (var fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // Здесь ambiguity не возникает, т.к. PageSize.A4.Rotate() — это iTextSharp.text.Rectangle
                    var doc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                    PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    using (var msImg = new MemoryStream())
                    {
                        var bmp = new Bitmap(chartBalance.Width, chartBalance.Height);
                        // снова квалификация
                        chartBalance.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));
                        bmp.Save(msImg, ImageFormat.Png);
                        var pdfImg = iTextSharp.text.Image.GetInstance(msImg.ToArray());
                        pdfImg.Alignment = Element.ALIGN_CENTER;
                        pdfImg.ScaleToFit(doc.PageSize.Width - 40, 300);
                        doc.Add(pdfImg);
                    }

                    doc.Add(pdfTable);
                    doc.Close();
                }

                MessageBox.Show("Экспорт в PDF выполнен успешно.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


    }
}
