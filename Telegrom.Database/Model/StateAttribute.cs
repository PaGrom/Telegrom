namespace Telegrom.Database.Model
{
    public sealed class StateAttribute
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Object { get; set; }
    }
}
