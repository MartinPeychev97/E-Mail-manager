﻿using System.Threading.Tasks;
using EmailManager.Services.DTO;
using EmailManager.Data.Implementation;

namespace EmailManager.Services.Contracts
{
    public interface ILoanServices
    {
        Task<bool> ApproveLoanAsync(ApproveLoanDTO approveLoanDto);
        Task<Client> ClientLoanApplication(ClientDTO clientDto);
        bool CheckEgnValidity(string email);
    }
}