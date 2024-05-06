using AutoMapper;
using WebBanHangMVC.Data;
using WebBanHangMVC.ViewModels;

namespace WebBanHangMVC.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterVM, KhachHang>();
            CreateMap<HangHoaVM, HangHoa>();
        }
    }
}