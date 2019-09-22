using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShowMustNotGoOn.DatabaseService.Entities
{
    public partial class Users
    {
        public long Id { get; set; }
        public long TelegramId { get; set; }
        [Required]
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
