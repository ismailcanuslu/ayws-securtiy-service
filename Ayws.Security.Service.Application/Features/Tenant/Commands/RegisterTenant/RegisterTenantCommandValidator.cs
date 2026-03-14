using Ayws.Security.Service.Application.Common;
using Ayws.Security.Service.Application.Contracts.Persistence;
using FluentValidation;

namespace Ayws.Security.Service.Application.Features.Tenant.Commands.RegisterTenant;

public class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.RealmName)
            .NotEmpty().WithMessage("Realm adı boş olamaz.")
            .MaximumLength(50).WithMessage("Realm adı en fazla 50 karakter olabilir.")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Realm adı yalnızca küçük harf, rakam ve tire içerebilir.")
            .MustAsync(async (name, ct) =>
                !await unitOfWork.Repository<Domain.Entities.Tenant.TenantEntity, Guid>()
                    .AnyAsync(t => t.RealmName == name, ct))
            .WithMessage("Bu realm adı zaten kullanımda.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Görünen ad boş olamaz.")
            .MaximumLength(100).WithMessage("Görünen ad en fazla 100 karakter olabilir.");

        RuleFor(x => x.OwnerEmail)
            .NotEmpty().WithMessage("E-posta adresi boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.OwnerFirstName)
            .NotEmpty().WithMessage("Ad boş olamaz.")
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

        RuleFor(x => x.OwnerLastName)
            .NotEmpty().WithMessage("Soyad boş olamaz.")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");

        RuleFor(x => x.OwnerPassword)
            .NotEmpty().WithMessage("Şifre boş olamaz.")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.");
    }
}
