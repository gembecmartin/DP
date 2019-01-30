using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_generator_WFA.Models
{
    [BsonIgnoreExtraElements]
    public class MongoAccount
    {
        [BsonElement("Address")]
        public string Address { get; set; }

        public MongoAccount(string address)
        {
            Address = address;
        }
    }
}
