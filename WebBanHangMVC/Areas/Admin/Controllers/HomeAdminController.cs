using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHangMVC.Data;
using X.PagedList;

namespace WebBanHangMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin")]
    [Route("Admin/HomeAdmin")]
    [Authorize(Policy = "RequireAdminRole")]
    public class HomeAdminController : Controller
    {
        private readonly EshopContext db;
        private readonly IMapper _mapper;

        public HomeAdminController(EshopContext context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]  // Chỉ định action này chỉ xử lý request GET
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [Route("DanhMucSanPham")]
        public IActionResult DanhMucSanPham(int? page, string searchString)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var listSanPham = db.HangHoas.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                listSanPham = listSanPham.Where(hh => hh.TenHh.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            listSanPham = listSanPham.OrderBy(hh => hh.MaHh);

            return View(listSanPham.ToPagedList(pageNumber, pageSize));
        }

        [Authorize]
        [Route("ThemSanPham")]
        [HttpGet]
        public IActionResult ThemSanPham()
        {
            ViewBag.MaLoai = new SelectList(db.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            return View();
        }

        [Authorize]
        [Route("ThemSanPham")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemSanPham(HangHoa sanPham, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Hinh != null && Hinh.Length > 0)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Hinh.FileName);
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa", uniqueFileName);
                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            Hinh.CopyTo(stream);
                        }
                        sanPham.Hinh = uniqueFileName;
                    }
                    db.HangHoas.Add(sanPham);
                    db.SaveChanges();
                    return RedirectToAction("DanhMucSanPham");
                }
                catch (Exception ex)
                {
                    // Log or handle the error as needed
                }
            }
            return View(sanPham);
        }

        [Authorize]
        [Route("SuaSanPham")]
        [HttpGet]
        public IActionResult SuaSanPham(int maSanPham)
        {
            ViewBag.MaLoai = new SelectList(db.Loais.ToList(), "MaLoai", "TenLoai");
            ViewBag.MaNcc = new SelectList(db.NhaCungCaps.ToList(), "MaNcc", "TenCongTy");
            var sanPham = db.HangHoas.Find(maSanPham);
            return View(sanPham);
        }

        [Authorize]
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

        [Authorize]
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

        [Authorize]
        [Route("DanhSachDanhMucSanPham")]
        public IActionResult DanhSachDanhMucSanPham(int? page, string searchString)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var listDanhMucSP = db.Loais.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                listDanhMucSP = listDanhMucSP.Where(dm => dm.TenLoai.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            listDanhMucSP = listDanhMucSP.OrderBy(dm => dm.MaLoai);

            return View(listDanhMucSP.ToPagedList(pageNumber, pageSize));
        }

        [Authorize]
        [Route("ThemDanhMucSanPham")]
        [HttpGet]
        public IActionResult ThemDanhMucSanPham()
        {
            return View();
        }

        [Authorize]
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

        [Authorize]
        [Route("SuaDanhMucSanPham")]
        [HttpGet]
        public IActionResult SuaDanhMucSanPham(int maLoai)
        {
            var danhMucSP = db.Loais.Find(maLoai);
            return View(danhMucSP);
        }

        [Authorize]
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
        [Authorize]
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

        [Authorize]
        [Route("DanhSachHoaDon")]
        public IActionResult DanhSachHoaDon(int? page, string searchString)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var listHoaDon = db.HoaDons.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                listHoaDon = listHoaDon.Where(hd => hd.HoTen.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            listHoaDon = listHoaDon.OrderBy(hd => hd.MaHd);

            Dictionary<int, string> trangThaiMapping = new Dictionary<int, string>()
            {
                { -1, "Khách hàng hủy đơn hàng" },
                { 0, "Mới đặt hàng" },
                { 1, "Chờ giao hàng" },
                { 2, "Đã giao hàng" }
            };
            ViewBag.TrangThaiMapping = trangThaiMapping;

            return View(listHoaDon.ToPagedList(pageNumber, pageSize));
        }

        [Authorize]
        [Route("ChiTietHoaDon/{maHd}")]
        public IActionResult ChiTietHoaDon(int maHd)
        {
            var hoaDon = db.HoaDons.Include(h => h.ChiTietHds).ThenInclude(ct => ct.MaHhNavigation).FirstOrDefault(h => h.MaHd == maHd);
            if (hoaDon == null)
            {
                return NotFound();
            }

            return View(hoaDon);
        }

        [Authorize]
        [HttpGet]
        [Route("ChonTrangThai/{maHd}")]
        public IActionResult ChonTrangThai(int maHd)
        {
            ViewBag.DanhSachTrangThai = db.TrangThais.ToList();
            return View(maHd);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ChonTrangThai/{maHd}")]
        public IActionResult ChonTrangThai(int maHd, int maTrangThaiMoi)
        {
            var hoaDon = db.HoaDons.FirstOrDefault(h => h.MaHd == maHd);
            if (hoaDon == null)
            {
                return NotFound();
            }

            hoaDon.MaTrangThai = maTrangThaiMoi;
            db.SaveChanges();
            TempData["Message"] = "Chuyển trạng thái thành công";
            return RedirectToAction("DanhSachHoaDon", "HomeAdmin");
        }

        [Authorize]
        [HttpPost] // Chỉ định action này chỉ xử lý request POST
        [Route("DangXuat")]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect(Url.Action("Index", ""));
        }
    }
}