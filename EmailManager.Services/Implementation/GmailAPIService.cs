﻿using EmailManager.Data;
using EmailManager.Data.Context;
using EmailManager.Data.Enums;
using EmailManager.Data.Implementation;
using EmailManager.Services.Contracts;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmailManager.Services.Implementation
{
    public class GmailAPIService : IGmailAPIService
    {
        private readonly EmailManagerContext _context;

        public GmailAPIService(EmailManagerContext context)
        {
            this._context = context;
        }

        public async Task SaveEmailsToDB()
        {
            // If modifying these scopes, delete your previously saved credentials
            // at ~/.credentials/gmail-dotnet-quickstart.json
            string[] Scopes = { GmailService.Scope.GmailReadonly };
            string ApplicationName = "Gmail API .NET Quickstart";

            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";

                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));

                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var emailListRequest = service.Users.Messages.List("emailmanager13@gmail.com");

            emailListRequest.LabelIds = "INBOX";
            emailListRequest.IncludeSpamTrash = false;

            var emailListResponse = emailListRequest.ExecuteAsync().Result;

            if (emailListResponse != null && emailListResponse.Messages != null)
            {
                foreach (var email in emailListResponse.Messages)
                {
                    var emailRequest = service.Users.Messages.Get("emailmanager13@gmail.com", email.Id);

                    //Collection with full email response
                    var emailFullResponse = emailRequest.ExecuteAsync().Result;

                    Attachment attachmentParts = null;

                    if (emailFullResponse.Payload.Body.AttachmentId != null)
                    {
                        attachmentParts = new Attachment
                        {
                            AttachmentId = emailFullResponse.Payload.Body.AttachmentId,
                            AttachmentSize = emailFullResponse.Payload.Body.Size,
                            FileName = emailFullResponse.Payload.Filename,
                            EmailId = emailFullResponse.Id,
                        };
                    }                    

                    string subject = emailFullResponse.Payload.Headers
                        .FirstOrDefault(s => s.Name == "Subject").Value;

                    string sender = emailFullResponse.Payload.Headers
                        .FirstOrDefault(s => s.Name == "From").Value;

                    string date = emailFullResponse.Payload.Headers
                        .FirstOrDefault(d => d.Name == "Date").Value;

                    EmailBody emailBody = new EmailBody
                    {
                        Body = emailFullResponse.Snippet,
                    };

                    Email emailParts = new Email
                    {
                        EmailId = emailFullResponse.Id,
                        //Attachments = attachmentParts.Email.Attachments,
                        EmailBody = emailBody,
                        EnumStatus = EmailStatus.NotReviewed,
                        CurrentStatus = DateTime.UtcNow,
                        //ReceiveDate = emailFullResponse.InternalDate.Value,
                        Subject = subject,
                        Sender = sender,
                    };

                    await _context.AddAsync(emailParts);
                    await _context.SaveChangesAsync();
                }
            }

            // Define parameters of request.
            UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("emailmanager13@gmail.com");

            // List labels.
            var labels = request.Execute().Labels;

            Console.WriteLine("Labels:");

            if (labels != null && labels.Count > 0)
            {
                foreach (var labelItem in labels)
                {
                    Console.WriteLine("{0}", labelItem.Name);
                }
            }
            else
            {
                Console.WriteLine("No labels found.");
            }
        }

        //public void PassEmailParams()
        //{

        //}

        //public bool SaveRefreshToken(string gmailId, string refreshToken)
        //{
        //    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString);
        //    string query = "INSERT INTO Member (GmailId,RefreshToken) VALUES (@GmailId,@RefreshToken)";
        //    SqlCommand cmd = new SqlCommand(query, connection);
        //    cmd.Parameters.AddWithValue("GmailId", gmailId);
        //    cmd.Parameters.AddWithValue("RefreshToken", refreshToken);
        //    connection.Open();

        //    int result = cmd.ExecuteNonQuery();

        //    connection.Close();

        //    return result > 0 ? true : false;
        //}

        //public string GetRefreshToken(string gmailId)
        //{
        //    SqlConnection connection = new SqlConnection(ConfigurationManager
        //                                                                    .ConnectionStrings["DbConnection"]
        //                                                                    .ConnectionString);

        //    string query = "SELECT RefreshToken FROM Member WHERE GmailId-@GmailId";

        //    SqlCommand cmd = new SqlCommand(query, connection);

        //    cmd.Parameters.AddWithValue("GmailId", gmailId);
        //    connection.Open();

        //    object result = cmd.ExecuteScalar();

        //    connection.Close();

        //    return result == null ? string.Empty : Convert.ToString(result);
        //}

        //public bool UpdateRefreshToken(string gmailId, string refreshToken)
        //{
        //    SqlConnection connection = new SqlConnection(ConfigurationManager
        //                            .ConnectionStrings["DbConnection"]
        //                            .ConnectionString);

        //    string query = "UPDATE Member SET RefreshToken-@RefreshToken WHERE GmailId-@GmailId";

        //    SqlCommand cmd = new SqlCommand(query, connection);

        //    cmd.Parameters.AddWithValue("GmailId", gmailId);
        //    cmd.Parameters.AddWithValue("RefreshToken", refreshToken);
        //    connection.Open();

        //    int result = cmd.ExecuteNonQuery();

        //    connection.Close();

        //    return result > 0 ? true : false;
        //}
    }
}
