using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Windows.Documents;

namespace MyClub
{
    public class DB
    {
        public string StringConnection => "Server=DESKTOP-N6JL58D\\SQLEXPRESS;Database=MyClubDB;Trusted_Connection=True";

        /// <summary>
        /// Регистрирует нового пользователя.
        /// Вставляет данные в таблицу UsersAuth (логин, пароль, роль) и UsersProfile (дополнительная информация, включая фото) в рамках одной транзакции.
        /// Остальные поля (email, Фамилия, Имя, Отчество, Дата рождения, Номер телефона, Фото) передаются значениями по умолчанию, если не заданы.
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <param name="role">Роль, например "Пользователь"</param>
        /// <param name="email">Электронная почта (может быть пустой)</param>
        /// <param name="lastName">Фамилия (может быть пустой)</param>
        /// <param name="firstName">Имя (может быть пустым)</param>
        /// <param name="fatherName">Отчество (может быть пустым)</param>
        /// <param name="dateBirth">Дата рождения (может быть null)</param>
        /// <param name="phoneNumber">Номер телефона (может быть пустым)</param>
        /// <param name="photo">Фото в виде массива байтов (может быть null)</param>
        /// <returns>Возвращает true, если регистрация прошла успешно, иначе false.</returns>
        public bool RegisterUser(
            string login,
            string password,
            string role,
            string email,
            string lastName,
            string firstName,
            string fatherName,
            DateTime? dateBirth,
            string phoneNumber,
            byte[] photo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(StringConnection))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        // Вставляем данные в таблицу аутентификации UsersAuth
                        string sqlAuth = @"
                            INSERT INTO [dbo].[UsersAuth] (
                                [Логин], 
                                [Пароль], 
                                [Роль])
                            VALUES (
                                @login, 
                                @password, 
                                @role);
                            SELECT SCOPE_IDENTITY();";
                        int newUserId;
                        using (SqlCommand cmd = new SqlCommand(sqlAuth, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@login", login);
                            cmd.Parameters.AddWithValue("@password", password);
                            cmd.Parameters.AddWithValue("@role", role);
                            newUserId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Вставляем данные в таблицу профиля UsersProfile
                        string sqlProfile = @"
                            INSERT INTO [dbo].[UsersProfile] (
                                [Фамилия],
                                [Имя],
                                [Отчество],
                                [Дата рождения],                              
                                [Номер телефона],   
                                [Электронная почта],                              
                                [Фото],
                                [ID Пользователя])
                            VALUES (
                                @lastName,
                                @firstName,
                                @fatherName,
                                @dateBirth,  
                                @phoneNumber,   
                                @email,                                          
                                @photo,
                                @userId)";
                        using (SqlCommand cmd = new SqlCommand(sqlProfile, connection, transaction))
                        {      
                            cmd.Parameters.AddWithValue("@lastName", lastName);
                            cmd.Parameters.AddWithValue("@firstName", firstName);
                            cmd.Parameters.AddWithValue("@fatherName", fatherName);
                            cmd.Parameters.AddWithValue("@dateBirth", (object)dateBirth ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                            cmd.Parameters.AddWithValue("@email", email);
                            // Явно указываем тип данных для параметра @photo
                            if (photo != null)
                            {
                                cmd.Parameters.Add("@photo", SqlDbType.VarBinary).Value = photo;
                            }
                            else
                            {
                                cmd.Parameters.Add("@photo", SqlDbType.VarBinary).Value = DBNull.Value;
                            }
                            cmd.Parameters.AddWithValue("@userId", newUserId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { }
                        MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool UpdateUserLoginPassword(int userId, string newLogin, string newPassword) //Обновление логина и пароля
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

        public bool UpdateUserProfile(int userId, string lastName, string firstName, string fatherName, //Обновление профиля
            DateTime? dateBirth, string phoneNumber, string email)
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
                    UPDATE UsersProfile 
                    SET [Фамилия] = @lastName, [Имя] = @firstName, [Отчество] = @fatherName,
                    [Дата рождения] = @dateBirth, [Номер телефона] = @phoneNumber, [Электронная почта] = @email
                    WHERE [ID Пользователя] = @id";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@lastName", lastName);
                        cmd.Parameters.AddWithValue("@firstName", firstName);
                        cmd.Parameters.AddWithValue("@fatherName", fatherName);
                        cmd.Parameters.AddWithValue("@dateBirth", (object)dateBirth ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@id", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();

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

        public bool UpdateUserPhoto(int userId, byte[] photoData)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(StringConnection))
                {
                    connection.Open();
                    if (connection.State != ConnectionState.Open)
                    {
                        MessageBox.Show("Не удалось установить подключение к базе данных.",
                                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Параметризованный запрос
                    string sql = @"
                    UPDATE UsersProfile
                    SET [Фото] = @Photo
                    WHERE [ID Пользователя] = @id";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Photo", (object)photoData ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@id", userId);

                        int rowsAffected = cmd.ExecuteNonQuery();
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
        
        public bool DeleteUser(int userId)
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
                    DELETE FROM UsersProfile WHERE [ID Пользователя] = @id;
                    DELETE FROM UsersAuth WHERE [ID Пользователя] = @id;";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();
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
        public bool UpdateUserLoginPasswordAndRole(int userId, string newLogin, string newPassword, string newRole)
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
                    SET [Логин] = @newLogin, [Пароль] = @newPassword, [Роль] = @newRole
                    WHERE [ID Пользователя] = @id";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@newLogin", newLogin);
                        cmd.Parameters.AddWithValue("@newPassword", newPassword);
                        cmd.Parameters.AddWithValue("@newRole", newRole);
                        cmd.Parameters.AddWithValue("@id", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();
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

        public bool SaveUserData(
                int userId,
                string login,
                string password,
                string role,
                string lastName,
                string firstName,
                string fatherName,
                DateTime? dateBirth,
                string phoneNumber,
                string email,
                byte[] photo)
        {
                        const string sqlAuth = @"
                    UPDATE UsersAuth
                    SET [Логин] = @login,
                        [Пароль] = @password,
                        [Роль]   = @role
                    WHERE [ID пользователя] = @userId;";
                        const string sqlProfile = @"
                    UPDATE UsersProfile
                    SET [Фамилия]           = @lastName,
                        [Имя]               = @firstName,
                        [Отчество]          = @fatherName,
                        [Дата рождения]     = @dateBirth,
                        [Номер телефона]    = @phoneNumber,
                        [Электронная почта] = @email,
                        [Фото]              = @photo
                    WHERE [ID пользователя] = @userId;";

            using (var conn = new SqlConnection(StringConnection))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(sqlAuth, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@login", login);
                            cmd.Parameters.AddWithValue("@password", password);
                            cmd.Parameters.AddWithValue("@role", role);
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqlCommand(sqlProfile, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@lastName", lastName);
                            cmd.Parameters.AddWithValue("@firstName", firstName);
                            cmd.Parameters.AddWithValue("@fatherName", fatherName);
                            cmd.Parameters.AddWithValue("@dateBirth", (object)dateBirth ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);
                            cmd.Parameters.AddWithValue("@email", email);

                            // Параметр photo: если null, отправляем DBNull
                            if (photo != null && photo.Length > 0)
                            {
                                cmd.Parameters.Add("@photo", SqlDbType.VarBinary).Value = photo;
                            }
                            else
                            {
                                cmd.Parameters.Add("@photo", SqlDbType.VarBinary).Value = DBNull.Value;
                            }
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        return false;
                    }
                }
            }
        }

     
        public int CreateSection(string name, string description, int? trainerId, string sport)
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
                        return -1;
                    }
                    string sql = @"
                        INSERT INTO Sections ([Название],[Описание],[ID тренера],[Вид спорта])
                        VALUES (@name,@desc,@tid,@sport);
                        SELECT SCOPE_IDENTITY();";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@desc", description);
                        cmd.Parameters.AddWithValue("@tid", trainerId.HasValue
                            ? (object)trainerId.Value
                            : DBNull.Value
                        );
                        cmd.Parameters.AddWithValue("@sport", sport);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания секции: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }


        public List<Section> GetAllSections(string sportFilter)
        {
            var list = new List<Section>();
            string sql = @"
            SELECT s.[ID секции]   AS SectionId,
             s.[Название]    AS Name,
             s.[Описание]    AS Description,
             s.[ID тренера]  AS TrainerId,
             s.[Вид спорта]  AS Sport,
              ISNULL(up.[Фамилия]+' '+up.[Имя],'') AS TrainerName,
             up.[Фото]         AS TrainerPhoto
              FROM Sections s
              LEFT JOIN UsersProfile up
                ON s.[ID тренера] = up.[ID пользователя]
              WHERE (@sport IS NULL OR s.[Вид спорта] = @sport)";
            using (var conn = new SqlConnection(StringConnection))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@sport", (object)sportFilter ?? DBNull.Value);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new Section
                        {
                            SectionId = (int)rdr["SectionId"],
                            Name = rdr["Name"].ToString(),
                            Description = rdr["Description"].ToString(),
                            TrainerId = rdr["TrainerId"] as int?,
                            Sport = rdr["Sport"].ToString(),
                            TrainerName = rdr["TrainerName"].ToString(),
                            TrainerPhoto = rdr["TrainerPhoto"] != DBNull.Value
                               ? (byte[])rdr["TrainerPhoto"]
                               : null
                        });
                    }
                }
            }
            return list;
        }

