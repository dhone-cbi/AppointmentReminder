using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentReminder
{
    public class AppointmentInfo
    {
        public Guid PersonID { get; set; }
        public Guid AppointmentID { get; set; }

        public string CellPhone { get; set; }
        public string AppointmentDate { get; set; }
        public string AppointmentTime { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Language { get; set; }
        public DateTime? ReminderSentTime { get; set; }

    }
}
