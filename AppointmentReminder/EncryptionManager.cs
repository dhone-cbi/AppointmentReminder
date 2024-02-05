using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime;
using System.Security.Cryptography;

namespace AppointmentReminder
{
    /// <summary>
    /// The EncryptionManager class manages generation and storage of the Key and IV for AES encryption.  It
    /// also supports quick AES encryption and decryption.  
    /// </summary>
    public class EncryptionManager
    {
        private Aes aes;

        /// <summary>
        /// The key to be used in AES encryption
        /// </summary>
        public byte[] Key { get { return aes.Key; } }

        /// <summary>
        /// The IV to be used in AES encyrption
        /// </summary>
        public byte[] IV { get { return aes.IV; } }

        /// <summary>
        /// The registry location to load and store the Key and IV for the encryption algorithm
        /// </summary>
        public string RegistryLocation { get; set; }

        /// <summary>
        /// This function encrypts a given string
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <returns>A byte array representing the encrypted string</returns>
        public byte[] EncryptString(string str)
        {
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            StreamWriter streamWriter = new StreamWriter(cryptoStream);
            streamWriter.Write(str);
            streamWriter.Close();

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Encrypts a given string, returning the result as base-64 binary
        /// </summary>
        /// <param name="str">The string to be encrypted</param>
        /// <returns>The encrypted string in base-64 binary</returns>
        public string EncryptStringB64(string str)
        {
            return Convert.ToBase64String(EncryptString(str));
        }


        /// <summary>
        /// Decrypts a given byte array, returning the result as a string
        /// </summary>
        /// <param name="data">A byte array to be decrypted</param>
        /// <returns>The decrypted data as a string</returns>
        string DecryptString(byte[] data)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream(data);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                StreamReader streamReader = new StreamReader(cryptoStream);
                string result = streamReader.ReadToEnd();
                streamReader.Close();

                return result;
            }
            catch (CryptographicException ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Decrypts a string in base-64 binary, returning the result as a string
        /// </summary>
        /// <param name="str">The string to be decrypted in base-64 binary</param>
        /// <returns>The decrypted data as a string</returns>
        public string DecryptStringB64(string str)
        {
            return DecryptString(Convert.FromBase64String(str));
        }

        // encryptionManager.DecryptStringB64((string)Settings.Default["DFUsername"]);

        /*public string DecryptSettingB64(string settingName)
        {
            return DecryptStringB64((string)Settings.Default[settingName]);
        }*/


        /// <summary>
        /// Generates a Key and IV for use in AES encryption
        /// </summary>
        public void GenerateKeyAndIV()
        {
            aes.GenerateKey();
            aes.GenerateIV();
        }


        /// <summary>
        /// Stores the current Key and IV in the registry location specified in the RegistryLocation property
        /// </summary>
        public void SaveKeyAndIV()
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(RegistryLocation, true);

            if (regKey == null)
                regKey = Registry.CurrentUser.CreateSubKey(RegistryLocation, true);

            regKey.SetValue("Key", aes.Key);
            regKey.SetValue("IV", aes.IV);
            regKey.Close();
        }


        /// <summary>
        /// Loads the Key and IV  from the location specified in the RegistryLocation property
        /// </summary>
        /// <returns>A bool indicating whether the key could be loaded</returns>
        public bool LoadKeyAndIV()
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(RegistryLocation);

            if (regKey == null)
                return false;
            else
            {
                aes.Key = (byte[])regKey.GetValue("Key");
                aes.IV = (byte[])regKey.GetValue("IV");
                regKey.Close();
                return true;
            }
        }

        /// <summary>
        /// Initializes an instance of the EncryptionManager class
        /// </summary>
        public EncryptionManager()
        {
            aes = Aes.Create();
        }

        /// <summary>
        /// Initializes an instance of the EncryptionManager using the specified inital RegistryLocation
        /// </summary>
        /// <param name="registryLocation">The registry location to load and store the Key and IV for the eyncryption algorithm</param>
        public EncryptionManager(string registryLocation) : this()
        {
            RegistryLocation = registryLocation;
        }
    }
}