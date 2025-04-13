using MyClub;
using System;
using System.Data.SqlClient;

namespace MyClub
{
    public class PersonalData
    {
        // Данные аутентификации и профиля
        public static int UserId { get; private set; }
        public static string Login { get; private set; }
        public static string Role { get; private set; }
        public static string Email { get; private set; }
        public static string LastName { get; private set; }
        public static string FirstName { get; private set; }
        public static string FatherName { get; private set; }
        public static DateTime? DateBirth { get; private set; }
        public static string PhoneNumber { get; private set; }
        public static string Password { get; private set; }

        /// <summary>
        /// Устанавливает личные данные пользователя, используя данные аутентификации.
        /// Метод проверяет, соответствует ли введённый логин и пароль данным в UsersAuth, 
        /// и, если да, загружает профильную информацию из UsersProfile.
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">
        /// Пароль пользователя, который перед хешированием должен быть обработан (в данном примере хеширование происходит на SQL-стороне, 
        /// но рекомендуется хешировать пароль уже в приложении)
        /// </param>
        /// <returns>Возвращает true, если данные успешно получены, иначе false.</returns>
        public bool SetPersonalData(string login, string password)
        {
            var db = new DB();

            // Запрос объединяет таблицы UsersAuth и UsersProfile по ID пользователя.
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
                        if (reader.HasRows)
                        {
                            reader.Read();
                            UserId = Convert.ToInt32(reader["UserId"]);
                            Login = reader["Login"].ToString();
                            Role = reader["Role"].ToString();
                            Email = reader["Email"].ToString();
                            LastName = reader["LastName"].ToString();
                            FirstName = reader["FirstName"].ToString();
                            FatherName = reader["FatherName"].ToString();
                            Password = reader["Password"].ToString();

                            // Если значение даты рождения не NULL, преобразуем в DateTime
                            if (reader["DateBirth"] != DBNull.Value)
                                DateBirth = Convert.ToDateTime(reader["DateBirth"]);

                            PhoneNumber = reader["PhoneNumber"].ToString();
                            return true;
                        }
                    }
                    return false;
                }
            }
        }
    }
}
