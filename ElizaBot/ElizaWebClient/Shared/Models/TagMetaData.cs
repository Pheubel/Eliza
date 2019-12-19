using System;
using System.Collections.Generic;
using System.Text;

namespace ElizaWebClient.Shared.Models
{
    public class TagMetaData
    {
        public string TagName { get; set; }
        public int SubscriberCount { get; set; }
        public int BlacklisterCount { get; set; }
    }
}
