using EmailManager.Data;
using EmailManager.Models.EmailViewModel;
using EmailManager.Services.Contracts;
using System.Collections.Generic;

namespace EmailManager.Mappers
{
    public static class EmailMapper
    {
        public static EmailViewModel MapFromEmail(this Email email, IEmailService emailService)
        {
            var emailListing = new EmailViewModel
            {
                Id = email.Id,
                EmailId = email.EmailId,
                Subject = email.Subject,
                Sender = email.Sender,
                Body = email.EmailBody.Body,
                ReceiveDate = email.ReceiveDate,
                StatusChangedBy = email.User.UserName,
                InCurrentStatusSince = email.Status.NewStatus,
                EnumStatus = email.EnumStatus,
                CurrentUser = email.User.UserName,
                HasAttachments = email.HasAttachments,
                //TODO - null за attachmenta
               //AttachmentName = emailService.GetAttachment(email.Id).FileName,
               //AttachmentSize = emailService.GetAttachment(email.Id).AttachmentSizeKb,
            };

            return emailListing;
        }

        public static EmailIndexViewModel MapFromEmailIndex(this IEnumerable<EmailViewModel> email, int currentPage, int totalPages)
        {
            var model = new EmailIndexViewModel
            {
                CurrentPage = currentPage,
                TotalPages = totalPages,
                Emails = email
            };

            return model;
        }
    }
}