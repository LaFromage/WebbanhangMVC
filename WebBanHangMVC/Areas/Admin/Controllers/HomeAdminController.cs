using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHangMVC.Areas.Admin.ViewModels;
using WebBanHangMVC.Data;
using WebBanHangMVC.Helpers;
using WebBanHangMVC.ViewModels;

namespace WebBanHangMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin")]
    [Route("Admin/HomeAdmin")]
    public class HomeAdminController : Controller
    {
        private readonly EshopContext db;
        private readonly IMapper _mapper;

        public HomeAdminController(EshopContext context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("DanhMucSanPham")]
        public IActionResult DanhMucSanPham()
        {
            var listSanPham = db.HangHoas.ToList();
            return View(listSanPham);
        }

        [Route("ThemSanPham")]
        [HttpGet]
        public IActionResult ThemSanPham()
        {
            ViewBag.MaLoai = new SelectList(db.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            return View();
        }

        [Route("ThemSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemSanPham(HangHoa sanPham)
        {
            if (ModelState.IsValid)
            {
                db.HangHoas.Add(sanPham);
                db.SaveChanges();
                return RedirectToAction("DanhMucSanPham");
            }
            return View(sanPham);
        }

        [Route("SuaSanPham")]
        [HttpGet]
        public IActionResult SuaSanPham(int maSanPham)
        {
            ViewBag.MaLoai = new SelectList(db.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            var sanPham = db.HangHoas.Find(maSanPham);
            return View(sanPham);
        }

        [Route("SuaSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaSanPham(HangHoa sanPham)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sanPham).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DanhMucSanPham", "HomeAdmin");
            }
            return View(sanPham);
        }

        [Route("XoaSanPham")]
        [HttpGet]
        public IActionResult XoaSanPham(int maSanPham)
        {
            if (maSanPham <= 0)
            {
                // Xử lý lỗi ở đây nếu maSanPham không hợp lệ
                return BadRequest("Mã sản phẩm không hợp lệ");
            }

            var hangHoa = db.HangHoas.Find(maSanPham);
            if (hangHoa == null)
            {
                // Xử lý lỗi ở đây nếu không tìm thấy sản phẩm
                return NotFound("Không tìm thấy sản phẩm");
            }
            db.Remove(hangHoa);
            db.SaveChanges();
            TempData["Message"] = "Sản phẩm đã được xóa";
            return RedirectToAction("DanhMucSanPham", "HomeAdmin");
        }

        [Route("DanhSachHoaDon")]
        public IActionResult DanhSachHoaDon()
        {
            var listHoaDon = db.HoaDons.ToList();
            return View(listHoaDon);
        }

        [Route("DanhSachDanhMucSanPham")]
        public IActionResult DanhSachDanhMucSanPham()
        {
            var listDanhMucSP = db.Loais.ToList();
            return View(listDanhMucSP);
        }

        [Route("ThemDanhMucSanPham")]
        [HttpGet]
        public IActionResult ThemDanhMucSanPham()
        {
            return View();
        }

        [Route("ThemDanhMucSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemDanhMucSanPham(Loai danhMucSP)
        {
            if (ModelState.IsValid)
            {
                db.Loais.Add(danhMucSP);
                db.SaveChanges();
                return RedirectToAction("DanhSachDanhMucSanPham");
            }
            return View(danhMucSP);
        }

        [Route("SuaDanhMucSanPham")]
        [HttpGet]
        public IActionResult SuaDanhMucSanPham(int maLoai)
        {
            var danhMucSP = db.Loais.Find(maLoai);
            return View(danhMucSP);
        }

        [Route("SuaDanhMucSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaDanhMucSanPham(Loai danhMucSP)
        {
            if (ModelState.IsValid)
            {
                db.Entry(danhMucSP).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DanhSachDanhMucSanPham", "HomeAdmin");
            }
            return View(danhMucSP);
        }

        [Route("XoaDanhMucSanPham")]
        [HttpGet]
        public IActionResult XoaDanhMucSanPham(int maLoai)
        {
            if (maLoai <= 0)
            {
                // Xử lý lỗi ở đây nếu maLoai không hợp lệ
                return BadRequest("Mã sản phẩm không hợp lệ");
            }

            var danhSachSP = db.Loais.Find(maLoai);
            if (danhSachSP == null)
            {
                // Xử lý lỗi ở đây nếu không tìm thấy danh mục thích hợp
                return NotFound("Không tìm thấy sản phẩm");
            }
            db.Remove(danhSachSP);
            db.SaveChanges();
            TempData["Message"] = "Sản phẩm đã được xóa";
            return RedirectToAction("DanhSachDanhMucSanPham", "HomeAdmin");
        }
    }
}