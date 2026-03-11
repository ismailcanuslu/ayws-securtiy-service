using Ayws.Security.Service.Api.Controllers.Common;
using Ayws.Security.Service.Application.Features.Certificate.Commands;
using Ayws.Security.Service.Domain.Entities.Tenant;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ayws.Security.Service.Api.Controllers;

[Route("api/tenants/{tenantId:guid}/certificates")]
public class CertificatesController(IMediator mediator) : CustomBaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] CreateCertificateBody body, CancellationToken ct)
        => CreateActionResult(await mediator.Send(
            new CreateCertificateCommand(tenantId, body.Domain, body.Type, body.PemContent, body.ExpiresAt), ct));

    [HttpGet("{certId:guid}/expiry")]
    public async Task<IActionResult> GetExpiry(Guid tenantId, Guid certId, CancellationToken ct)
        => CreateActionResult(await mediator.Send(new GetCertificateExpiryQuery(tenantId, certId), ct));
}

public record CreateCertificateBody(string Domain, CertificateType Type, string PemContent, DateTime ExpiresAt);
