using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    public static string ComputeHash(string rawPassword)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(rawPassword);
            byte[] hash = sha256.ComputeHash(bytes);

            // Преобразуем полный хеш в hex-строку:
            var sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));

            // Допустим, нам нужно только 10 символов:
            // С точки зрения безопасности это плохая практика,
            // но так можно уложиться в 10 символов поля.
            string hashShort = sb.ToString().Substring(0, 10);
            return hashShort;
        }
    }
}
