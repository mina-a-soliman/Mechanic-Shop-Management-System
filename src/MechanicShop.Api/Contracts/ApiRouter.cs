using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Contracts
{

    public class ApiRouter
    {
        public static class Identity
        {
            private const string Controller = "identity";
            public const string GenerateToken = $"{Controller}/token/generate";
            public const string RefreshToken = $"{Controller}/token/refresh-token";
            public const string CurrentUserClaims = $"{Controller}/current-user/claims";
        }
    }
}
