using System;
using Microsoft.AspNetCore.Identity;

namespace BookLib.Entities
{
    public class User: IdentityUser
    {
        public DateTimeOffset BirthDate { get; set; }
    }
}
