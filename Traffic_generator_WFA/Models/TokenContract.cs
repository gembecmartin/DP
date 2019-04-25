using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_generator_WFA.Models
{
    [BsonIgnoreExtraElements]
    public class TokenContract
    {

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("properties")]
        public TokenContractProperties Properties {get; set; }
    }

    [BsonIgnoreExtraElements]
    public class TokenContractProperties
    {
        [BsonElement("master")]
        public string Master { get; set; }
        [BsonElement("code")]
        public string Code { get; set; }
        [BsonElement("address")]
        public string Address { get; set; }
        [BsonElement("decimals")]
        public int Decimals { get; set; }
        [BsonElement("supply")]
        public double Supply { get; set; }
        [BsonElement("originalTokenAddress")]
        public string OriginalTokenAddress { get; set; }
    }
}
