using System.ComponentModel.DataAnnotations.Schema;

namespace Telegrom.Database.Model
{
    [Table("SessionUpdate", Schema = "Telegrom")]
    public sealed class SessionUpdate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IdentityId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UpdateId { get; set; }
        public string UpdateType { get; set; }
        public string Update { get; set; }
        public bool Processed { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
