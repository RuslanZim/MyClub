using MyClub.Services;
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
    public partial class Inventory : Form
    {

        private readonly InventoryService _svc = new InventoryService();
        public Inventory()
        {
            InitializeComponent();

            // привязка событий — только здесь
            this.Load += Inventory_Load;
            btnNew.Click += BtnNew_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        #region — Загрузка и Refresh —

        private void Inventory_Load(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            var list = _svc.GetAllInventory();
            guna2DataGridView1.DataSource = list;
        }

        #endregion

        #region — Команды Создать/Изменить/Удалить —

        private void BtnNew_Click(object sender, EventArgs e)
        {
            using (var dlg = new InventoryForm())
            {
                if (this.TopLevelControl is Form1 mainForm)
                {
                    mainForm.OpenForm(new InventoryForm());
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (guna2DataGridView1.CurrentRow == null) return;

            var item = guna2DataGridView1.CurrentRow.DataBoundItem as InventoryItem;
            if (item == null) return;

            using (var dlg = new InventoryForm(item))
            {
                if (this.TopLevelControl is Form1 mainForm)
                {
                    mainForm.OpenForm(new InventoryForm());
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (guna2DataGridView1.CurrentRow == null) return;

            var item = guna2DataGridView1.CurrentRow.DataBoundItem as InventoryItem;
            if (item == null) return;

            if (MessageBox.Show(
                    $"Удалить «{item.Name}» из инвентаря?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question
                ) != DialogResult.Yes)
                return;

            if (!_svc.DeleteInventory(item.InventoryId))
                MessageBox.Show("Не удалось удалить.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            RefreshGrid();
        }

        #endregion
    }
}
    