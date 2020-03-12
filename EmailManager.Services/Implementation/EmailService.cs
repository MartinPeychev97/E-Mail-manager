using EmailManager.Data;
using EmailManager.Data.Context;
using EmailManager.Data.Enums;
using EmailManager.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailManager.Data.Implementation;


namespace EmailManager.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private static readonly log4net.ILog log =
           log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EmailManagerContext _context;
        private readonly IDecryptionServices _decrypt;

        public EmailService(EmailManagerContext context, IDecryptionServices decrypt)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._decrypt = decrypt;
        }

        public async Task<IEnumerable<Email>> GetAllStatusEmails(int currentPage, string userId)
        {
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            IEnumerable<Email> emailAll = _context.Emails
                     .Include(m => m.EmailBody)
                     .Include(m => m.Attachments)
                     .Include(m => m.Status)
                     .Include(m => m.User)
                     .OrderByDescending(m => m.Id);

            if (currentUser.Role == "Operator")
            {
                IEnumerable<Email> checkStatus = emailAll.Where(s => s.Status.EmailStatus == EmailStatus.Closed
                     || s.Status.EmailStatus == EmailStatus.Open);

                if (checkStatus != null)
                {
                    emailAll = emailAll
                    .Where(u => u.UserId == currentUser.Id || u.UserId == "NoUser");
                }
            }

            if (currentPage == 1)
            {
                emailAll = emailAll
                     .Take(10)
                     .ToList();
            }
            else
            {
                emailAll = emailAll
                    .Skip((currentPage - 1) * 10)
                    .Take(10)
                    .ToList();
            }

            log.Info("System listing all emails.");

            return emailAll;
        }

        public async Task<IEnumerable<Email>> SearchEmails(string search, int currentPage, string userId)
        {
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            IEnumerable<Email> searchResult = _context.Emails
                .Include(m => m.EmailBody)
                .Include(m => m.Attachments)
                .Include(m => m.Status)
                .Include(m => m.User).Where(
                       b => b.User.Email.Contains(search) ||
                       b.User.UserName.Contains(search) ||
                       b.Sender.Contains(search) ||
                       b.EmailId.Contains(search) ||
                       b.EnumStatus.ToString().ToLower().Contains(search.ToLower())
                       )
                .OrderByDescending(b => b.Status.NewStatus);

            if (currentUser.Role == "Operator")
            {
                searchResult = searchResult
                    .Where(u => (u.UserId == currentUser.Id || u.UserId == "NoUser")
                    && u.Status.EmailStatus == EmailStatus.Open || u.Status.EmailStatus == EmailStatus.Closed);
            }

            if (currentPage == 1)
            {
                searchResult = searchResult
                .Take(10)
                .ToList();
            }
            else
            {
                searchResult = searchResult
                .Skip((currentPage - 1) * 10)
                .Take(10)
                .ToList();
            }

            return searchResult;
        }

        public async Task<int> GetPageCount(int emailsPerPage)
        {
            var allEmails = await _context.Emails
                .CountAsync();

            var totalPages = (allEmails - 1) / emailsPerPage + 1;

            return totalPages;
        }


        public async Task<Email> GetEmail(int emailId)
        {
            Email email = await _context.Emails
                .Include(m => m.EmailBody)
                .Include(m => m.Attachments)
                .Include(m => m.Status)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == emailId);

            string decryptBody = _decrypt.Base64Decrypt(email.EmailBody.Body);
            email.EmailBody.Body = decryptBody;

            if (email == null)
            {
                log.Info($"System didn't find email with id: {emailId}");
            }

            log.Warn ($"User look for email with id: {emailId}");

            return email;
        }

        public async Task<Attachment> GetAttachment(int emailId)
        {
            Email email = await _context.Emails
                .Include(m => m.EmailBody)
                .Include(m => m.Attachments)
                .Include(m => m.Status)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == emailId);

            Attachment attachment = await _context.Attachments
                .Include(a => a.Email)
                .FirstOrDefaultAsync(e => email.Id == emailId);

            log.Info($"Server fetch attachment for email with Id: {emailId}");

            return attachment;
        }

        public async Task<EmailStatus> GetStatus(string emailId)
        {
            Email email = await _context.Emails
                .FirstOrDefaultAsync(b => b.EmailId == emailId);

            log.Info($"Email status sent. Email id: {emailId}");

            return email.EnumStatus;
        }


        public async Task MarkNewStatus(int emailId, string userId)
        {
            var email = EmailRepeatedPart(emailId, userId);

            email.Result.EnumStatus = EmailStatus.New;
            await _context.SaveChangesAsync();

            log.Info($"Changed email status to new. Email Id: {emailId}");
        }

        public async Task MarkClosedApprovedStatus(int emailId, string userId)
        {
            var email = EmailRepeatedPart(emailId, userId);

            email.Result.EnumStatus = EmailStatus.Approved;
            await _context.SaveChangesAsync();

            log.Info($"Changed email status to closed - approved. Email Id: {emailId} by user id: {userId}.");
        }

        public async Task MarkClosedRejectedStatus(int emailId, string userId)
        {
            var email = EmailRepeatedPart(emailId, userId);

            email.Result.EnumStatus = EmailStatus.Rejected;
            await _context.SaveChangesAsync();

            log.Info($"Changed email status to closed - rejected. Email Id: {emailId} by user id: {userId}.");
        }

        public async Task MarkOpenStatus(int emailId, string userId)
        {
            var email = EmailRepeatedPart(emailId, userId);

            email.Result.EnumStatus = EmailStatus.Open;
            await _context.SaveChangesAsync();

            log.Info($"Changed email status to open. Email Id: {emailId}");
        }

        public async Task MarkNotReviewStatus(int emailId, string userId)
        {
            var email = EmailRepeatedPart(emailId, userId);

            email.Result.EnumStatus = EmailStatus.NotReviewed;
            await _context.SaveChangesAsync();

            log.Info($"Changed email status to not review. Email Id: {emailId}");
        }

        public async Task MarkInvalidStatus(int emailId, string userId)
        {
            var email = EmailRepeatedPart(emailId, userId);

            email.Result.EnumStatus = EmailStatus.NotValid;
            await _context.SaveChangesAsync();

            log.Info($"Changed email status to invalid. Email Id: {emailId}");
        }

        public async Task<Email> EmailRepeatedPart(int emailId, string userId)
        {
            Email email = await _context.Emails
                .Include(a => a.Status)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == emailId);

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            log.Info($"Status changed: for Email Id: {emailId}; from user Id: {userId};" +
                $" last status: {email.Status.LastStatus}; new status {email.Status.NewStatus};" +
                $" changed on {DateTime.UtcNow}");

            var status = new Status
            {
                LastStatus = email.Status.LastStatus = email.Status.NewStatus,
                NewStatus = email.Status.NewStatus = DateTime.UtcNow,
                TimeStamp = email.Status.TimeStamp = DateTime.UtcNow,
                ActionTaken = email.Status.ActionTaken = "Changed",
            };

            email.User = user;
            user.UserEmails.Add(email);
            await _context.SaveChangesAsync();

            return email;
        }       
    }
}
