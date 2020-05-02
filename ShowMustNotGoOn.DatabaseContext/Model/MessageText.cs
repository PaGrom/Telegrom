using System.ComponentModel.DataAnnotations.Schema;

namespace ShowMustNotGoOn.DatabaseContext.Model
{
    public sealed class MessageText
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Text { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
