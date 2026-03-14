using Ayws.Security.Service.Api.Controllers.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Features.Tenant.Commands.RegisterTenant;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ayws.Security.Service.Api.Controllers;

[Route("api/auth")]
public class AuthController(IMediator mediator, IUnitOfWork unitOfWork) : CustomBaseController
{
    /// <summary>
    /// Yeni bir tenant oluşturur ve owner kullanıcısını kaydeder.
    /// Login işlemi Keycloak üzerinden yapılır:
    /// POST /realms/{realmName}/protocol/openid-connect/token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterTenantCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return CreateActionResult(result);
    }

    /// <summary>
    /// E-posta adresine göre tenant realm adını döndürür.
    /// Frontend bu bilgiyi Keycloak token isteğinde kullanır.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("realm")]
    public async Task<IActionResult> GetRealmByEmail([FromQuery] string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            return CreateActionResult(ServiceResult<string>.Fail("E-posta adresi zorunludur."));

        var tenant = await unitOfWork.Repository<TenantEntity, Guid>()
            .Where(t => t.OwnerEmail == email.ToLower().Trim())
            .FirstOrDefaultAsync(ct);

        if (tenant is null)
            return CreateActionResult(ServiceResult<string>.Fail("Bu e-posta ile kayıtlı hesap bulunamadı.", System.Net.HttpStatusCode.NotFound));

        return CreateActionResult(ServiceResult<string>.SuccessAsOk(tenant.RealmName));
    }
}
