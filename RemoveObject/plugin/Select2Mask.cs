// file này chứa logic để convert vùng mà user đã chọn bằng select tool sang mask 0/1, tách riêng thành 1 file để dễ quản lý

// không phải pulgin, tại vì ko có hàm render gì cả vs không dùng effect api
// chỉ nhận dữ liệu từ RemoveObjectEffect.cs truyền vào r trả về mask

// đây mới là skeleton thui, chưa có error handling, tối ưu gì cả
using System.Drawing;
using PaintDotNet;

namespace MyPlugin
{
    public static class Select2Mask
    {
        public static void GenerateMask(
            EffectEnvironmentParameters env,
            int width,
            int height,
            string path)
        {
            // 1. Lấy selection từ Paint.NET
            var selection = env.GetSelection();

            Bitmap mask = new Bitmap(width, height);

            // 2. Duyệt từng pixel
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 3. Nếu pixel nằm trong vùng chọn
                    if (selection.Contains(x, y))
                    {
                        // trắng = vùng cần xóa
                        mask.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        // đen = giữ lại
                        mask.SetPixel(x, y, Color.Black);
                    }
                }
            }

            // 4. Lưu mask
            mask.Save(path);
        }
    }
}