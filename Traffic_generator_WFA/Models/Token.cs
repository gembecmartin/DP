using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_generator_WFA.Models
{
    [BsonIgnoreExtraElements]
    public class Token
    {
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("code")]
        public string Code { get; set; }
        [BsonElement("address")]
        public string Address { get; set; }
    }
}
