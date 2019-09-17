using AutoMapper;
using ShowMustNotGoOn.Core;
using ShowMustNotGoOn.MyShowsRepository.Model;

namespace ShowMustNotGoOn.MyShowsRepository
{
    public class MyShowsRepositoryMappingProfile : Profile
    {
        public MyShowsRepositoryMappingProfile()
        {
            CreateMap<Result, TvShow>();
        }
    }
}
