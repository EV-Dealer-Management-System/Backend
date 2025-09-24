using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Domain.Entities
{
    public class Attachment
    {
        public Guid Id { set; get; }
        public string S3Key { set; get; }
        public string Type { set; get; }
        public Attachment(string s3Key, string type)
        {
            S3Key = s3Key;
            Type = type;
        }
    }
}
