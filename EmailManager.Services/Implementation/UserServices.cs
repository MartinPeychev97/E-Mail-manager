﻿using EmailManager.Data.Context;
using EmailManager.Data.Implementation;
using EmailManager.Services.Contracts;
using EmailManager.Services.Exeptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailManager.Services.Implementation
{
    public class UserServices : IUserServices
    {
        private readonly EmailManagerContext _context;
        private readonly UserManager<User> _userManager;

        public UserServices(EmailManagerContext context, UserManager<User> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }

        public async Task RegisterAccountAsync(User registerAccount)
        {
            var validationMethod = ValidationMethod(registerAccount);

            var user = await this._context.Users
                .Where(name => name.UserName == validationMethod.UserName)
                .Select(username => username.UserName)
                .SingleOrDefaultAsync();

            if (user != null)
            {
                throw new UserExeptions($"You cannot register accout with the following username {validationMethod.UserName}");
            }
        }

        public User ValidationMethod(User user)
        {
            if (user.Role != "Manager" && user.Role != "Operator")
            {
                throw new UserExeptions("Wrong role name!");
            }

            if (user.UserName.Length < 3 || user.UserName.Length > 50)
            {
                throw new UserExeptions("Username must be betweeen 3 and 50 symbols!");
            }

            //if (user.Password.Length < 5 || user.Password.Length > 100)
            //{
            //    throw new UserExeptions("Password must be betweeen 5 and 50 symbols!");
            //}

            if (user.Email == null)
            {
                throw new UserExeptions("Email cannot be null!");
            }

            if (user.Email.Length < 5 || user.Email.Length > 50)
            {
                throw new UserExeptions("Email must be betweeen 5 and 50 symbols!");
            }
            return user;
        }
    }
}
