using AutoMapper;
using Ayws.Security.Service.Application.Features.Tenant.Dto;
using Ayws.Security.Service.Domain.Entities.Tenant;

namespace Ayws.Security.Service.Application.Features.Tenant;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<TenantEntity, TenantResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
