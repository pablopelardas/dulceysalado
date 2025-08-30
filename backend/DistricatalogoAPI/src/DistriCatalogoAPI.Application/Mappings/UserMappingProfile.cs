using AutoMapper;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Application.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.EmpresaId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => MapRoleToSimple(src.Role)))
                .ForMember(dest => dest.PuedeGestionarProductosBase, opt => opt.MapFrom(src => src.CanManageBaseProducts))
                .ForMember(dest => dest.PuedeGestionarProductosEmpresa, opt => opt.MapFrom(src => src.CanManageCompanyProducts))
                .ForMember(dest => dest.PuedeGestionarCategoriasBase, opt => opt.MapFrom(src => src.CanManageBaseCategories))
                .ForMember(dest => dest.PuedeGestionarCategoriasEmpresa, opt => opt.MapFrom(src => src.CanManageCompanyCategories))
                .ForMember(dest => dest.PuedeGestionarUsuarios, opt => opt.MapFrom(src => src.CanManageUsers))
                .ForMember(dest => dest.PuedeVerEstadisticas, opt => opt.MapFrom(src => src.CanViewStatistics))
                .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.UltimoLogin, opt => opt.MapFrom(src => src.LastLogin))
                .ForMember(dest => dest.Empresa, opt => opt.Ignore()); // Will be filled separately

            // Mapping para UserNotificationPreferences
            CreateMap<UserNotificationPreferences, UserNotificationPreferencesDto>();
            CreateMap<UserNotificationPreferencesDto, UserNotificationPreferences>();
        }

        private string MapRoleToSimple(UserRole role)
        {
            return role switch
            {
                UserRole.PrincipalAdmin => "admin",
                UserRole.ClientAdmin => "admin",
                UserRole.PrincipalEditor => "editor",
                UserRole.ClientEditor => "editor",
                UserRole.PrincipalViewer => "viewer",
                UserRole.ClientViewer => "viewer",
                _ => "viewer"
            };
        }
    }
}