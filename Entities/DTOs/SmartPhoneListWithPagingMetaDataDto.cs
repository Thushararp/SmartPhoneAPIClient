using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public  class SmartPhoneListWithPagingMetaDataDto
    {
        [JsonPropertyName("products")]
        public IEnumerable<SmartPhoneDto> SmartPhones { get; set; } = new List<SmartPhoneDto>();

        public int Total { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
    }
}
