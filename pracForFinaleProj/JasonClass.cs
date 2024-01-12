using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProj
{
    internal class JasonClass
    {
        public string firstName {  get; set; }
        public string lastName { get; set; }
        public cardDetails cardDetails { get; set; }
        public string PinCode {  get; set; }
        public List<transactionHistory> transactionHistory { get; set; }
    }
}
