using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Windows;

namespace NetworkProg_AppList._4_SMTP.View
{
    /// <summary>
    /// Interaction logic for SmtpWindow.xaml
    /// </summary>
    public partial class SmtpWindow : Window
    {
        private SqlConnection _sqlConnection;
        Random _random = new();
        private dynamic? _email;

        public SmtpWindow()
        {
            InitializeComponent();
        }



        private void CloseWindowError(string message)
        {
            MessageBox.Show(message);
            this.Close();
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _email = JsonSerializer.Deserialize<dynamic>(File.ReadAllText("4_SMTP\\emailconfig.json"));
            if (_email is null) CloseWindowError("Email config load error");

            var database = JsonSerializer.Deserialize<dynamic>(File.ReadAllText("4_SMTP\\database.json"));
            if (database is null) CloseWindowError("Email config load error");

            try
            {
                _sqlConnection = new SqlConnection(database.GetProperty("ConnectionString").GetString());
                _sqlConnection.Open();
            }
            catch(Exception ex) { CloseWindowError("DB connection error " + ex.Message); }
        }

        private void SendMailButton_Click(object sender, RoutedEventArgs e)
        {
            if (_email is null) return;

            int validationCode = _random.Next(100000, 1000000);
            bool mailExists = false;

            using (var sqlCheckExistingEmailCommand = new SqlCommand(
                $"SELECT code FROM email_codes WHERE email = '{ValidationEmailTextBox.Text}'",
                _sqlConnection))
            {
                string result = sqlCheckExistingEmailCommand.ExecuteScalar().ToString();
                if (result == "000000")
                {
                    MessageBox.Show("Почта уже зарегестрирована!");
                    return;
                }
                else if (result is not null)
                {
                    mailExists = true;
                }
            }

            JsonElement smtp = _email.GetProperty("smtp");
            string host = smtp.GetProperty("host").GetString()!;
            int port = smtp.GetProperty("port").GetInt32();
            string mailbox = smtp.GetProperty("email").GetString()!;
            string password = smtp.GetProperty("password").GetString()!;
            bool ssl = smtp.GetProperty("ssl").GetBoolean();

            using var smtpClient = new SmtpClient(host)
            {
                Port = port,
                EnableSsl = ssl,
                Credentials = new NetworkCredential(mailbox, password)
            };
            smtpClient.Send(mailbox, RecipientEmailTextBox.Text,
                                     EmailSubjectTextBox.Text,
                                     EmailBodyTextBox.Text + validationCode);

            if (!mailExists)
            {
                using var sqlCommand = new SqlCommand(
                    $"INSERT INTO email_codes(email, code) VALUES(N'{ValidationEmailTextBox.Text}', '{validationCode}')",
                    _sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            else
            {
                using var sqlCommand = new SqlCommand(
                    $"UPDATE email_codes SET code = '{validationCode}' WHERE email = N'{ValidationEmailTextBox.Text}'",
                    _sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }

            MessageBox.Show("Код подтверждения выслан.");
        }

        private void AcceptCodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sqlConnection is null) return;

            string resultMessage = String.Empty;
            string? validationCode;
            string email = ValidationEmailTextBox.Text;

            using var sqlCommand = new SqlCommand(
                $"SELECT code FROM email_codes WHERE email = N'{email}'",
                _sqlConnection);
            try
            {
                validationCode = Convert.ToString(sqlCommand.ExecuteScalar());
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return; }

            if (validationCode == String.Empty)     // Нет почты в БД - возврат NULL 
            {
                resultMessage = "Введёная почта не зарегистрирована";
            }
            else if (validationCode == "000000")    // Почта уже подтверждена
            {
                resultMessage = "Введёная почта уже подтверждена";
            }
            else if (validationCode == ValidationCodeTextBox.Text)
            {
                resultMessage = "Введенная почта успешно подтверждена";

                // Сброс кода в БД - установление значений 000000
                using var sqlCommand2 = new SqlCommand(
                    $"UPDATE email_codes SET code = '000000' WHERE email = N'{email}'", _sqlConnection);
                sqlCommand2.ExecuteNonQuery();
            }
            else
            {
                resultMessage = "Код введён неправильно";
            }

            MessageBox.Show(resultMessage);
        }
    }
}
