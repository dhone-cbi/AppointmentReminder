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
using AppointmentReminderSettings;

namespace AppointmentReminderService
{
    public partial class ReminderService : ServiceBase
    {

        Timer timer;
        EventLog eventLog;
        RegistrySettings settings = new RegistrySettings();

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
            timer.Elapsed += TimerElapsed;

            eventLog = new EventLog();
            /*if (!EventLog.SourceExists("CBI Appointment Reminder"))
            {
                EventLog.CreateEventSource("CBI Appointment Reminder", "Application");
            }*/
            eventLog.Source = "ReminderService";
            eventLog.Log = "Application";

            settings.Load();

            eventLog.WriteEntry("Settings loaded successfully.", EventLogEntryType.Information);
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            eventLog.WriteEntry("Timer Elapsed", EventLogEntryType.Information);
            timer.Interval = 24 * 60 * 60 * 1000.0;
        }

        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("Appointment Reminder Service Started", EventLogEntryType.Information);
            timer.Start();
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("Appointment Reminder Service Stopped", EventLogEntryType.Information);
        }
    }
}
