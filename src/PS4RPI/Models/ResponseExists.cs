using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS4RPI.Models
{
    public class ResponseExists
    {
        public string status { get; set; }
        public string error { get; set; }
        public bool exists { get; set; }
        public long size { get; set; }
    }
}
