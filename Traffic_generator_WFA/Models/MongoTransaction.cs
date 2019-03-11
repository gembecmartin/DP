using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_generator_WFA.Models
{
    [BsonIgnoreExtraElements]
    public class MongoTransaction
    {
        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("tokenContract")]
        public string TokenContract { get; set; }

        [BsonElement("txhash")]
        public string Hash{ get; set; }

        [BsonElement("amount")]
        public decimal Amount { get; set; }

        [BsonElement("blockNumber")]
        public decimal BlockNumber { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }
    }
}
