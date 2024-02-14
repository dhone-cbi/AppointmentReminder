using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AppointmentReminderService
{
    public partial class ReminderService : ServiceBase
    {

        Timer timer;
        public ReminderService()
        {
            InitializeComponent();
            timer = new Timer();

            DateTime scheduledTime = DateTime.Today.AddHours(11);

            if (scheduledTime.Subtract(DateTime.Now).TotalMilliseconds < 0)
                scheduledTime = scheduledTime.AddDays(1);


            double interval = (scheduledTime - DateTime.Now).TotalMilliseconds;

            timer.Interval = interval;
            timer.AutoReset = true;
        }

        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("Appointment Reminder Service Started", EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("Appointment Reminder Service Stopped", EventLogEntryType.Information);
        }
    }
}
