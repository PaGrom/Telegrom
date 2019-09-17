using AutoMapper;

namespace DbRepository
{
    public class DbRepositoryMappingProfile : Profile
    {
        public DbRepositoryMappingProfile()
        {
            CreateMap<ShowMustNotGoOn.Core.TvShow, DbRepository.Model.TvShow>().ReverseMap();
        }
    }
}
