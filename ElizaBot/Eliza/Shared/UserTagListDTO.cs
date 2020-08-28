using System.Collections.Generic;

namespace Eliza.Shared
{
    public class UserTagListDTO
    {
        public IEnumerable<string> SubscribedTags { get; set; }
        public IEnumerable<string> BlacklistedTags { get; set; }
    }
}
