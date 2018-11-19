using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSRekonition.Model
{
    public class MessageResponse
    {
        public string id { get; set; }
        public int MessageCode { get; set; }
        public string Message { get; set; }
        public bool IsMatched { get; set; }
        public float Confidence { get; set; }
        public string CardNumber { get; set; }
    }
}
