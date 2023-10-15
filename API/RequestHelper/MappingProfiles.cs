using API.DTOs;
using API.Entities;
using AutoMapper;
using Task = API.Entities.Task;

namespace API.RequestHelper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<ListBoardDTO, ListBoard>()
            .ForMember(dest => dest.BuyerIDs, opt => opt.MapFrom(src => src.BuyerIDs));
        }
    }
}