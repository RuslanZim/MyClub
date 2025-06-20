using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyClub
{
    public static class RegistrationValidator
    {
        /// <summary>
        /// Проверяет, что все переданные поля заполнены (не пустые или состоящие только из пробелов).
        /// </summary>
        /// <param name="fields">Набор строковых полей</param>
        /// <returns>True, если все поля заполнены; иначе false.</returns>
        public static bool ValidateNonEmpty(params string[] fields)
        {
            return fields.All(field => !string.IsNullOrWhiteSpace(field));
        }

        /// <summary>
        /// Проверяет корректность пароля:
        /// - Пароль не пустой;
        /// - Пароли совпадают;
        /// - Длина от 8 до 32 символов;
        /// - Содержит хотя бы одну цифру;
        /// - Содержит хотя бы одну заглавную букву;
        /// - Содержит хотя бы одну строчную букву;
        /// - Содержит хотя бы один спец. символ.
        /// Возвращает false и сообщение об ошибке, если проверка не пройдена.
        /// </summary>
        /// <param name="password">Введённый пароль</param>
        /// <param name="confirmPassword">Подтверждение пароля</param>
        /// <param name="error">Сообщение об ошибке валидации</param>
        /// <returns>True, если пароль корректен, иначе false.</returns>
        public static bool ValidatePassword(string password, string confirmPassword, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(password))
            {
                error = "Поле с паролем пустое.";
                return false;
            }
            if (password != confirmPassword)
            {
                error = "Пароли не совпадают.";
                return false;
            }

            var hasLength = new Regex(@"^.{8,32}$");
            var hasDigit = new Regex(@"[0-9]+");
            var hasUpper = new Regex(@"[A-Z]+");
            var hasLower = new Regex(@"[a-z]+");
            var hasSpecial = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+");

            if (!hasLength.IsMatch(password))
            {
                error = "Пароль должен быть от 8 до 32 символов.";
                return false;
            }
            if (!hasDigit.IsMatch(password))
            {
                error = "Пароль должен содержать хотя бы одну цифру.";
                return false;
            }
            if (!hasUpper.IsMatch(password))
            {
                error = "Пароль должен содержать хотя бы одну заглавную букву.";
                return false;
            }
            if (!hasLower.IsMatch(password))
            {
                error = "Пароль должен содержать хотя бы одну строчную букву.";
                return false;
            }
            if (!hasSpecial.IsMatch(password))
            {
                error = "Пароль должен содержать хотя бы один спец. символ.";
                return false;
            }
            return true;
        }
    }
}
