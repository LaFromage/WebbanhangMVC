using Microsoft.AspNetCore.Mvc;
using WebBanHangMVC.Data;
using WebBanHangMVC.ViewModels;

namespace WebBanHangMVC.ViewComponents
{
    public class MenuLoaiViewComponent : ViewComponent
    {
        private readonly EshopContext db;

        public MenuLoaiViewComponent(EshopContext context) => db = context;

        public IViewComponentResult Invoke()
        {
            var data = db.Loais.Select(lo => new MenuLoaiVM
            {
                MaLoai = lo.MaLoai,
                TenLoai = lo.TenLoai,
                SoLuong = lo.HangHoas.Count
            }).OrderBy(p => p.TenLoai);
            return View(data); //Default.cshtml
        }
    }
}