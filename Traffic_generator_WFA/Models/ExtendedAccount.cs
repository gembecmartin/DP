using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;

namespace Traffic_generator_WFA.Models
{
    public class ExtendedAccount
    {
        public string Name { get; set; }
        public double Balance { get; set; }

        public ExtendedAccount() { }
    }
}
