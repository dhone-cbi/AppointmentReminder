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


        public AppointmentReminderEngine() 
        { 
        }

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


            sqlConnection.Close();

            return list;
        }

    }
}
