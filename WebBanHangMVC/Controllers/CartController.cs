using Microsoft.AspNetCore.Mvc;
using WebBanHangMVC.Data;
using WebBanHangMVC.ViewModels;
using WebBanHangMVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace WebBanHangMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly EshopContext db;
        public CartController(EshopContext context, PaypalClient paypalClient)
        {
            _paypalClient = paypalClient;
            db = context;
        }

        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MyConstants.CART_KEY) ?? new List<CartItem>();
        public IActionResult Index()
        {
            return View(Cart);
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);

            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);

                if (hangHoa == null)
                {
                    TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
                    return Redirect("/404");
                }

                item = new CartItem
                {
                    MaHh = (int)hangHoa.MaHh,
                    TenHh = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity,
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MyConstants.CART_KEY, gioHang);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);

            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MyConstants.CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            if (Cart.Count == 0)
            {
                return Redirect("/HangHoa");
            }
            ViewBag.PaypalClientdId = _paypalClient.ClientId;
            return View(Cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckOutVM model)
        {

            if (ModelState.IsValid)
            {
                var customerID = HttpContext.User.Claims.SingleOrDefault
                    (p => p.Type == MyConstants.CLAM_CUSTOMER_ID).Value;
                var khachHang = new KhachHang();

                if (model.GiongKhachHang)
                {
                    khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerID);
                }
                var hoadon = new HoaDon
                {
                    MaKh = customerID,
                    HoTen = model.HoTen ?? khachHang.HoTen,
                    DiaChi = model.DiaChi ?? khachHang.DiaChi,
                    DienThoai = model.DienThoai ?? khachHang.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "GRAB",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu
                };
                db.Database.BeginTransaction();

                try
                {
                    db.Add(hoadon);
                    db.SaveChanges();
                    var chiTietHoaDon = new List<ChiTietHd>();
                    foreach (var item in Cart)
                    {
                        chiTietHoaDon.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            MaHh = item.MaHh,
                            GiamGia = 0
                        });
                    }
                    db.AddRange(chiTietHoaDon);
                    db.SaveChanges();
                    db.Database.CommitTransaction();
                    HttpContext.Session.Set<List<CartItem>>(MyConstants.CART_KEY, new List<CartItem>());
                    return View("Success");
                }
                catch
                {
                    db.Database.RollbackTransaction();
                }
            }
            TempData["Message"] = "Mời nhập thông tin";
            return View(Cart);
        }

        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            // Thông tin đơn hàng ở Paypal
            var tongTien = Cart.Sum(p => p.ThanhTien).ToString();
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();
            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string hoTen, string diaChi, string dienThoai, string ghiChu, string orderID, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);
                if (response != null && response.status == "COMPLETED")
                {
                    var hoaDonPaypal = response;
                    var tongTien = hoaDonPaypal.purchase_units.FirstOrDefault()?.amount?.value ?? "0"; // Tổng tiền
                    var donViTienTe = hoaDonPaypal.purchase_units.FirstOrDefault()?.amount?.currency_code ?? "USD"; // Đơn vị tiền tệ
                    var maDonHangThamChieu = hoaDonPaypal.id; // Mã đơn hàng tham chiếu

                    // Tạo đơn hàng mới trong database
                    var customerID = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MyConstants.CLAM_CUSTOMER_ID)?.Value;
                    var khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerID);
                    var hoadon = new HoaDon
                    {
                        MaKh = customerID,
                        HoTen = hoTen ?? khachHang.HoTen,
                        DiaChi = diaChi ?? khachHang.DiaChi,
                        DienThoai = dienThoai ?? khachHang.DienThoai,
                        NgayDat = DateTime.Now,
                        CachThanhToan = "PayPal",
                        CachVanChuyen = "GRAB",
                        MaTrangThai = 0,
                        GhiChu = ghiChu
                    };
                    db.Database.BeginTransaction();
                    try
                    {
                        db.Add(hoadon);
                        db.SaveChanges();
                        var chiTietHoaDon = new List<ChiTietHd>();
                        foreach (var item in Cart)
                        {
                            chiTietHoaDon.Add(new ChiTietHd
                            {
                                MaHd = hoadon.MaHd,
                                SoLuong = item.SoLuong,
                                DonGia = item.DonGia,
                                MaHh = item.MaHh,
                                GiamGia = 0
                            });
                        }
                        db.AddRange(chiTietHoaDon);
                        db.SaveChanges();
                        db.Database.CommitTransaction();
                        HttpContext.Session.Set<List<CartItem>>(MyConstants.CART_KEY, new List<CartItem>());
                        return RedirectToAction("PaymentSuccess");
                    }
                    catch
                    {
                        db.Database.RollbackTransaction();
                        TempData["Message"] = "Có lỗi xảy ra khi xử lý đơn hàng.";
                        return RedirectToAction("Checkout");
                    }
                }
                else
                {
                    TempData["Message"] = "Có lỗi xảy ra khi thanh toán qua PayPal.";
                    return RedirectToAction("Checkout");
                }
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [Authorize]
        public IActionResult PaymentSuccess()
        {
            return View("Success");
        }
    }
}