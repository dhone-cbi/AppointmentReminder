﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AppointmentReminder;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.CodeDom;
using Microsoft.Graph;
using Azure.Identity;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace AppointmentReminderSettings
{
    public partial class FormMain : Form
    {
        public const string GraphCredentialsKey = @"Software\CBI\AppointmentReminder\GraphCredentials";
        public const string GraphURL = "https://graph.microsoft.com/.default";

        public GraphServiceClient graphClient;
        

        public FormMain()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            /*AppointmentInfo info = new AppointmentInfo()
            {
                AppointmentDate = "2/8/2024",
                AppointmentTime = "8:00 AM",
                Language = "Spanish",
                Address = "1234 Test Ln",
                City = "Phoenix",
                State = "AZ",
                Zip = "85021",
                CellPhone = "6233129573"
            };*/
            AppointmentReminderEngine reminderEngine = new AppointmentReminderEngine()
            {
                GraphClient = graphClient
            };
            //reminderEngine.SendReminderEmail(info);
            //info.ReminderSentTime = DateTime.Now;
            //reminderEngine.WriteReminderLog(info, null);

            IEnumerable<AppointmentInfo> list = reminderEngine.GetAppointments();
            reminderEngine.SendReminders(list);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string applicationID = txtApplicationID.Text;
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

            RegistryKey key = Registry.LocalMachine.OpenSubKey(GraphCredentialsKey, true);

            if (key == null)
                key = Registry.LocalMachine.CreateSubKey(GraphCredentialsKey, true);

            key.SetValue("ApplicationID", applicationIdEncrypted);
            key.SetValue("TenantID", tenantIdEncrypted);
            key.SetValue("ClientSecret", clientSecretEncrypted);

            key.Close();

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
            RegistryKey key = Registry.LocalMachine.OpenSubKey(GraphCredentialsKey);

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
                txtClientSecret.Text = clientSecret;

                InitializeInterfaces();
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            btnReload_Click(sender, e);

            InitializeInterfaces();
        }

        private void InitializeInterfaces()
        {
            string applicationID = txtApplicationID.Text;
            string tenantID = txtTenantID.Text;
            string clientSecret = txtClientSecret.Text;

            string[] scopes = { GraphURL };
            ClientSecretCredential clientSecretCredential = new ClientSecretCredential(tenantID, applicationID, clientSecret);
            graphClient = new GraphServiceClient(clientSecretCredential);
        }
    }
}
