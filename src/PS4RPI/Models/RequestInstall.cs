using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS4RPI.Models
{
    public class RequestInstall
    {
        public string type { get; set; } = "direct";
        public List<string> packages { get; set; }
    }
}
