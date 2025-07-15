using MeetApp.DataEntities.DTOs;
using MeetApp.DataEntities.Entities;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfile : Profile{
    
    public AutoMapperProfile(){
        CreateMap<AppUser, UserMember>();
        CreateMap<AppUser, AboutMember>();
        CreateMap<Interest, InterestDTO>();
        CreateMap<Post, PostDTO>();
    }
}