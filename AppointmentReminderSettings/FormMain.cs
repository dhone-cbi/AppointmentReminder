using AppointmentReminder;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AppointmentReminderSettings
{
    public partial class FormMain : Form
    {
        public const string GraphCredentialsKey = @"Software\CBI\AppointmentReminder\GraphCredentials";
        public const string GraphURL = "https://graph.microsoft.com/.default";

        public GraphServiceClient graphClient;
        public AppointmentReminderEngine appointmentReminderEngine;
        public RegistrySettings Settings = new RegistrySettings();

        public FormMain()
        {
            InitializeComponent();
            Settings.Load();
            bindingSource1.DataSource = Settings;
            txtApplicationID.DataBindings.Add("Text", bindingSource1, "GraphApplicationId");
            txtTenantID.DataBindings.Add("Text", bindingSource1, "GraphTenantId");
            txtClientSecret.DataBindings.Add("Text", bindingSource1, "GraphClientSecret");

        }

        private async void SendReminders()
        {
            List<string> recipients = new List<string> { "dneves@cbridges.com", "mallen@cbridges.com",
                "dhone@cbridges.com"};
            IEnumerable<AppointmentInfo> list = appointmentReminderEngine.GetAppointments();
            list = await appointmentReminderEngine.SendReminders(list);
            var sentList = from item in list where item.ReminderSentTime.HasValue select item;
            var failedList = from item in list where !item.ReminderSentTime.HasValue select item;
            appointmentReminderEngine.SendReminderReport(list, recipients);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            /*string applicationID = txtApplicationID.Text;
            string tenantID = txtTenantID.Text;
            string clientSecret = txtClientSecret.Text;

            byte[] applicationIdBytes = Encoding.Unicode.GetBytes(applicationID);
            byte[] tenantIdBytes = Encoding.Unicode.GetBytes(tenantID);
            byte[] clientSecretBytes = Encoding.Unicode.GetBytes(clientSecret);

            byte[] applicationIdEncrypted = 
                ProtectedData.Protect(applicationIdBytes, null, DataProtectionScope.LocalMachine);
            byte[] tenantIdEncrypted = 
                ProtectedData.Protect(tenantIdBytes, null, DataProtectionScope.LocalMachine);
            byte[] clientSecretEncrypted = 
                ProtectedData.Protect(clientSecretBytes, null, DataProtectionScope.LocalMachine);

            //string applicationIdEncryptedB64 = Convert.ToBase64String(applicationIdEncrypted);
            //string tenantIdEncryptedB64 = Convert.ToBase64String(tenantIdEncrypted);
            //string clientSecretEncryptedB64 = Convert.ToBase64String(clientSecretEncrypted);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(GraphCredentialsKey, true);

            if (key == null)
                key = Registry.CurrentUser.CreateSubKey(GraphCredentialsKey, true);

            key.SetValue("ApplicationID", applicationIdEncrypted);
            key.SetValue("TenantID", tenantIdEncrypted);
            key.SetValue("ClientSecret", clientSecretEncrypted);

            key.Close();*/

            Settings.Save();

            InitializeInterfaces();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtApplicationID.Clear();
            txtTenantID.Clear();
            txtClientSecret.Clear();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            /*RegistryKey key = Registry.CurrentUser.OpenSubKey(GraphCredentialsKey);

            if (key == null) 
                return;
            else
            {
                byte[] applicationIdBytes = (byte[])key.GetValue("ApplicationId");
                byte[] tenantIdBytes = (byte[])key.GetValue("TenantId");
                byte[] clientSecretBytes = (byte[])key.GetValue("ClientSecret");

                key.Close();

                byte[] applicationIdDecrypted = 
                    ProtectedData.Unprotect(applicationIdBytes, null, DataProtectionScope.LocalMachine);
                byte[] tenantIdDecrypted = 
                    ProtectedData.Unprotect(tenantIdBytes, null, DataProtectionScope.LocalMachine);
                byte[] clientSecretDecrypted =
                    ProtectedData.Unprotect(clientSecretBytes, null, DataProtectionScope.LocalMachine);


                string applicationID = Encoding.Unicode.GetString(applicationIdDecrypted);
                string tenantID = Encoding.Unicode.GetString(tenantIdDecrypted);
                string clientSecret = Encoding.Unicode.GetString(clientSecretDecrypted);

                txtApplicationID.Text = applicationID;
                txtTenantID.Text = tenantID;
                txtClientSecret.Text = clientSecret;*/

            Settings.Load();

            bindingSource1.ResetBindings(true);

            InitializeInterfaces();
            //}
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //btnReload_Click(sender, e);

            InitializeInterfaces();

            DateTime scheduledTime = DateTime.Today.AddHours(11);

            
            if (scheduledTime.Subtract(DateTime.Now).TotalMilliseconds < 0)
                scheduledTime = scheduledTime.AddDays(1);


            double interval = (scheduledTime - DateTime.Now).TotalMilliseconds;

            timer1.Interval = (int)interval;
            timer1.Start();

        }

        private void InitializeInterfaces()
        {
            string applicationID = Settings.GraphApplicationId;
            string tenantID = Settings.GraphTenantId;
            string clientSecret = Settings.GraphClientSecret;

            try
            {
                string[] scopes = { GraphURL };
                ClientSecretCredential clientSecretCredential = new ClientSecretCredential(tenantID, applicationID, clientSecret);
                graphClient = new GraphServiceClient(clientSecretCredential);

                appointmentReminderEngine = new AppointmentReminderEngine()
                {
                    GraphClient = graphClient
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error initializing interfaces.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Interval = 24 * 60 * 60 * 1000;

            SendReminders();
        }

        private void btnSendReminders_Click(object sender, EventArgs e)
        {
            SendReminders();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var list = appointmentReminderEngine.GetAppointments();
            MessageBox.Show($"{list.Count()}");
        }
    }
}
