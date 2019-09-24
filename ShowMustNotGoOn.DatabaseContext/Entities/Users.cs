using System.ComponentModel.DataAnnotations;

namespace ShowMustNotGoOn.DatabaseContext.Entities
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
