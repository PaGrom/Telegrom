using AutoMapper;
using ShowMustNotGoOn.DatabaseContext.Model;
using ShowMustNotGoOn.MyShowsService.Model;

namespace ShowMustNotGoOn.MyShowsService
{
    public class MyShowsServiceMappingProfile : Profile
    {
        public MyShowsServiceMappingProfile()
        {
            CreateMap<Result, TvShow>()
                .ForMember(dest => dest.MyShowsId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore());
        }
    }
}
