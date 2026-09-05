using Npgsql;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Launcher.Services.Database
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string host, int port, string database, string user, string password)
        {
            _connectionString = string.Format(
                "Host={0};Port={1};Database={2};Username={3};Password={4};",
                host, port, database, user, password);
        }

        public bool ValidateLogin(string username, string passwordMd5)
        {
            string token;
            return TryLogin(username, passwordMd5, out token);
        }

        /// <summary>
        /// Valida login e devolve o token da conta (usado pelo client 122 BR com /token).
        /// Aceita senha MD5, senha em texto puro ou senha vazia no banco.
        /// </summary>
        public bool TryLogin(string username, string passwordMd5, out string token)
        {
            token = null;
            using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(
                    "SELECT token, password FROM accounts WHERE username = @u LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("u", username);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        string dbPass = reader["password"] != null ? reader["password"].ToString() : string.Empty;
                        string dbToken = reader["token"] != null ? reader["token"].ToString() : string.Empty;

                        bool ok =
                            string.IsNullOrEmpty(dbPass) ||
                            string.Equals(dbPass, passwordMd5, StringComparison.OrdinalIgnoreCase) ||
                            // banco com senha em texto puro (ex.: README player1/player1)
                            string.Equals(ComputeMd5(dbPass), passwordMd5, StringComparison.OrdinalIgnoreCase);

                        if (!ok)
                            return false;

                        token = dbToken;
                        return !string.IsNullOrEmpty(token);
                    }
                }
            }
        }

        private static string ComputeMd5(string input)
        {
            if (input == null)
                input = string.Empty;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
