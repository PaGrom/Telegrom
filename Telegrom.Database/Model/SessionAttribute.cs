using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telegrom.Database.Model
{
    public sealed class SessionAttribute
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SessionId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Type { get; set; }
        public string Value { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
