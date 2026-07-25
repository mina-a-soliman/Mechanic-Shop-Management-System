using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MechanicShop.Infrastructure.Settings
{
    public class JwtSettings
    {
        public const string SectionName= "JwtSettings";

        [Required]
        public string Issuer { get; init; }

        [Required]
        public string Audience { get; init; }

        [Required]
        public string Secret { get; init; }

        [Range(1, 60)]
        public int TokenExpirationInMinutes { get; init; }

        [Range(1, 30)]
        public int RefreshTokenExpirationInDays { get; init; }

    }
}
