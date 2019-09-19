using AutoMapper;

namespace DbRepository
{
    public class DbRepositoryMappingProfile : Profile
    {
        public DbRepositoryMappingProfile()
        {
            CreateMap<ShowMustNotGoOn.Core.TvShow, DbRepository.Entities.TvShows>()
                .ForMember(
                    dest => dest.MyShowsId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(
                    dest => dest.Id,
                    opt => opt.Ignore());

            CreateMap<DbRepository.Entities.TvShows, ShowMustNotGoOn.Core.TvShow>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => src.MyShowsId));
        }
    }
}
