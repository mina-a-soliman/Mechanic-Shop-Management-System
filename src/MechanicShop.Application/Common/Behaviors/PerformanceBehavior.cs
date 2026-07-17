using System.Diagnostics;
using MechanicShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>
   (ILogger<TRequest> logger,
     IUser user,
     IIdentityService identityService
   )
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        try
        {
          return await next(cancellationToken);
        }
        finally
        {
            timer.Stop();

            var elapsedMilliseconds = timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;
                var userId = user.Id ?? string.Empty;
                var userName = string.Empty;
                if (!string.IsNullOrEmpty(userId))
                {
                    userName = await identityService.GetUserNameAsync(userId);
                }

                logger.LogWarning(
                         "Long running request {RequestName} took {ElapsedMilliseconds} ms. UserId: {UserId}, UserName: {UserName}, Request: {@Request}",
                         requestName,
                         timer.ElapsedMilliseconds,
                         userId,
                         userName,
                         request);
            }
        }

    }
}