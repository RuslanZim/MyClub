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


        /// <summary>
        /// Возвращает список транзакций за диапазон дат [from..to]
        /// </summary>
        public List<Transaction> GetTransactions(DateTime from, DateTime to)
        {
            var list = new List<Transaction>();
            const string sql = @"
            SELECT
            [ID транзакции]   AS TransactionId,
            [Дата транзакции] AS Date,
            [Сумма]           AS Amount,
            [Тип операции]    AS OperationType,
            [Комментарий]     AS Comment,
            [ID пользователя] AS UserId
            FROM dbo.Transactions
            WHERE [Дата транзакции] BETWEEN @from AND @to
            ORDER BY [Дата транзакции]";

            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@from", from.Date);
                    cmd.Parameters.AddWithValue("@to", to.Date);
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                            list.Add(new Transaction
                            {
                                TransactionId = (int)rdr["TransactionId"],
                                Date = (DateTime)rdr["Date"],
                                Amount = (decimal)rdr["Amount"],
                                OperationType = rdr["OperationType"].ToString(),
                                Comment = rdr["Comment"].ToString(),
                                UserId = rdr["UserId"] as int?
                            });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка GetTransactions: {ex.Message}",
                                "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return list;
        }

        public Transaction GetTransactionById(int transactionId)
        {
            const string sql = @"
        SELECT
            [ID транзакции]   AS TransactionId,
            [Дата транзакции] AS Date,
            [Сумма]           AS Amount,
            [Тип операции]    AS OperationType,
            [Комментарий]     AS Comment,
            [ID пользователя] AS UserId
        FROM dbo.Transactions
        WHERE [ID транзакции] = @id";
            using (var conn = new SqlConnection(StringConnection))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", transactionId);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    return new Transaction
                    {
                        TransactionId = (int)rdr["TransactionId"],
                        Date = (DateTime)rdr["Date"],
                        Amount = (decimal)rdr["Amount"],
                        OperationType = rdr["OperationType"].ToString(),
                        Comment = rdr["Comment"].ToString(),
                        UserId = rdr["UserId"] as int?
                    };
                }
            }
        }


        /// <summary>
        /// Баланс на начало периода: учитывает все транзакции до переданной даты.
        /// </summary>
        public decimal GetStartingBalance(DateTime beforeDate)
        {
            const string sql = @"
            SELECT ISNULL(SUM(CASE 
            WHEN [Тип операции] = 'Доход'  THEN [Сумма]
            WHEN [Тип операции] = 'Расход' THEN -[Сумма]
            ELSE 0 END), 0) FROM dbo.Transactions
            WHERE [Дата транзакции] < @date";

            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@date", beforeDate.Date);
                    conn.Open();
                    return (decimal)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка GetStartingBalance: {ex.Message}",
                                "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0M;
            }
        }

        /// <summary>
        /// Текущий баланс: сумма всех транзакций во всей истории.
        /// </summary>
        public decimal GetCurrentBalance()
        {
            const string sql = @"
            SELECT ISNULL(SUM(
             CASE 
             WHEN [Тип операции] = 'Доход'  THEN [Сумма]
             WHEN [Тип операции] = 'Расход' THEN -[Сумма]
             ELSE 0 END), 0) FROM dbo.Transactions";

            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    return (decimal)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка GetCurrentBalance: {ex.Message}",
                                "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0M;
            }
        }

    public bool CreateTransaction(
    DateTime date,
    string operationType,
    decimal amount,
    string comment,
    int? userId)
        {
            const string sql = @"
        INSERT INTO Transactions
            ([Дата транзакции],[Тип операции],[Сумма],[Комментарий],[ID пользователя])
        VALUES
            (@date,@type,@amount,@comment,@userId)";
            using (var conn = new SqlConnection(StringConnection))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@type", operationType);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@comment",
                    string.IsNullOrEmpty(comment) ? DBNull.Value : (object)comment);
                cmd.Parameters.AddWithValue("@userId",
                    userId.HasValue ? (object)userId.Value : DBNull.Value);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool UpdateTransaction(
            int transactionId,
            DateTime date,
            string operationType,
            decimal amount,
            string comment,
            int? userId)
        {
            const string sql = @"
        UPDATE Transactions
           SET [Дата транзакции] = @date,
               [Тип операции]    = @type,
               [Сумма]           = @amount,
               [Комментарий]     = @comment,
               [ID пользователя] = @userId
         WHERE [ID транзакции]   = @id";
            using (var conn = new SqlConnection(StringConnection))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", transactionId);
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@type", operationType);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@comment",
                    string.IsNullOrEmpty(comment) ? DBNull.Value : (object)comment);
                cmd.Parameters.AddWithValue("@userId",
                    userId.HasValue ? (object)userId.Value : DBNull.Value);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteTransaction(int transactionId)
        {
            const string sql = @"
            DELETE FROM dbo.Transactions
            WHERE [ID транзакции] = @id";
            using (var conn = new SqlConnection(StringConnection))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", transactionId);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>Справочник активных типов подписок</summary>
        public List<SubscriptionType> GetSubscriptionTypes()
        {
            var list = new List<SubscriptionType>();
            const string sql = @"
            SELECT 
                    [ID типа подписки]    AS SubscriptionTypeId,
                    [Название]            AS Name,
                    [Описание]            AS Description,
                    [Цена]                AS Price,
                    [Длительность_дней]   AS DurationDays,
                    [Дата создания]       AS CreatedAt
                  FROM dbo.[SubscriptionType]
                 ORDER BY [Название]";
            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new SubscriptionType
                            {
                                SubscriptionTypeId = rdr.GetInt32(0),
                                Name = rdr.GetString(1),
                                Description = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                                Price = rdr.GetDecimal(3),
                                DurationDays = rdr.GetInt32(4),
                                CreatedAt = rdr.GetDateTime(5),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка GetSubscriptionTypes: {ex.Message}", "Ошибка БД",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return list;
        }

        /// <summary>
        /// Возвращает оформлённые подписки за период [from..to];
        /// если typeFilter != null — фильтрует по SubscriptionTypeId.
        /// </summary>
        public List<UserSubscription> GetUserSubscriptions(DateTime from, DateTime to, int? typeFilter)
        {
            var list = new List<UserSubscription>();
            const string sql = @"
               SELECT
                    [ID подписки]          AS UserSubscriptionId,
                    [ID пользователя]      AS UserId,
                    [ID типа подписки]     AS SubscriptionTypeId,
                    [Дата начала]          AS StartDate,
                    [Дата окончания]       AS EndDate,
                    [Автопродление]        AS IsActive
                  FROM dbo.[UserSubscription]
                 WHERE [Дата начала] BETWEEN @from AND @to
                   AND (@typeFilter IS NULL OR [ID типа подписки] = @typeFilter)
                 ORDER BY [Дата начала]";
            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@from", from.Date);
                    cmd.Parameters.AddWithValue("@to", to.Date);
                    cmd.Parameters.AddWithValue("@typeFilter", (object)typeFilter ?? DBNull.Value);
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            list.Add(new UserSubscription
                            {
                                UserSubscriptionId = rdr.GetInt32(0),
                                UserId = rdr.GetInt32(1),
                                SubscriptionTypeId = rdr.GetInt32(2),
                                StartDate = rdr.GetDateTime(3),
                                EndDate = rdr.GetDateTime(4),
                                IsActive = rdr.GetBoolean(5),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка GetUserSubscriptions: {ex.Message}", "Ошибка БД",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return list;
        }

        //// <summary>Возвращает оформленную подписку по её идентификатору или null, если не найдена</summary>
        public UserSubscription GetUserSubscriptionById(int subscriptionId)
        {
            const string sql = @"
           SELECT
                    [ID подписки]          AS UserSubscriptionId,
                    [ID пользователя]      AS UserId,
                    [ID типа подписки]     AS SubscriptionTypeId,
                    [Дата начала]          AS StartDate,
                    [Дата окончания]       AS EndDate,
                    [Автопродление]        AS IsActive
                  FROM dbo.[UserSubscription]
                WHERE [ID подписки] = @id";
            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", subscriptionId);
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (!rdr.Read()) return null;
                        return new UserSubscription
                        {
                            UserSubscriptionId = rdr.GetInt32(0),
                            UserId = rdr.GetInt32(1),
                            SubscriptionTypeId = rdr.GetInt32(2),
                            StartDate = rdr.GetDateTime(3),
                            EndDate = rdr.GetDateTime(4),
                            IsActive = rdr.GetBoolean(5)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка GetUserSubscriptionById: {ex.Message}", "Ошибка БД",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }



        /// <summary>Добавить подписку пользователю</summary>
        public bool CreateUserSubscription(int userId, int typeId, DateTime startDate, DateTime endDate, bool autoRenew)
        {
            const string sql = @"
                INSERT INTO dbo.[UserSubscription]
                    ([ID пользователя],[ID типа подписки],[Дата начала],[Дата окончания],[Автопродление])
                VALUES
                    (@uid, @tid, @start, @end, @auto)";
            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@tid", typeId);
                    cmd.Parameters.AddWithValue("@start", startDate.Date);
                    cmd.Parameters.AddWithValue("@end", endDate.Date);
                    cmd.Parameters.AddWithValue("@auto", autoRenew);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка CreateUserSubscription: {ex.Message}", "Ошибка БД",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>Обновить подписку</summary>
        public bool UpdateUserSubscription(int subscriptionId, DateTime startDate, DateTime endDate, bool autoRenew)
        {
            const string sql = @"
                UPDATE dbo.[UserSubscription]
                   SET [Дата начала]    = @start,
                       [Дата окончания] = @end,
                       [Автопродление]  = @auto
                 WHERE [ID подписки]   = @id";
            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", subscriptionId);
                    cmd.Parameters.AddWithValue("@start", startDate.Date);
                    cmd.Parameters.AddWithValue("@end", endDate.Date);
                    cmd.Parameters.AddWithValue("@auto", autoRenew);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка UpdateUserSubscription: {ex.Message}", "Ошибка БД",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>Удалить подписку</summary>
        public bool DeleteUserSubscription(int subscriptionId)
        {
            const string sql = @"
                 DELETE FROM dbo.[UserSubscription]
                 WHERE [ID подписки] = @id";
            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", subscriptionId);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка DeleteUserSubscription: {ex.Message}", "Ошибка БД",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>Создать новый тип подписки</summary>
        public bool CreateSubscriptionType(string name, string description, decimal price, int durationDays)
        {
            const string sql = @"
                INSERT INTO dbo.[SubscriptionType]
                    ([Название],[Описание],[Цена],[Длительность_дней])
                VALUES
                    (@name,@desc,@price,@days)";
            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@desc", (object)description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@days", durationDays);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка CreateSubscriptionType: {ex.Message}", "Ошибка БД",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>Обновить тип подписки</summary>
        public bool UpdateSubscriptionType(int typeId, string name, string description, decimal price, int durationDays)
        {
            const string sql = @"
                UPDATE dbo.[SubscriptionType]
                   SET [Название]          = @name,
                       [Описание]          = @desc,
                       [Цена]              = @price,
                       [Длительность_дней] = @days
                 WHERE [ID типа подписки] = @id";
            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", typeId);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@desc", (object)description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@days", durationDays);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка UpdateSubscriptionType: {ex.Message}", "Ошибка БД",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>Удалить тип подписки (физически)</summary>
        public bool DeleteSubscriptionType(int typeId)
        {
            const string sql = @"
                DELETE FROM dbo.[SubscriptionType]
                 WHERE [ID типа подписки] = @id";
            try
            {
                using (var conn = new SqlConnection(StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", typeId);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка DeleteSubscriptionType: {ex.Message}", "Ошибка БД",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}

