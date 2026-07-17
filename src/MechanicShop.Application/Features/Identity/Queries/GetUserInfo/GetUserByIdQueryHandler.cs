using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

public class GetUserByIdQueryHandler(
    ILogger<GetUserByIdQueryHandler> logger,
    IIdentityService identityService) : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
{
    public async Task<Result<AppUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var getUserByIdResult = await identityService.GetUserByIdAsync(request.UserId);
        if (getUserByIdResult.IsError)
        {
            logger.LogError("User with Id { UserId } : {ErrorDetails}", request.UserId, getUserByIdResult.TopError.Description);
            return getUserByIdResult.Errors;
        }
         return getUserByIdResult.Value;
    }
}