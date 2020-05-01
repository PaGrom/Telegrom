using System.ComponentModel.DataAnnotations.Schema;

namespace ShowMustNotGoOn.DatabaseContext.Model
{
    public sealed class IdentityUser
    {
	    [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
