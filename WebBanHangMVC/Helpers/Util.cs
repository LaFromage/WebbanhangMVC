using System.Text;

namespace WebBanHangMVC.Helpers
{
    public class Util
    {
        public static string UploadHinh(IFormFile Hinh, string folder, string oldFileName = null)
        {
            try
            {
                if (Hinh != null && Hinh.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Hinh.FileName);
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder, uniqueFileName);

                    using (var myfile = new FileStream(fullPath, FileMode.Create))
                    {
                        Hinh.CopyTo(myfile);
                    }

                    // Xóa hình ảnh cũ (nếu có)
                    if (!string.IsNullOrEmpty(oldFileName))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder, oldFileName);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    return uniqueFileName; // Trả về tên file mới
                }
                else
                {
                    return oldFileName; // Trả về tên file cũ nếu không có hình ảnh mới
                }
            }
            catch (Exception ex)
            {
                // Xử lý exception (ví dụ: log lỗi)
                return string.Empty;
            }
        }

        public static string GenerateRandomKey(int length = 5)
        {
            var pattern = @"qazwsxedcrfvtgbyhnujmiklopQAZWSXEDCRFVTGBYHNUJMIKLOP!";
            var sb = new StringBuilder();
            var rd = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(pattern[rd.Next(0, pattern.Length)]);
            }

            return sb.ToString();
        }
    }
}