using EmailManager.Data.Context;
using EmailManager.Data.Implementation;
using EmailManager.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EmailManager.Services.Implementation
{
    public class DecryptionServices : IDecryptionServices
    {
        private readonly EmailManagerContext _context;

        public DecryptionServices(EmailManagerContext context)
        {
            this._context = context;
        }

        public string Decrypt(string cipherText)
        {
            string EncryptionKey = "banana";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(EncryptionKey, new byte[]
                { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });

                encryptor.Key = key.GetBytes(32);
                encryptor.IV = key.GetBytes(16);
                using (MemoryStream stream = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(stream, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(stream.ToArray());
                }
            }

            return cipherText;
        }

        public IEnumerable<Client> DecryptClientList(IEnumerable<Client> client)
        {
            var clients = new List<Client>();

            foreach (var item in client)
            {
                var egn = Decrypt(item.ClientEGN);

                clients.Add(new Client
                {
                    ClientEGN = egn,
                    ClientId = item.ClientId
                });
            }
            return clients;
        }

        public string Base64Decrypt(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public async Task<Client> DecryptClientInfo(Client clientId)
        {
            Client client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == clientId.ClientId);

            string decryptName = Decrypt(client.ClientName);
            string decryptEgn = Decrypt(client.ClientEGN);
            string decryptPhoneNumber = Decrypt(client.ClientPhoneNumber);
            string decryptEmail = Decrypt(client.ClientEmail);

            Client clientForLoan = new Client
            {
                ClientName = decryptName,
                ClientEmail = decryptEgn,
                ClientEGN = decryptPhoneNumber,
                ClientPhoneNumber = decryptEmail,
                UserId = client.UserId,
                EmailId = client.EmailId,
                Email = client.Email,
            };

            return client;
        }
    }
}
