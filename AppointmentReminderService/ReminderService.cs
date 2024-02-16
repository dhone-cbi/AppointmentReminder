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
using AppointmentReminder;
using AppointmentReminderSettings;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace AppointmentReminderService
{
    public partial class ReminderService : ServiceBase
    {

        Timer timer;
        EventLog eventLog;
        RegistrySettings settings = new RegistrySettings();
        GraphServiceClient graphClient;
        AppointmentReminderEngine appointmentReminderEngine;

        public ReminderService()
        {
            InitializeComponent();

            timer = new Timer();

            DateTime scheduledTime = DateTime.Today.AddHours(11);

            if (scheduledTime.Subtract(DateTime.Now).TotalMilliseconds < 0)
                scheduledTime = scheduledTime.AddDays(1);


            double interval = (scheduledTime - DateTime.Now).TotalMilliseconds;

            //timer.Interval = interval;
            timer.Interval = 30 * 1000.0;
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

            InitializeGraphClient();
        }

        public void InitializeGraphClient()
        {
            string applicationId = settings.GraphApplicationId;
            string tenantId = settings.GraphTenantId;
            string clientSecret = settings.GraphClientSecret;

            try
            {
                ClientSecretCredential clientSecretCredential =
                    new ClientSecretCredential(tenantId, applicationId, clientSecret);
                graphClient = new GraphServiceClient(clientSecretCredential);
                appointmentReminderEngine = new AppointmentReminderEngine
                {
                    GraphClient = graphClient
                };
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry("Could not initialize interfaces. Please check credentials.");

                throw new Exception("Could not initialize interfaces. Please check credentials.", ex);
            }
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            List<string> recipients = new List<string> { "dneves@cbridges.com", "mallen@cbridges.com",
                "dhone@cbridges.com"};

            eventLog.WriteEntry("Timer Elapsed", EventLogEntryType.Information);
            //timer.Interval = 24 * 60 * 60 * 1000.0;
            timer.Interval = 30 * 1000.0;

            IEnumerable<AppointmentInfo> list = appointmentReminderEngine.GetAppointments();

            eventLog.WriteEntry($"Successfully retrieved {list.Count()} appointments", EventLogEntryType.Information);
            /*list = await appointmentReminderEngine.SendReminders(list);
            var sentList = from item in list where item.ReminderSentTime.HasValue select item;
            var failedList = from item in list where !item.ReminderSentTime.HasValue select item;
            appointmentReminderEngine.SendReminderReport(list, recipients);*/
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
