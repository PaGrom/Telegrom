using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShowMustNotGoOn.DatabaseContext.Model
{
    public sealed class BotMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public int UserId { get; set; }

        public int MessageId { get; set; }

        public BotCommandType? BotCommandType { get; set; }

        public int MessageTextId { get; set; }

        public int MyShowsId { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }
    }
}
