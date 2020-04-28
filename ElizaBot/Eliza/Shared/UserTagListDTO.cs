using System;
using System.Collections.Generic;
using System.Text;

namespace Eliza.Shared
{
    public class UserTagListDTO
    {
        public IEnumerable<string> SubscribedTags { get; set; }
        public IEnumerable<string> BlacklistedTags { get; set; }
    }
}
