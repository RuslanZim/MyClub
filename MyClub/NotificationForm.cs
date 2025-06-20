using Guna.UI2.WinForms;
using System;
using System.Windows.Forms;

namespace MyClub
{
    public partial class NotificationForm : Form
    {
        private readonly Notification _model;
        private readonly bool _isNew;

        // Тексты ошибок
        private const string ErrEmptyTitle = "Введите заголовок уведомления.";
        private const string ErrEmptyBody = "Введите текст уведомления.";
        private const string ErrSaveFailed = "Не удалось сохранить уведомление.";

        public NotificationForm()
        {
            InitializeComponent();
            _isNew = true;
            Text = "Новое уведомление";
        }

        public NotificationForm(Notification model)
        {
            InitializeComponent();
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _isNew = false;
            Text = $"Уведомление #{model.NotificationId}";
        }

        private void NotificationForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Если редактирование — заполняем поля из модели
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
                this.Close();
            }
        }

        private void FillFromModel()
        {
            // Заголовок
            txtTitle.Text = _model.Title;
            // Тело
            txtBody.Text = _model.Body;
            lblId.Text = _model.NotificationId.ToString();
            lblId.Visible = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1) Заголовок
            var title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show(ErrEmptyTitle, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2) Тело
            var body = txtBody.Text.Trim();
            if (string.IsNullOrEmpty(body))
            {
                MessageBox.Show(ErrEmptyBody, "Внимание",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3) Сохраняем через DB
            try
            {
                var db = new DB();
                bool ok = _isNew
                    ? db.CreateNotification(title, body, PersonalData.Current.UserId)
                    : db.UpdateNotification(_model.NotificationId, title, body);

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
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
