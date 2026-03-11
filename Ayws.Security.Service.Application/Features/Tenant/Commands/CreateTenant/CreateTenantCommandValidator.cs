using FluentValidation;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.CreateTenant;

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.RealmName)
            .NotEmpty().WithMessage("RealmName boş olamaz.")
            .MinimumLength(3).WithMessage("RealmName en az 3 karakter olmalıdır.")
            .MaximumLength(50).WithMessage("RealmName en fazla 50 karakter olabilir.")
            .Matches(@"^[a-z0-9\-]+$").WithMessage("RealmName yalnızca küçük harf, rakam ve tire içerebilir.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Görünen ad boş olamaz.")
            .MaximumLength(100).WithMessage("Görünen ad en fazla 100 karakter olabilir.");
    }
}
