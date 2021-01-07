using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS4RPI.Models
{
    public class ResponseTaskStatus
    {
        public string status { get; set; }
        public long bits { get; set; }
        public long error { get; set; }
        public long length { get; set; }
        public long transferred { get; set; }
        public long length_total { get; set; }
        public long transferred_total { get; set; }
        public long num_index { get; set; }
        public long num_total { get; set; }
        public long rest_sec { get; set; }
        public long rest_sec_total { get; set; }
        public long preparing_percent { get; set; }
        public long local_copy_percent { get; set; }
    }
}
