﻿using System;
namespace BookLib.Models
{
    public class AuthorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string BirthPlace { get; set; }
    }
}
