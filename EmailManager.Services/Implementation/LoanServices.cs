using EmailManager.Data.Context;
using EmailManager.Data.Implementation;
using EmailManager.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailManager.Services.Implementation
{
    public class LoanServices : ILoanServices
    {
        private static readonly log4net.ILog log =
          log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EmailManagerContext _context;
        private readonly IDecryptionServices _decrypt;
        private readonly IEncryptionServices _encrypt;
        private readonly IEmailService _emailService;

        public LoanServices(EmailManagerContext context, IEncryptionServices securityEncrypt,
            IDecryptionServices decrypt, IEncryptionServices encrypt, IEmailService emailService)
        {
            this._context = context;
            this._decrypt = decrypt;
            this._encrypt = encrypt;
            this._emailService = emailService;
        }

        public async Task<Loan> CreateLoanApplication(Client client, int loanSum, string userId, int emailId)
        {
            var email = await _context.Emails
                .Where(e => e.Id == emailId)
                .FirstOrDefaultAsync();

            var createLoan = new Loan
            {
                LoanedSum = loanSum,
                DateAsigned = DateTime.UtcNow,
                ClientId = client.ClientId,
                Client = client,
                LoanEmailId = email.Id,
            };

            await _emailService.MarkOpenStatus(emailId, userId);

            await _context.Loans.AddAsync(createLoan);
            await _context.SaveChangesAsync();

            log.Info($"New loan to client Id: {client.ClientId} for {loanSum} sum.");

            return createLoan;
        }

        public bool CheckEgnValidity(string email)
        {
            var egn = "";

            for (int i = 0; i < email.Length; i++)
            {
                if (Char.IsDigit(email[i]))
                {
                    egn.Concat(email[i].ToString());
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<Client> AddClient(string clientName, string clientPhone, string clientEGN,
            string clientEmail, string userId)
        {
            IEnumerable<Client> clientAll = _context.Clients;
            IEnumerable<Client> decryptedClients = _decrypt.DecryptClientList(clientAll);

            Client newClientCheck = decryptedClients
                .FirstOrDefault(a => a.ClientEGN == clientEGN);

            if (newClientCheck == null)
            {
                newClientCheck = _encrypt.EncryptClientInfo(clientName, clientPhone, clientEGN, clientEmail, userId);

                await _context.Clients.AddAsync(newClientCheck);
                await _context.SaveChangesAsync();

                log.Info($"New client with Id: {newClientCheck.ClientId}");
            }
            else
            {
                Client decryptClientData = await _decrypt.DecryptClientInfo(newClientCheck);
                newClientCheck = decryptClientData;

                log.Info($"Found excisting client with Id {newClientCheck.ClientId}");
            }

            if (newClientCheck == null)
            {
                log.Fatal("New client is null! Must never reach here! Error in AddClient method => Loan service.");
            }

            return newClientCheck;
        }
    }
}
