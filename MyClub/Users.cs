using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyClub
{
    public partial class Users : Form
    {
        public Users()
        {
            InitializeComponent();
        }

        private void Users_Load(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        /// <summary>
        /// Загружает данные из базы и ставит в DataGridView.
        /// </summary>
        private void RefreshGrid()
        {
            var all = PersonalData.GetAllUsers();
            guna2DataGridView1.DataSource = all;

            // Скрываем поле Photo (бинарный массив) и другие ненужные
            if (guna2DataGridView1.Columns["Photo"] != null)
                guna2DataGridView1.Columns["Photo"].Visible = false;
        }
        /// <summary>
        /// Открывает форму AllProfile вместо себя в panel3 главного окна.
        /// </summary>
        private void guna2Button2_Click(object sender, EventArgs e) //изменить/сохранить
        {
            if (guna2DataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Выберите пользователя в списке.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var pd = guna2DataGridView1.CurrentRow.DataBoundItem as PersonalData;
            if (pd == null) return;

            // Находим родительский Form1 и вызываем OpenForm
            if (this.TopLevelControl is Form1 mainForm)
            {
                mainForm.OpenForm(new AllProfile(pd));
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        private void guna2Button1_Click(object sender, EventArgs e) //добавить
        {

            if (this.TopLevelControl is Form1 mainForm)
            {
                mainForm.OpenForm(new AllProfile());
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e) //удалить
        {
            if (guna2DataGridView1.CurrentRow == null) return;

            var pd = guna2DataGridView1.CurrentRow.DataBoundItem as PersonalData;
            if (pd == null) return;

            if (MessageBox.Show($"Удалить пользователя {pd.Login}?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var db = new DB();
            bool ok = db.DeleteUser(pd.UserId);  
            if (ok)
                RefreshGrid();
            else
                MessageBox.Show("Ошибка при удалении.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        
    }
}
