using System.ComponentModel.DataAnnotations.Schema;

namespace Telegrom.Database.Model
{
    public sealed class IdentityState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdentityId { get; set; }
        public string StateName { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
