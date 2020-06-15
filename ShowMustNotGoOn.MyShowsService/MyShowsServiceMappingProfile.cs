using AutoMapper;
using ShowMustNotGoOn.Core.Model;
using ShowMustNotGoOn.MyShowsService.Model;

namespace ShowMustNotGoOn.MyShowsService
{
    public class MyShowsServiceMappingProfile : Profile
    {
        public MyShowsServiceMappingProfile()
        {
            CreateMap<Result, TvShowDescription>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id));

            CreateMap<Result, TvShowInfo>()
                .ForMember(dest => dest.MyShowsId,
                    opt => opt.MapFrom(src => src.Id));
        }
    }
}
