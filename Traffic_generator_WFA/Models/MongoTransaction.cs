using MongoDB.Bson;
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
        [BsonElement("count")]
        public int Count { get; set; }
        [BsonElement("amount")]
        public double Amount { get; set; }

    }
}
