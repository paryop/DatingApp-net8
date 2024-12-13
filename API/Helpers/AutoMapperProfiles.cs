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
            CreateMap<MemberUpdateDto, AppUser>(); 
            CreateMap<RegisterDto, AppUser>(); 
            CreateMap<string,DateOnly>().ConvertUsing(s=> DateOnly.Parse(s));
            CreateMap<Message, MessageDto>()
            .ForMember(d => d.SenderPhotoUrl, 
                        o => o.MapFrom(s => s.Sender.Photos.FirstOrDefault(x=> x.IsMain)!.Url))  
            .ForMember(d => d.RecipientPhotoUrl, 
                        o => o.MapFrom(s => s.Recipient.Photos.FirstOrDefault(x=> x.IsMain)!.Url)); 
            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
        }
    }
}