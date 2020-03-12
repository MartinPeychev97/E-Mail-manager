using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmailManager.Data;
using EmailManager.Data.Implementation;
using EmailManager.Mappers;
using EmailManager.Models.EmailViewModel;
using EmailManager.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailManager.Controllers
{
    public class EmailController : Controller
    {
        private static readonly log4net.ILog log =
        log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEmailService _emailService;
        private readonly IGmailAPIService _gmailAPIService;

        public EmailController(IEmailService service, IGmailAPIService gmailAPIService)
        {
            this._emailService = service;
            this._gmailAPIService = gmailAPIService;
        }
        
        public async Task<IActionResult> Detail(int id)
        {
            Email email = await _emailService.GetEmail(id);
            Attachment emailAttachments = await _emailService.GetAttachment(id);

            EmailViewModel emailModel = new EmailViewModel(email, emailAttachments);

            log.Info($"User opened email detail page. Email Id: {id}");

            return View("Detail", emailModel);
        }

        //TODO: Kiro - тук е да взема новите писма и да ги запазва в датабазата
        //Custom Email google check - because of email access problems
        [ResponseCache(Duration = 7200)]
        private async void GetEmailsFromGmail()
        {
            DateTime dateTimeNow = DateTime.UtcNow;

            dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day,
                dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second, dateTimeNow.Kind);

            //за да не го проверява на всеки клик съм сложила 5 секунди да са минали поне след влизане в каталога
            DateTime dateTimeAfter = dateTimeNow.AddSeconds(5);

            if (dateTimeNow <= dateTimeAfter)
            {
                await _gmailAPIService.SaveEmailsToDB();
                //тук актуализирам времето (нз дали е нужно) сегашното да е сега, а не да е с добавеното от 5 сек
                dateTimeNow = DateTime.UtcNow;
            }
        }

        //For displaying all emails (with pagination of 10 per page)
        public async Task<IActionResult> ListAllStatusEmails(int? currentPage, string search = null)
        {
            GetEmailsFromGmail();

            string userId = FindUserById();
            int currPage = currentPage ?? 1;
            int totalPages = await _emailService.GetPageCount(10);

            IEnumerable<Email> emailAllResults = null;

            if (!string.IsNullOrEmpty(search))
            {
                //For email search
                emailAllResults = await _emailService.SearchEmails(search, currPage, userId);
                log.Info($"User searched for {search}.");
            }
            else
            {
                emailAllResults = await _emailService.GetAllStatusEmails(currPage, userId);
                log.Info($"Displayed all emails list.");
            }

            IEnumerable<EmailViewModel> emailsListing = emailAllResults
                .Select(m => EmailMapper.MapFromEmail(m, _emailService));

            EmailIndexViewModel emailModel = EmailMapper.MapFromEmailIndex(emailsListing, currPage, totalPages);

            //For pagination buttons and distribution
            emailModel.CurrentPage = currPage;
            emailModel.TotalPages = totalPages;

            if (totalPages > currPage)
            {
                emailModel.NextPage = currPage + 1;
            }

            if (currPage > 1)
            {
                emailModel.PreviousPage = currPage - 1;
            }

            return View(emailModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNew(int id)
        {
            string userId = FindUserById();
            int emailId = id;

            try
            {
                await _emailService.MarkNewStatus(emailId, userId);
                log.Info($"User changed email status to new. User Id: {userId} Email Id: {id}");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                log.Info($"System failed to change the email status to new. User Id: {userId} Email Id: {id}");

                return RedirectToAction("Detail", new { id = emailId });
            }

            return RedirectToAction("Detail", new { id = emailId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkClosedApproved(EmailViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            string userId = FindUserById();
            int emailId = viewModel.Id;

            try
            {
                await _emailService.MarkClosedApprovedStatus(emailId, userId);
                log.Info($"User changed email status to closed - approved. User Id: {userId} Email Id: {emailId}");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                log.Error($"System failed to change the email status to closed - approved. User Id: {userId} Email Id: {emailId}");

                return RedirectToAction("Detail", new { id = emailId });
            }

            return RedirectToAction("Detail", new { id = emailId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkClosedRejected(EmailViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            string userId = FindUserById();
            int emailId = viewModel.Id;

            try
            {
                await _emailService.MarkClosedRejectedStatus(emailId, userId);
                log.Info($"User changed email status to closed - rejected. User Id: {userId} Email Id: {emailId}");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                log.Error($"System failed to change the email status to closed - rejected. User Id: {userId} Email Id: {emailId}");

                return RedirectToAction("Detail", new { id = emailId });
            }

            return RedirectToAction("Detail", new { id = emailId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInvalid(EmailViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            string userId = FindUserById();
            int emailId = viewModel.Id;

            try
            {
                await _emailService.MarkInvalidStatus(emailId, userId);
                log.Info($"User changed email status to invalid. User Id: {userId} Email Id: {emailId}");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                log.Error($"System failed to change the email status to invalid. User Id: {userId} Email Id: {emailId}");

                return RedirectToAction("Detail", new { id = emailId });
            }

            return RedirectToAction("Detail", new { id = emailId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> MarkNotReviewed(EmailViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            string userId = FindUserById();
            int emailId = viewModel.Id;

            try
            {
                await _emailService.MarkNotReviewStatus(emailId, userId);
                log.Info($"User changed email status to not reviewed. User Id: {userId} Email Id: {emailId}");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                log.Error($"System failed to change the email status to not reviewed. User Id: {userId} Email Id: {emailId}");

                return RedirectToAction("Detail", new { id = emailId });
            }

            return RedirectToAction("Detail", new { id = emailId });
        }

        public string FindUserById()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return userId;
        }
    }
}