namespace Eliza.Database.Models
{
    public class UserBlacklistedTag
    {
        public ulong UserId { get; set; }
        public int TagId { get; set; }

        public User User { get; set; }
        public Tag Tag { get; set; }
    }
}
