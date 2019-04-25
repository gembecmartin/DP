using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_generator_WFA.Models.Smart_Contract
{
    public class StandardTokenDeployment : ContractDeploymentMessage
    {
        public static string BYTECODE = "";

        public StandardTokenDeployment() : base(BYTECODE) { }

        [Parameter("uint256", "_initialSupply")]
        public BigInteger TotalSupply { get; set; }
        [Parameter("uint8", "_decimals")]
        public BigInteger Decimals { get; set; }
        [Parameter("string", "_symbol")]
        public string Code { get; set; }
        [Parameter("string", "_name")]
        public string Name { get; set; }
    }
}
