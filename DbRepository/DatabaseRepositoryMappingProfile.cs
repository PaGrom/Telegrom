using AutoMapper;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.DatabaseService.Entities;

namespace ShowMustNotGoOn.DatabaseService
{
    public class DatabaseRepositoryMappingProfile : Profile
    {
        public DatabaseRepositoryMappingProfile()
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
