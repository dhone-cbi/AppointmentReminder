using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentReminder
{
    public class AppointmentReminderEngine
    {
        private SqlConnection sqlConnection;

        public SqlConnection SqlConnection { get { return sqlConnection; } }

        public GraphServiceClient GraphClient { get; set; }


        public void OpenSqlConnection()
        {
            sqlConnection = new SqlConnection();
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
            sb.IntegratedSecurity = true;
            sb.TrustServerCertificate = true;
            sb.DataSource = "NGSQLPROD";

            sqlConnection.ConnectionString = sb.ConnectionString;

            sqlConnection.Open();
            sqlConnection.ChangeDatabase("CBIProd");
        }

        public void CloseSqlConnection()
        {
            sqlConnection.Close();
        }

        public AppointmentInfo ReadAppointmentInfo(SqlDataReader reader)
        {
            AppointmentInfo info = new AppointmentInfo();

            
            info.PersonID = (Guid)reader["person_id"];
            info.AppointmentID = (Guid)reader["appt_id"];
            info.CellPhone = (string)reader["cell_phone"];
            info.AppointmentDate = (string)reader["ApptDate"];
            info.AppointmentTime = (string)reader["ApptTime"];
            info.Address = (string)reader["address_line_1"];
            info.City = (string)reader["city"];
            info.State = (string)reader["state"];
            info.Zip = (string)reader["zip"];
            info.Language = (string)reader["SpanishSpeaking"];


            return info;
        }
        public IEnumerable<AppointmentInfo> GetAppointments()
        {
            List<AppointmentInfo> list = new List<AppointmentInfo>();            
            
            OpenSqlConnection();

            SqlCommand command = sqlConnection.CreateCommand();

            command.CommandText = "select * from vw_GetUpcomingReminders";

            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                AppointmentInfo info = ReadAppointmentInfo(reader);
                list.Add(info);
            }

            CloseSqlConnection();

            return list;
        }

        public async Task<IEnumerable<AppointmentInfo>> SendReminders(IEnumerable<AppointmentInfo> list)
        {
            OpenSqlConnection();
            foreach (var item in list)
            {
                DateTime sentTime = await SendReminderEmail(item);

                item.ReminderSentTime = sentTime;

                WriteReminderLog(item);
            }
            CloseSqlConnection();

            return list;
        }


        public int WriteReminderLog(AppointmentInfo info, SqlTransaction transaction = null)
        {
            //OpenSqlConnection();
            SqlCommand command = sqlConnection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = "insert into CBI_ApptReminders.dbo.ApptReminder (" +
                "   person_id, appt_id, cell_phone, appt_date, ApptTime, address_line_1, city, state, zip, ReminderSentTime" +
                ")" +
                "values (" +
                "   @person_id, @appt_id, @cell_phone, @appt_date, @appt_time, @address, @city, @state, @zip, @reminder_sent_time" +
                ")";

            command.Parameters.AddWithValue("person_id", info.PersonID);
            command.Parameters.AddWithValue("appt_id", info.AppointmentID);
            command.Parameters.AddWithValue("cell_phone", info.CellPhone);
            command.Parameters.AddWithValue("appt_date", info.AppointmentDate);
            command.Parameters.AddWithValue("appt_time", info.AppointmentTime);
            command.Parameters.AddWithValue("address", info.Address);
            command.Parameters.AddWithValue("city", info.City);
            command.Parameters.AddWithValue("state", info.State);
            command.Parameters.AddWithValue("zip", info.Zip);
            //command.Parameters.AddWithValue("language", info.Language);

            if (info.ReminderSentTime.HasValue)
                command.Parameters.AddWithValue("reminder_sent_time", info.ReminderSentTime);
            else
                command.Parameters.AddWithValue("reminder_sent_time", DBNull.Value);

            int rowsModified = command.ExecuteNonQuery();

            //CloseSqlConnection();

            return rowsModified;
        }

        public async Task<DateTime> SendReminderEmail(AppointmentInfo info)
        {
            string reminderText;
            string reminderSubject;
            if (string.Compare(info.Language, "Spanish", true) == 0)
            {
                reminderText = $"Este es un recordatorio que tienes una cita el " +
                    $"{info.AppointmentDate} {info.AppointmentTime} en {info.Address}, {info.City}, " +
                    $"{info.State} {info.Zip}. " +
                    "No responda a este mensaje. Si necesita cambiar o cancelar su cita, por favor, " +
                    "llame al 1-877-931-9142.";
                reminderSubject = "Recordatorio de Cita";

            }
            else
            {
                reminderText = $"This is a reminder that you have an appointment scheduled on " +
                    $"{info.AppointmentDate} {info.AppointmentTime} at {info.Address}, {info.City}, " +
                    $"{info.State} {info.Zip}. " +
                    $"Do not reply to this message. If you need to change or cancel your appointment, " +
                    $"please call us at 1-877-931-9142.";
                reminderSubject = "Appointment Reminder";

            }

            ItemBody body = new ItemBody
            {
                ContentType = BodyType.Text,
                Content = reminderText
            };

            Message msg = new Message()
            {
                Subject = reminderSubject,
                ToRecipients = new List<Recipient>
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress { Address = $"{info.CellPhone}@cvzvmg.biz" }
                    }
                },
                BccRecipients = new List<Recipient>
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress { Address = "dhone@cbridges.com" }
                    }
                },
                Sender = new Recipient
                {
                    EmailAddress = new EmailAddress { Address = "no_reply@cbridges.com" }
                },
                Body = body

            };

            SendMailPostRequestBody postRequestBody = new SendMailPostRequestBody
            {
                Message = msg
            };

            await GraphClient.Users["no_reply@cbridges.com"].SendMail.PostAsync(postRequestBody);

            return DateTime.Now;
        }

        public async void SendReminderReport(IEnumerable<AppointmentInfo> list, IEnumerable<string> recipients)
        {
            int sentReminders = list.Count(item => item.ReminderSentTime.HasValue);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<h1>Appointment Reminder Report</h1>");
            sb.AppendLine("<p>");
            sb.AppendLine($"<b>Reminders Sent:</b> {sentReminders}");
            sb.AppendLine("</p>");

            ItemBody body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = sb.ToString()
            };

            Message msg = new Message()
            {
                Subject = "Appointment Reminder Report",
                ToRecipients = new List<Recipient>(
                    from item in recipients select new Recipient
                    {
                        EmailAddress = new EmailAddress { Address = item }
                    }
                    ),
                Sender = new Recipient
                {
                    EmailAddress = new EmailAddress { Address = "no_reply@cbridges.com" }
                },
                Body = body

            };

            SendMailPostRequestBody postRequestBody = new SendMailPostRequestBody
            {
                Message = msg
            };

            await GraphClient.Users["no_reply@cbridges.com"].SendMail.PostAsync(postRequestBody);

        }
    }
}
