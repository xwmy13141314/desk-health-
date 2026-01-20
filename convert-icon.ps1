# 将 PNG 转换为 ICO 格式 - 改进版
Add-Type -AssemblyName System.Drawing

$pngPath = "tubiao\22222.png"
$icoPath = "tubiao\22222.ico"

# 读取 PNG 图像
$image = [System.Drawing.Image]::FromFile((Resolve-Path $pngPath).Path)

# 确保图像大小为标准图标尺寸（16x16, 32x32, 48x48, 256x256）
$sizes = @(16, 32, 48, 256)

# 创建 ICO 文件
$fs = [System.IO.FileStream]::new((Join-Path (Get-Location) $icoPath), [System.IO.FileMode]::Create)
$bw = [System.IO.BinaryWriter]::new($fs)

# ICO 文件头
$bw.Write([UInt16]0)       # 保留字段
$bw.Write([UInt16]1)       # 图像类型 (1 = ICO)
$bw.Write([UInt16]$sizes.Count)  # 图像数量

# 计算数据偏移量
$imageDataOffset = 6 + (16 * $sizes.Count)

# 为每个尺寸写入目录条目并收集图像数据
$imageDatas = @()

foreach ($size in $sizes) {
    # 调整图像大小
    $resized = New-Object System.Drawing.Bitmap($size, $size)
    $graphics = [System.Drawing.Graphics]::FromImage($resized)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.DrawImage($image, 0, 0, $size, $size)
    $graphics.Dispose()

    # 转换为 32-bit BGRA 格式
    $bmp = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.DrawImage($resized, 0, 0)
    $g.Dispose()

    # 锁定位数据
    $rect = [System.Drawing.Rectangle]::FromLTRB(0, 0, $size, $size)
    $bmpData = $bmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

    # 读取像素数据
    $bytes = [Math]::Abs($bmpData.Stride) * $bmpData.Height
    $pixels = [byte[]]::new($bytes)
    [System.Runtime.InteropServices.Marshal]::Copy($bmpData.Scan0, $pixels, 0, $bytes)

    $bmp.UnlockBits($bmpData)

    # 计算图像数据大小（像素数据 + AND mask）
    $dataSize = $bytes + ($size * $size / 8)

    # 写入目录条目
    $widthByte = if ($size -eq 256) { 0 } else { [byte]$size }
    $heightByte = if ($size -eq 256) { 0 } else { [byte]$size }

    $bw.Write([Byte]$widthByte)           # 宽度
    $bw.Write([Byte]$heightByte)          # 高度
    $bw.Write([Byte]0)                    # 颜色数 (0 = >8bpp)
    $bw.Write([Byte]0)                    # 保留字段
    $bw.Write([UInt16]1)                  # 颜色平面
    $bw.Write([UInt16]32)                 # 每像素位数
    $bw.Write([UInt32]$dataSize)          # 图像数据大小
    $bw.Write([UInt32]$imageDataOffset)   # 图像数据偏移量

    # 保存图像数据供后续写入
    $imageDatas += @{
        Pixels = $pixels
        DataSize = $dataSize
        Size = $size
        Bytes = $bytes
    }

    $imageDataOffset += $dataSize

    $resized.Dispose()
    $bmp.Dispose()
}

# 写入所有图像数据
foreach ($data in $imageDatas) {
    $size = $data.Size
    $pixels = $data.Pixels
    $bytes = $data.Bytes

    # BITMAPINFOHEADER
    $bw.Write([UInt32]40)                 # 信息头大小
    $bw.Write([Int32]$size)               # 宽度
    $bw.Write([Int32]($size * 2))         # 高度 (X2 for XOR + AND masks)
    $bw.Write([UInt16]1)                  # 颜色平面
    $bw.Write([UInt16]32)                 # 每像素位数
    $bw.Write([UInt32]0)                  # 压缩方式
    $bw.Write([UInt32]$bytes)             # 图像大小
    $bw.Write([Int32]0)                   # 水平分辨率
    $bw.Write([Int32]0)                   # 垂直分辨率
    $bw.Write([UInt32]0)                  # 颜色数
    $bw.Write([UInt32]0)                  # 重要颜色数

    # 写入 XOR mask (BGRA 像素数据，从底到顶)
    $stride = [Math]::Abs($bytes / $size)
    for ($y = $size - 1; $y -ge 0; $y--) {
        for ($x = 0; $x -lt $stride; $x += 4) {
            $i = $y * $stride + $x
            $bw.Write([Byte]$pixels[$i])        # B
            $bw.Write([Byte]$pixels[$i + 1])    # G
            $bw.Write([Byte]$pixels[$i + 2])    # R
            $bw.Write([Byte]$pixels[$i + 3])    # A
        }
    }

    # 写入 AND mask (全0，因为已有 alpha 通道)
    $andMaskSize = $size * $size / 8
    $andMask = [byte[]]::new([Math]::Ceiling($andMaskSize))
    $bw.Write($andMask)
}

$bw.Close()
$fs.Close()

$image.Dispose()

Write-Host "ICO 文件已创建: $icoPath"
