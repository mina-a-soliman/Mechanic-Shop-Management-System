using MechanicShop.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviors;

public class LoggingBehavior<TRequest>(ILogger<TRequest> logger, IUser user, IIdentityService identityService) : IRequestPreProcessor<TRequest>
{
    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {

        var requestName = typeof(TRequest).Name;
        var userId = user.Id ?? string.Empty;
        string? userName = string.Empty;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            userName = await identityService.GetUserNameAsync(userId);
        }
        logger.LogInformation("Request: {Name} {@UserId} {@UserName} {@Request}",requestName,userId,userName,request);
    }
}