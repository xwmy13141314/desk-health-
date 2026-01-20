using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    static void Main()
    {
        string basePath = @"D:\work\桌面健康提醒工具";
        string pngPath = Path.Combine(basePath, "tubiao", "22222.png");
        string icoPath = Path.Combine(basePath, "tubiao", "22222.ico");

        using (var image = Image.FromFile(pngPath))
        {
            // 使用单一尺寸 256x256
            int size = 256;
            using (var resized = new Bitmap(size, size))
            {
                using (var g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(image, 0, 0, size, size);
                }

                // 转换为 32-bit BGRA
                using (var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.DrawImage(resized, 0, 0);
                    }

                    // 获取像素数据
                    var rect = new Rectangle(0, 0, size, size);
                    var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    int stride = Math.Abs(bmpData.Stride);
                    int pixelDataSize = stride * size;
                    byte[] pixels = new byte[pixelDataSize];
                    System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixels, 0, pixelDataSize);
                    bmp.UnlockBits(bmpData);

                    // 计算数据大小（像素数据 + AND mask）
                    int andMaskSize = (size * size + 7) / 8;
                    int dataSize = pixelDataSize + andMaskSize;

                    using (var fs = new FileStream(icoPath, FileMode.Create))
                    using (var bw = new BinaryWriter(fs))
                    {
                        // ICO 文件头
                        bw.Write((ushort)0);    // 保留
                        bw.Write((ushort)1);    // 类型 (1 = ICO)
                        bw.Write((ushort)1);    // 图像数量

                        // 图像目录条目
                        bw.Write((byte)0);      // 宽度 (256 = 0)
                        bw.Write((byte)0);      // 高度 (256 = 0)
                        bw.Write((byte)0);      // 颜色数
                        bw.Write((byte)0);      // 保留
                        bw.Write((ushort)1);    // 颜色平面
                        bw.Write((ushort)32);   // 每像素位数
                        bw.Write(dataSize);     // 数据大小
                        bw.Write(22);           // 数据偏移 (6 + 16)

                        // BITMAPINFOHEADER
                        bw.Write(40);           // 头大小
                        bw.Write(size);         // 宽度
                        bw.Write(size * 2);     // 高度 (x2)
                        bw.Write((ushort)1);    // 颜色平面
                        bw.Write((ushort)32);   // 每像素位数
                        bw.Write(0);            // 压缩
                        bw.Write(pixelDataSize);// 图像大小
                        bw.Write(0);            // X 像素/米
                        bw.Write(0);            // Y 像素/米
                        bw.Write(0);            // 颜色数
                        bw.Write(0);            // 重要颜色数

                        // 写入像素数据（从底到顶，BGRA 顺序）
                        for (int y = size - 1; y >= 0; y--)
                        {
                            for (int x = 0; x < stride; x += 4)
                            {
                                int i = y * stride + x;
                                bw.Write(pixels[i]);     // B
                                bw.Write(pixels[i + 1]); // G
                                bw.Write(pixels[i + 2]); // R
                                bw.Write(pixels[i + 3]); // A
                            }
                        }

                        // 写入 AND mask（全0）
                        byte[] andMask = new byte[andMaskSize];
                        bw.Write(andMask);
                    }
                }
            }
        }

        Console.WriteLine($"ICO 文件已创建: {icoPath}");
    }
}
