// file này chứa full core logic của plugin
// từ việc lấy ảnh từ Paint.NET, tạo mask, gọi Python chạy ONNX, rồi render lại kết quả lên Paint.NET
// đây mới là skeleton thui, chưa có error handling, tối ưu gì cả

using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.PropertySystem;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace MyPlugin
{
    public class RemoveObjectEffect : PropertyBasedEffect
    {
        public RemoveObjectEffect()
            : base("Remove Object (LaMa)", null, "AI", new EffectOptions())
        {
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            return new PropertyCollection();
        }

        protected override void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            // 1. Lấy ảnh từ Paint.NET
            var src = this.SrcArgs.Surface;
            var dst = this.DstArgs.Surface;

            int width = src.Width;
            int height = src.Height;

            // Tạo file tạm để lưu ảnh gốc, mask, và kết quả
            string tempImage = "input.png";
            string tempMask = "mask.png";
            string outputImage = "output.png";

            // 2. Export ảnh gốc ra file PNG
            Bitmap bmp = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ColorBgra c = src[x, y];
                    bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B));
                }
            }

            bmp.Save(tempImage);

            // 3. Tạo mask từ brush (gọi Select2Mask trong Select2Mask.cs)
            mask = Select2Mask.GenerateMask(this.Environment, width, height, tempMask);

            // 4. Gọi Python chạy ONNX 
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "python";
            psi.Arguments = $"../modelLaMa/inference_lama.py {tempImage} {tempMask} {outputImage}";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            Process p = Process.Start(psi);
            p.WaitForExit();

            // 5. Load ảnh output
            Bitmap result = new Bitmap(outputImage);

            // 6. Render lại lên Paint.NET
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var c = result.GetPixel(x, y);
                    dst[x, y] = ColorBgra.FromBgra(c.B, c.G, c.R, c.A);
                }
            }
        }
    }
}