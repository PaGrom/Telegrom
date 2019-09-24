using AutoMapper;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.DatabaseContext.Entities;

namespace ShowMustNotGoOn.DatabaseContext
{
    public class DatabaseContextMappingProfile : Profile
    {
        public DatabaseContextMappingProfile()
        {
            CreateMap<TvShow, TvShows>()
                .ForMember(
                    dest => dest.MyShowsId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(
                    dest => dest.Id,
                    opt => opt.Ignore());

            CreateMap<TvShows, TvShow>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => src.MyShowsId));

            CreateMap<User, Users>()
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
