using MyClub.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyClub
{
    public partial class InventoryForm : Form
    {
        private readonly InventoryItem _model;
        private readonly bool _isNew;
        private readonly InventoryService _service = new InventoryService();
        private List<PersonalData> _trainers;

        // Тексты ошибок
        private const string ErrEmptyName = "Введите наименование инвентаря.";
        private const string ErrInvalidQty = "Количество должно быть больше нуля.";
        private const string ErrEmptyStatus = "Введите статус инвентаря.";
        private const string ErrSaveFailed = "Не удалось сохранить изменения.";
        public InventoryForm()
        {
            InitializeComponent();
            _model = new InventoryItem();
            _isNew = true;
            Text = "Новый элемент инвентаря";
        }

        public InventoryForm(InventoryItem model)
        {
            InitializeComponent();
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _isNew = false;
            Text = $"Инвентарь #{model.InventoryId}";

            btnSave.Click += btnSave_Click;
            btnCancel.Click += btnCancel_Click;
        }

        private void InventoryForm_Load(object sender, EventArgs e)
        {
            try
            {
                // 1) Настроим список тренеров
                _trainers = new DB().GetAllTrainers();
                // Вставим пункт «— не назначен —»
                _trainers.Insert(0, new PersonalData { UserId = 0, LastName = "(не назначен)" });
                comboResponsible.DataSource = _trainers;
                comboResponsible.DisplayMember = nameof(PersonalData.LastName);
                comboResponsible.ValueMember = nameof(PersonalData.UserId);
                comboResponsible.SelectedIndex = 0;

                // 2) Настройка дат
                datePickerPurchaseDate.Format = DateTimePickerFormat.Short;
                datePickerLastMaintenance.Format = DateTimePickerFormat.Short;
                datePickerPurchaseDate.Value = DateTime.Today;
                datePickerLastMaintenance.Value = DateTime.Today;

                // 3) Если редактирование — заполняем поля
                if (!_isNew)
                    FillFromModel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке формы: {ex.Message}",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void FillFromModel()
        {
            txtName.Text = _model.Name;
            numQuantity.Value = _model.Quantity;
            txtStatus.Text = _model.Status;
            datePickerPurchaseDate.Value = _model.PurchaseDate ?? DateTime.Today;
            datePickerLastMaintenance.Value = _model.LastMaintenance ?? DateTime.Today;
            // Выбираем ответственного в списке
            var idx = _trainers.FindIndex(t => t.UserId == (_model.ResponsibleId ?? 0));
            comboResponsible.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1) Валидация
            var name = txtName.Text.Trim();
            var status = txtStatus.Text.Trim();
            var qty = (int)numQuantity.Value;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show(ErrEmptyName, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (qty <= 0)
            {
                MessageBox.Show(ErrInvalidQty, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(status))
            {
                MessageBox.Show(ErrEmptyStatus, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2) Собираем модель
            _model.Name = name;
            _model.Quantity = qty;
            _model.Status = status;
            _model.PurchaseDate = datePickerPurchaseDate.Value.Date;
            _model.LastMaintenance = datePickerLastMaintenance.Value.Date;

            var sel = comboResponsible.SelectedItem as PersonalData;
            _model.ResponsibleId = sel != null && sel.UserId != 0
                ? (int?)sel.UserId
                : null;

            // 3) Сохраняем через сервис и логируем исключения
            try
            {
                bool ok = _isNew
                    ? _service.CreateInventory(_model)
                    : _service.UpdateInventory(_model);

                if (!ok)
                {
                    MessageBox.Show(ErrSaveFailed, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"[InventoryForm] Save failed: {ex}");
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            CloseToParent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CloseToParent();
        }

        private void CloseToParent()
        {
            if (TopLevelControl is Form1 mainForm)
            {

                mainForm.OpenForm(new Inventory());
                Close();
            }
        }


    }
}
