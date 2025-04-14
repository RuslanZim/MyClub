using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyClub
{
    public class DB
    {
        public string StringConnection => "Server=DESKTOP-N6JL58D\\SQLEXPRESS;Database=MyClubDB;Trusted_Connection=True";

        public bool UpdateUserLoginPassword(int userId, string newLogin, string newPassword)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(StringConnection))
                {
                    connection.Open();
                    if (connection.State != ConnectionState.Open)
                    {
                        MessageBox.Show("Не удалось установить подключение к базе данных.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Параметризованный запрос
                    string sql = @"
                UPDATE UsersAuth 
                SET [Логин] = @newLogin, [Пароль] = @newPassword
                WHERE [ID Пользователя] = @id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@newLogin", newLogin);
                        cmd.Parameters.AddWithValue("@newPassword", newPassword);
                        cmd.Parameters.AddWithValue("@id", userId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        // Если rowsAffected > 0, значит запись в БД успешно обновлена
                        return (rowsAffected > 0);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Возникла ошибка при выполнении запроса: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла непредвиденная ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

    }

}
