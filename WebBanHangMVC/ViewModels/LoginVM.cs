using System.ComponentModel.DataAnnotations;

namespace WebBanHangMVC.ViewModels
{
    public class LoginVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Chưa nhập thông tin")]
        [MaxLength(20, ErrorMessage = "Tối đa 20 ký tự")]
        public string UserName { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Chưa nhập thông tin")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}