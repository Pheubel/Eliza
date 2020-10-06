using System.Collections.Generic;

namespace Eliza.Shared
{
    public class UserInfoDTO
    {
        public bool IsAuthenticated { get; set; }
        public string Username { get; set; }
        public ulong UserId { get; set; }
        public string AvatarHash { get; set; }
        public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
    }
}
