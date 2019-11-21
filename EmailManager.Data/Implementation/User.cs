﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace EmailManager.Data.Implementation
{
    public class User : IdentityUser
    {
        public User()
        {
            this.UserEmails = new HashSet<Email>();
        }

        public string Role { get; set; }
        public string Name { get; set; }
        public ICollection<Email> UserEmails { get; set; }
        public ICollection<Client> Clients { get; set; }
       
    }
}
