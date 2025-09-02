using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapClastering
{
    internal class Application
    {
        public string Address {  get; set; }
        public ApplicationType Type { get; set; }
        public DateTime DateTime { get; set; }
    }

    public enum ApplicationType
    {
        G_PON,
        PACKET
    }
}
