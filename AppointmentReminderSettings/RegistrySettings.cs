using Microsoft.Graph.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentReminderSettings
{
    public class RegistrySettings
    {
        public const string GraphCredentialsKey = @"Software\CBI\AppointmentReminder\GraphCredentials";
        public const string SettingsKey = @"Software\CBI\AppointmentReminder\Settings";

        public string GraphTenantId { get; set; }
        public string GraphApplicationId { get; set; }
        public string GraphClientSecret { get; set; }

        public string ReportRecipients { get; set; }
        public string SmsGatewayDomain { get; set; }

        public void Save()
        {
            SaveCredentials();
            SaveSettings();
        }

        public void Load()
        {
            LoadCredentials();
            LoadSettings();
        }

        private void SaveCredentials()
        {
            byte[] applicationIdBytes = Encoding.Unicode.GetBytes(GraphApplicationId);
            byte[] tenantIdBytes = Encoding.Unicode.GetBytes(GraphTenantId);
            byte[] clientSecretBytes = Encoding.Unicode.GetBytes(GraphClientSecret);

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

        private void SaveSettings()
        {

        }

        private void LoadCredentials()
        {

        }

        private void LoadSettings()
        {

        }

    }
}
