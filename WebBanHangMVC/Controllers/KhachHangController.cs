using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBanHangMVC.Data;
using WebBanHangMVC.Helpers;
using WebBanHangMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace WebBanHangMVC.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly EshopContext db;
        private readonly IMapper _mapper;

        public KhachHangController(EshopContext context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangKy(RegisterVM model, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var khachHang = _mapper.Map<KhachHang>(model);
                    khachHang.RandomKey = Util.GenerateRandomKey();
                    khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
                    khachHang.HieuLuc = true;
                    khachHang.VaiTro = 0;
                    if (Hinh != null)
                    {
                        khachHang.Hinh = Util.UploadHinh(Hinh, "KhachHang");
                    }
                    db.Add(khachHang);
                    db.SaveChanges();
                    return RedirectToAction("Index", "HangHoa");
                }
                catch (Exception ex)
                {

                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginVM model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (ModelState.IsValid)
            {
                var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.UserName);
                if (khachHang == null)
                {
                    ModelState.AddModelError("loi", "Không có thông tin");
                }
                else
                {
                    if (!khachHang.HieuLuc)
                    {
                        ModelState.AddModelError("loi", "Tài khoản không còn hiệu lực.");
                    }
                    else
                    {
                        if (khachHang.MatKhau != model.Password.ToMd5Hash(khachHang.RandomKey))
                        {
                            ModelState.AddModelError("loi", "Sai thông tin đăng nhập");
                        }
                        else
                        {
                            var claims = new List<Claim> {
                        new Claim(ClaimTypes.Email, khachHang.Email),
                        new Claim(ClaimTypes.Name, khachHang.HoTen),
                        new Claim(MyConstants.CLAM_CUSTOMER_ID, khachHang.MaKh)
                            };

                            if (khachHang.MaKh.ToLower() == "admin")
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "Admin")); // Thêm Claim Role "Admin"
                            }
                            else
                            {
                                claims.Add(new Claim(ClaimTypes.Role, "Customer")); // Thêm Claim Role "Customer"
                            }

                            var claimsIdentity = new ClaimsIdentity(claims,
                                CookieAuthenticationDefaults.AuthenticationScheme);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await HttpContext.SignInAsync(claimsPrincipal);

                            if (khachHang.MaKh.ToLower() == "admin")
                            {
                                // Chuyển hướng đến trang Admin (ví dụ: /Admin/Index)
                                return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
                            }
                            else
                            {
                                if (Url.IsLocalUrl(ReturnUrl))
                                {
                                    return Redirect(ReturnUrl);
                                }
                                else
                                {
                                    // Chuyển hướng đến trang khách hàng (ví dụ: /HangHoa/Index)
                                    return RedirectToAction("Index", "HangHoa");
                                }
                            }
                        }
                    }
                }
            }
            return View();
        }


        [Authorize]
        public IActionResult Profile()
        {
            var MaKh = User.Claims.FirstOrDefault(c => c.Type == MyConstants.CLAM_CUSTOMER_ID)?.Value;
            var customer = db.KhachHangs.FirstOrDefault(kh => kh.MaKh == MaKh);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "HangHoa");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChinhSuaProfile()
        {
            var MaKh = User.Claims.FirstOrDefault(c => c.Type == MyConstants.CLAM_CUSTOMER_ID)?.Value;
            var customer = db.KhachHangs.FirstOrDefault(kh => kh.MaKh == MaKh);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChinhSuaProfile(KhachHang model, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                var MaKh = User.Claims.FirstOrDefault(c => c.Type == MyConstants.CLAM_CUSTOMER_ID)?.Value;
                var customer = db.KhachHangs.FirstOrDefault(kh => kh.MaKh == MaKh);
                if (customer == null)
                {
                    return NotFound();
                }

                // Cập nhật các thông tin khác
                customer.HoTen = model.HoTen;
                customer.Email = model.Email;
                customer.DiaChi = model.DiaChi;
                customer.DienThoai = model.DienThoai;
                customer.NgaySinh = model.NgaySinh;

                // Cập nhật hình ảnh (có thể giữ nguyên hoặc thay đổi)
                customer.Hinh = Util.UploadHinh(Hinh, "KhachHang", customer.Hinh);

                db.Entry(customer).State = EntityState.Modified; // Thêm dòng này
                db.SaveChanges();
                return RedirectToAction("Profile");
            }
            return View(model);
        }

        [Authorize]
        public IActionResult DanhSachHoaDon()
        {
            var MaKh = User.Claims.FirstOrDefault(c => c.Type == MyConstants.CLAM_CUSTOMER_ID)?.Value;
            var hoaDons = db.HoaDons.Where(hd => hd.MaKh == MaKh).ToList();
            Dictionary<int, string> trangThaiMapping = new Dictionary<int, string>()
            {
                { -1, "Khách hàng hủy đơn hàng" },
                { 0, "Mới đặt hàng" },
                { 1, "Chờ giao hàng" },
                { 2, "Đã giao hàng" }
            };
            ViewBag.TrangThaiMapping = trangThaiMapping;
            return View(hoaDons);
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
    }
}