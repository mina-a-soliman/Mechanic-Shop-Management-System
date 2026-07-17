using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateTokens;

public class GenerateTokenQueryHandler(ILogger<GenerateTokenQueryHandler> logger, IIdentityService identityService, ITokenProvider tokenProvider) : IRequestHandler<GenerateTokenQuery, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(GenerateTokenQuery request, CancellationToken cancellationToken)
    {
        var userResponse = await identityService.AuthenticateAsync(request.Email, request.Password);

        if (userResponse.IsError)
        {
            // Do not log critical data.
            return userResponse.Errors;
        }

        var generateTokenResult = await tokenProvider.GenerateJwtTokenAsync(userResponse.Value, cancellationToken);

        if (generateTokenResult.IsError)
        {
            logger.LogError("Generate token error occurred: {ErrorDescription}", generateTokenResult.TopError.Description);
            return generateTokenResult.Errors;
        }
        return generateTokenResult.Value;
    }
}