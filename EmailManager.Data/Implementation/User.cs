﻿using System.Collections.Generic;
using EmailManager.Data.Contracts;
using Microsoft.AspNetCore.Identity;

namespace EmailManager.Data.Implementation
{
    public class User : IdentityUser, IUser
    {
        //Мисля Role Enum както каза Мадин
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<Email> UserEmails { get; set; }
    }
}
