﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmailManager.Data;
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
        
        public IActionResult Detail(int id)
        {
            var email = _emailService.GetEmail(id);
            var emailAttachments = _emailService.GetAttachment(id);

            var emailModel = new EmailViewModel(email, emailAttachments);

            log.Info($"User opened email detail page. Email Id: {id}");

            return View("Detail", emailModel);
        }

        [ResponseCache(Duration = 7200)]
        private async Task GetEmailsFromGmail()
        {
            DateTime dateTimeNow = DateTime.UtcNow;

            dateTimeNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day,
                dateTimeNow.Hour, dateTimeNow.Minute, dateTimeNow.Second, dateTimeNow.Kind);

            DateTime dateTimeAfter = dateTimeNow.AddSeconds(5);

            if (dateTimeNow <= dateTimeAfter)
            {
                await _gmailAPIService.SaveEmailsToDB();
                dateTimeNow = DateTime.UtcNow;
            }
        }

        [ResponseCache(Duration = 7200)]
        public async Task<IActionResult> ListAllStatusEmails(int? currentPage, string search = null)
        {
            await GetEmailsFromGmail();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currPage = currentPage ?? 1;
            int totalPages = await _emailService.GetPageCount(10);

            IEnumerable<Email> emailAllResults = null;

            if (!string.IsNullOrEmpty(search))
            {
                emailAllResults = await _emailService.SearchEmails(search, currPage, userId);
                log.Info($"User searched for {search}.");
            }
            else
            {
                emailAllResults = await _emailService.GetAllStatusEmails(currPage, userId);
                log.Info($"Displayed all emails list.");
            }

            var emailsListing = emailAllResults
                .Select(m => EmailMapper.MapFromEmail(m));

            var emailModel = EmailMapper.MapFromEmailIndex(emailsListing, currPage, totalPages);

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
    }
}