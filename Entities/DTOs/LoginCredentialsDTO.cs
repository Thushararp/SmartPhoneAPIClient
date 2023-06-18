using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class LoginCredentialsDTO
    {
        [Required(ErrorMessage = "Username is required")]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
