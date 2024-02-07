using System;
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

namespace AppointmentReminderSettings
{
    public partial class FormMain : Form
    {
        public const string GraphCredentialsKey = @"Software\CBI\AppointmentReminder\GraphCredentials";
        public FormMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string str = "Hello";

            byte[] bytes = Encoding.Unicode.GetBytes(str);

            byte[] encryptedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);

            string encryptedB64 = Convert.ToBase64String(encryptedBytes);

            MessageBox.Show(encryptedB64);

            byte[] decodedB64 = Convert.FromBase64String(encryptedB64);

            byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.LocalMachine);

            string decryptedString = Encoding.Unicode.GetString(decryptedBytes);

            MessageBox.Show(decryptedString);

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

            RegistryKey key = Registry.CurrentUser.OpenSubKey(GraphCredentialsKey, true);

            if (key == null)
                key = Registry.CurrentUser.CreateSubKey(GraphCredentialsKey, true);

            key.SetValue("ApplicationID", applicationIdEncrypted);
            key.SetValue("TenantID", tenantIdEncrypted);
            key.SetValue("ClientSecret", clientSecretEncrypted);

            key.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtApplicationID.Clear();
            txtTenantID.Clear();
            txtClientSecret.Clear();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(GraphCredentialsKey);

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
            }
        }
    }
}
