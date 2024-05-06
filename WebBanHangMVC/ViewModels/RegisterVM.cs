using System.ComponentModel.DataAnnotations;

namespace WebBanHangMVC.ViewModels
{
    public class RegisterVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Chưa nhập thông tin")]
        [MaxLength(20, ErrorMessage = "Tối đa 20 ký tự")]
        public string MaKh { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Chưa nhập thông tin")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Chưa nhập thông tin")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 ký tự")]
        public string HoTen { get; set; }

        public bool GioiTinh { get; set; } = true;

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Chưa nhập thông tin")]
        [MaxLength(60, ErrorMessage = "Tối đa 60 ký tự")]
        public string DiaChi { get; set; }

        [Display(Name = "Điện thoại")]
        [Required(ErrorMessage = "Chưa nhập thông tin")]
        [MaxLength(24, ErrorMessage = "Tối đa 24 ký tự")]
        [RegularExpression(@"0[987654321]\d{8}", ErrorMessage = "Mời nhập đúng số")]
        public string DienThoai { get; set; }

        [Required(ErrorMessage = "Chưa nhập thông tin")]
        [EmailAddress(ErrorMessage = "Chưa đúng định dạng email")]
        public string Email { get; set; }

        public string? Hinh { get; set; }
    }
}