using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShowMustNotGoOn.DatabaseContext.Model
{
    public sealed class Callback
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public int BotMessageId { get; set; }

        public CallbackType CallbackType { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
