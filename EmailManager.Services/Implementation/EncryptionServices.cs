﻿using EmailManager.Services.Contracts;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EmailManager.Services.Implementation
{
    public class EncryptionServices : IEncryptionServices
    {
        public string Encrypt(string clearText)
        {
            string EncryptionKey = "banana";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = key.GetBytes(32);
                encryptor.IV = key.GetBytes(16);
                using (MemoryStream stream = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(stream, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(stream.ToArray());

                }
            }

            return clearText;
        }
    }
}
