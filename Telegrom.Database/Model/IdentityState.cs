using System.ComponentModel.DataAnnotations.Schema;

namespace Telegrom.Database.Model
{
    [Table("IdentityState", Schema = "Telegrom")]
    public sealed class IdentityState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdentityId { get; set; }
        public string StateName { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
