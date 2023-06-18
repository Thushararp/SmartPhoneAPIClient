using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class SmartPhoneForPriceUpdateDto
    {
        [JsonPropertyName ("price")]
        public double Price { get; set; }
    }
}
