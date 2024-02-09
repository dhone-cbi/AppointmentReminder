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


        private void OpenSqlConnection()
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

        private void CloseSqlConnection()
        {
            sqlConnection.Close();
        }

        public AppointmentInfo ReadAppointmentInfo(SqlDataReader reader)
        {
            AppointmentInfo info = new AppointmentInfo();

            
            info.PersonID = (Guid)reader["person_id"];
            info.AppointmentID = (Guid)reader["appt_id"];
            info.CellPhone = (string)reader["cell_phone"];
            info.AppointmentDate = (string)reader["appt_date"];
            info.AppointmentTime = (string)reader["appt_time"];
            info.Address = (string)reader["address"];
            info.City = (string)reader["city"];
            info.State = (string)reader["state"];
            info.Zip = (string)reader["zip"];
            info.Language = (string)reader["language"];


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

        public async void SendReminders(IEnumerable<AppointmentInfo> list)
        {
            OpenSqlConnection();
            foreach (var item in list)
            {
                DateTime sentTime = await SendReminderEmail(item);

                item.ReminderSentTime = sentTime;

                WriteReminderLog(item);
            }
            CloseSqlConnection();
        }


        public int WriteReminderLog(AppointmentInfo info, SqlTransaction transaction = null)
        {
            //OpenSqlConnection();
            SqlCommand command = sqlConnection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = "insert into CBI_ApptReminders.dbo.ApptReminderLog (" +
                "   person_id, appt_id, cell_phone, appt_date, appt_time, address, city, state, zip, language, reminder_sent_time" +
                ")" +
                "values (" +
                "   @person_id, @appt_id, @cell_phone, @appt_date, @appt_time, @address, @city, @state, @zip, @language, @reminder_sent_time" +
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
            command.Parameters.AddWithValue("language", info.Language);

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
                    "No responda a este mensaje . Si necesita cambiar o cancelar su cita, por favor, " +
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
    }
}