            public Section GetSectionById(int sectionId)
            {
                const string sql = @"
                SELECT s.[ID секции]   AS SectionId,
                 s.[Название]    AS Name,
                 s.[Описание]    AS Description,
                 s.[ID тренера]  AS TrainerId,
                 s.[Вид спорта]  AS Sport,
                 ISNULL(up.[Фамилия] + ' ' + up.[Имя], '') AS TrainerName,
                 up.[Фото]         AS TrainerPhoto
                  FROM Sections s
                  LEFT JOIN UsersProfile up
                    ON s.[ID тренера] = up.[ID пользователя]
                  WHERE s.[ID секции] = @id";
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", sectionId);
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (!rdr.Read()) return null;
                        return new Section
                        {
                            SectionId = (int)rdr["SectionId"],
                            Name = rdr["Name"].ToString(),
                            Description = rdr["Description"].ToString(),
                            TrainerId = rdr["TrainerId"] as int?,
                            Sport = rdr["Sport"].ToString(),
                            TrainerName = rdr["TrainerName"].ToString(),
                            TrainerPhoto = rdr["TrainerPhoto"] != DBNull.Value
                               ? (byte[])rdr["TrainerPhoto"]
                               : null
                        };
                    }
                }
            }


        public List<PersonalData> GetAllTrainers()
        {
            List<PersonalData> list = new List<PersonalData>();
            const string sql = @"
                SELECT ua.[ID пользователя] AS UserId
                FROM UsersAuth ua
                WHERE ua.[Роль]='Тренер'";
            using (SqlConnection conn = new SqlConnection(StringConnection))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            PersonalData pd = new PersonalData();
                            pd.SetPersonalDataById((int)rdr["UserId"]);
                            list.Add(pd);
                        }
                    }
                }
            }
            return list;
        }
    


        public bool UpdateSection(int sectionId, string name, string description, int? trainerId, string sport)
        {
            const string sql = @"
        UPDATE Sections
           SET [Название]   = @name,
               [Описание]   = @desc,
               [ID тренера] = @tid,
               [Вид спорта] = @sport
         WHERE [ID секции] = @id";

            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@sport", sport);
                    cmd.Parameters.AddWithValue("@id", sectionId);

                    // Явно проверяем HasValue
                    if (trainerId.HasValue)
                        cmd.Parameters.AddWithValue("@tid", trainerId.Value);
                    else
                        cmd.Parameters.AddWithValue("@tid", DBNull.Value);

                    conn.Open();
                    //вывод для проверки какие данные записываются
                    MessageBox.Show($"ID секции: {sectionId}, Название: {name}, Описание: {description}, ID тренера: {trainerId}, Вид спорта: {sport}");
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления секции: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool DeleteSection(int sectionId)
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

                    string sql = "DELETE FROM SectionMembers WHERE [ID секции] = @sectionId";
                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@sectionId", sectionId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления секции: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }

}
