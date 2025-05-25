using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace MyClub
{
    public class PersonalData
    {
        public static PersonalData Current { get; set; }

        // Данные аутентификации и профиля
        public int UserId { get;  set; }
        public string Login { get;  set; }
        public string Role { get;  set; }
        public string Password { get;  set; }
        public string Email { get;  set; }
        public string LastName { get;  set; }
        public string FirstName { get;  set; }
        public string FatherName { get;  set; }
        public DateTime? DateBirth { get;  set; }
        public string PhoneNumber { get;  set; }
        public byte[] Photo { get;  set; }


        /// <summary>
        /// Загружает данные пользователя по логину и паролю.
        /// Пароль хранится в открытом виде.
        /// </summary>
        public bool SetPersonalData(string login, string password)
        {
            var db = new DB();
            string sqlExpression = @"
                SELECT TOP 1
                    ua.[ID пользователя] AS UserId,
                    ua.[Логин] AS Login,
                    ua.[Роль] AS Role,
                    ua.[Пароль] AS Password,
                    up.[Электронная почта] AS Email,
                    up.[Фамилия] AS LastName,
                    up.[Имя] AS FirstName,
                    up.[Отчество] AS FatherName,
                    up.[Дата рождения] AS DateBirth,
                    up.[Номер телефона] AS PhoneNumber,
                    up.[Фото] AS Photo
                FROM UsersAuth ua
                INNER JOIN UsersProfile up
                    ON ua.[ID пользователя] = up.[ID пользователя]
                WHERE ua.[Логин] = @Login AND ua.[Пароль] = @Password
            ";

            using (SqlConnection connection = new SqlConnection(db.StringConnection))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlExpression, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            UserId = Convert.ToInt32(reader["UserId"]);
                            Login = reader["Login"].ToString();
                            Role = reader["Role"].ToString();
                            Password = reader["Password"].ToString();
                            Email = reader["Email"].ToString();
                            LastName = reader["LastName"].ToString();
                            FirstName = reader["FirstName"].ToString();
                            FatherName = reader["FatherName"].ToString();
                            if (reader["DateBirth"] != DBNull.Value)
                                DateBirth = Convert.ToDateTime(reader["DateBirth"]);
                            PhoneNumber = reader["PhoneNumber"].ToString();

                            if (reader["Photo"] != DBNull.Value)
                                Photo = (byte[])reader["Photo"];
                            else
                                Photo = null;

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void UpdateCredentials(string newLogin, string newPassword)
        {
            Login = newLogin;
            Password = newPassword;
        }


        public void UpdateProfile(string newLastName, string newFirstName, string newFatherName, DateTime? newDateBirth, string newPhoneNumber, string newEmail)
        {

            LastName = newLastName;
            FirstName = newFirstName;
            FatherName = newFatherName;
            DateBirth = newDateBirth;
            PhoneNumber = newPhoneNumber;
            Email = newEmail;
        }

        public void UpdatePhoto(byte[] newPhoto)
        {
            Photo = newPhoto;
        }

    /// <summary>
    /// Возвращает список всех пользователей (объединённый JOIN из UsersAuth + UsersProfile).
    /// </summary>
    public static List<PersonalData> GetAllUsers()
        {
            var list = new List<PersonalData>();
            var db = new DB();
            string sql = @"
            SELECT
                ua.[ID пользователя] AS UserId,
                ua.[Логин]      AS Login,
                ua.[Роль]       AS Role,
                ua.[Пароль]     AS Password,
                up.[Электронная почта] AS Email,
                up.[Фамилия]    AS LastName,
                up.[Имя]        AS FirstName,
                up.[Отчество]   AS FatherName,
                up.[Дата рождения]   AS DateBirth,
                up.[Номер телефона]  AS PhoneNumber,
                up.[Фото]             AS Photo
            FROM UsersAuth ua
            INNER JOIN UsersProfile up
              ON ua.[ID пользователя] = up.[ID пользователя];
        ";
            using (var conn = new SqlConnection(db.StringConnection))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var allData = new PersonalData();
                        allData.UserId = Convert.ToInt32(rdr["UserId"]);
                        allData.Login = rdr["Login"].ToString();
                        allData.Role = rdr["Role"].ToString();
                        allData.Password = rdr["Password"].ToString();
                        allData.Email = rdr["Email"].ToString();
                        allData.LastName = rdr["LastName"].ToString();
                        allData.FirstName = rdr["FirstName"].ToString();
                        allData.FatherName = rdr["FatherName"].ToString();
                        allData.DateBirth = rdr["DateBirth"] != DBNull.Value
                                            ? (DateTime?)rdr["DateBirth"]
                                            : null;
                        allData.PhoneNumber = rdr["PhoneNumber"].ToString();
                        allData.Photo = rdr["Photo"] != DBNull.Value
                                            ? (byte[])rdr["Photo"]
                                            : null;
                        list.Add(allData);
                    }
                }
            }
            return list;
        }

            public bool SetPersonalDataById(int userId)
            {
                var db = new DB();
                string sql = @"
            SELECT
                ua.[ID пользователя] AS UserId,
                ua.[Логин]            AS Login,
                ua.[Роль]             AS Role,
                ua.[Пароль]           AS Password,
                up.[Электронная почта]AS Email,
                up.[Фамилия]          AS LastName,
                up.[Имя]              AS FirstName,
                up.[Отчество]         AS FatherName,
                up.[Дата рождения]    AS DateBirth,
                up.[Номер телефона]   AS PhoneNumber,
                up.[Фото]             AS Photo
            FROM UsersAuth ua
            INNER JOIN UsersProfile up
              ON ua.[ID пользователя] = up.[ID пользователя]
            WHERE ua.[ID пользователя] = @UserId;";
                using (var conn = new SqlConnection(db.StringConnection))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (!rdr.Read()) return false;

                    this.UserId = userId;

                    // Перезаписываем все поля модели:
                    Login = rdr["Login"].ToString();
                        Password = rdr["Password"].ToString();
                        Role = rdr["Role"].ToString();
                        Email = rdr["Email"].ToString();
                        LastName = rdr["LastName"].ToString();
                        FirstName = rdr["FirstName"].ToString();
                        FatherName = rdr["FatherName"].ToString();
                        DateBirth = rdr["DateBirth"] != DBNull.Value
                                    ? (DateTime?)rdr["DateBirth"]
                                    : null;
                        PhoneNumber = rdr["PhoneNumber"].ToString();
                        Photo = rdr["Photo"] != DBNull.Value
                                    ? (byte[])rdr["Photo"]
                                    : null;

                        return true;
                    }
                }
            }


    }
}