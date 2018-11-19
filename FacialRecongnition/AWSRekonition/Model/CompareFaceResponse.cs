using System;
using System.Collections.Generic;
using System.Text;

namespace AWSRekonition.Model
{
    public class CompareFaceResponse : MessageResponse
    {
        public string FaceId { get; set; }
        public float Confidence { get; set; }
    }
}
