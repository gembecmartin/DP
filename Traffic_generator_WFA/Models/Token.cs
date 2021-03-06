﻿using MongoDB.Bson.Serialization.Attributes;
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
        [BsonElement("properties")]
        public TokenProperties Properties { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class TokenProperties
    {
        [BsonElement("code")]
        public string Code { get; set; }
        [BsonElement("address")]
        public string Address { get; set; }
        [BsonElement("decimals")]
        public int Decimals { get; set; }
        [BsonElement("supply")]
        public double Supply { get; set; }
    }
}
