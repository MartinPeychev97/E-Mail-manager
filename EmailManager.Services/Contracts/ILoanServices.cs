﻿using System.Threading.Tasks;
using EmailManager.Data;
using EmailManager.Data.Implementation;

namespace EmailManager.Services.Contracts
{
    public interface ILoanServices
    {
        //Task<bool> ApproveLoan(ApproveLoan approveLoan);
        Task<Client> CreateLoanApplication(Client client, string userId, Email email);
        bool CheckEgnValidity(string email);
        Client EncryptClientInfo(Client clientId);
        Client DecryptClientInfo(Client clientId);
    }
}