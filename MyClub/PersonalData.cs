using System;
using System.Data.SqlClient;

namespace MyClub
{
    public class PersonalData
    {
        public static PersonalData Current { get; set; }

        // Данные аутентификации и профиля
        public int UserId { get; private set; }
        public string Login { get; private set; }
        public string Role { get; private set; }
        public string Password { get; private set; }
        public string Email { get; private set; }
        public string LastName { get; private set; }
        public string FirstName { get; private set; }
        public string FatherName { get; private set; }
        public DateTime? DateBirth { get; private set; }
        public string PhoneNumber { get; private set; }

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
                    up.[Номер телефона] AS PhoneNumber
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
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

