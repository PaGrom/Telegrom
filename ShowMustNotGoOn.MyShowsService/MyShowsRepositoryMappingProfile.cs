using AutoMapper;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.MyShowsService.Model;

namespace ShowMustNotGoOn.MyShowsService
{
    public class MyShowsRepositoryMappingProfile : Profile
    {
        public MyShowsRepositoryMappingProfile()
        {
            CreateMap<Result, TvShow>()
                .ForMember(dest => dest.MyShowsId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore());
        }
    }
}
