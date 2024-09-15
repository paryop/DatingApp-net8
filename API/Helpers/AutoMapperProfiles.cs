using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
            .ForMember(m => m.Age, o => o.MapFrom(s=> s.DateOfBirth.CalculateAge()))
            .ForMember(p => p.PhotoUrl, 
                       q => q.MapFrom(r => r.Photos.FirstOrDefault(t => t.IsMain)!.Url));    
            CreateMap<Photo, PhotoDto>();    
        }
    }
}