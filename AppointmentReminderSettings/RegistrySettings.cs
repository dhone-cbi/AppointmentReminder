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
            RegistryKey key = Registry.CurrentUser.OpenSubKey(SettingsKey, true);

            if (key == null)
                key = Registry.CurrentUser.CreateSubKey(SettingsKey, true);

            key.SetValue("ReportRecipients", ReportRecipients);
            key.SetValue("SmsGatewayDomain", SmsGatewayDomain);

            key.Close();
        }

        private void LoadCredentials()
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


                GraphApplicationId = Encoding.Unicode.GetString(applicationIdDecrypted);
                GraphTenantId = Encoding.Unicode.GetString(tenantIdDecrypted);
                GraphClientSecret = Encoding.Unicode.GetString(clientSecretDecrypted);
            }
        }

        private void LoadSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(SettingsKey);

            if (key == null) 
                return;
            else
            {
                ReportRecipients = (string)key.GetValue("ReportRecipients");
                SmsGatewayDomain = (string)key.GetValue("SmsGatewayDomain");
            }
        }

    }
}
