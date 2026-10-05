# Generates the installer artwork (dark indigo glass + neon green glow, same palette as the app).
# Output: installer\assets\wizard-large*.bmp (welcome/finish side panel), wizard-small*.bmp (header icon).
# Run:  powershell -ExecutionPolicy Bypass -File installer\make-wizard-images.ps1
Add-Type -AssemblyName System.Drawing

$root   = Split-Path -Parent $MyInvocation.MyCommand.Path
$assets = Join-Path $root 'assets'
New-Item -ItemType Directory -Force $assets | Out-Null
$iconPath = Join-Path (Split-Path -Parent $root) 'src\CoBu\app.ico'

function C($hex, $a = 255) {
    $h = $hex.TrimStart('#')
    [System.Drawing.Color]::FromArgb($a, [Convert]::ToInt32($h.Substring(0,2),16), [Convert]::ToInt32($h.Substring(2,2),16), [Convert]::ToInt32($h.Substring(4,2),16))
}

function New-Canvas([int]$w, [int]$h) {
    $bmp = New-Object System.Drawing.Bitmap $w, $h, ([System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode      = 'AntiAlias'
    $g.InterpolationMode  = 'HighQualityBicubic'
    $g.TextRenderingHint  = 'ClearTypeGridFit'
    $g.PixelOffsetMode    = 'HighQuality'
    return @{ Bmp = $bmp; G = $g }
}

# Soft radial glow (the same trick the app uses: a radial gradient fading to transparent, no blur effect)
function Add-Glow($g, [float]$cx, [float]$cy, [float]$rx, [float]$ry, $color, [int]$alpha) {
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddEllipse($cx - $rx, $cy - $ry, 2 * $rx, 2 * $ry)
    $br = New-Object System.Drawing.Drawing2D.PathGradientBrush $path
    $br.CenterColor      = [System.Drawing.Color]::FromArgb($alpha, $color)
    $br.SurroundColors   = @([System.Drawing.Color]::FromArgb(0, $color))
    $g.FillPath($br, $path)
    $br.Dispose(); $path.Dispose()
}

function Draw-Background($g, [int]$w, [int]$h) {
    $rect = New-Object System.Drawing.Rectangle 0, 0, $w, $h
    $grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, (C '0D0F1B'), (C '070810'), 90
    $g.FillRectangle($grad, $rect); $grad.Dispose()
}

# System.Drawing.Icon cannot read PNG-compressed .ico frames, so pull the largest frame out by hand
$script:iconBitmap = $null
function Get-IconBitmap {
    if ($null -eq $script:iconBitmap) {
        $b = [System.IO.File]::ReadAllBytes($iconPath)
        $best = 0; $bestLen = 0
        for ($i = 0; $i -lt [BitConverter]::ToUInt16($b, 4); $i++) {
            $len = [BitConverter]::ToUInt32($b, 6 + 16 * $i + 8)
            if ($len -gt $bestLen) { $bestLen = $len; $best = $i }
        }
        $off = [BitConverter]::ToUInt32($b, 6 + 16 * $best + 12)
        $ms = New-Object System.IO.MemoryStream (, $b[$off..($off + $bestLen - 1)])
        $script:iconBitmap = [System.Drawing.Bitmap]::new($ms)
    }
    return $script:iconBitmap
}

function Make-Large([double]$s, [string]$name) {
    $w = [int](164 * $s); $h = [int](314 * $s)
    $c = New-Canvas $w $h; $g = $c.G
    Draw-Background $g $w $h
    Add-Glow $g ($w * 0.95) ($h * 0.10) ($w * 1.25) ($h * 0.30) (C '00E676') 105
    Add-Glow $g ($w * 0.05) ($h * 0.62) ($w * 1.10) ($h * 0.22) (C '00C853') 55
    Add-Glow $g ($w * 0.90) ($h * 1.00) ($w * 1.00) ($h * 0.20) (C '007A33') 70

    # glass card behind the logo
    $cardW = 92 * $s; $cardH = 92 * $s; $cardX = ($w - $cardW) / 2; $cardY = 46 * $s
    $rr = New-Object System.Drawing.Drawing2D.GraphicsPath
    $r = 24 * $s
    $rr.AddArc($cardX, $cardY, $r, $r, 180, 90); $rr.AddArc($cardX + $cardW - $r, $cardY, $r, $r, 270, 90)
    $rr.AddArc($cardX + $cardW - $r, $cardY + $cardH - $r, $r, $r, 0, 90); $rr.AddArc($cardX, $cardY + $cardH - $r, $r, $r, 90, 90); $rr.CloseFigure()
    $g.FillPath((New-Object System.Drawing.SolidBrush (C 'FFFFFF' 22)), $rr)
    $g.DrawPath([System.Drawing.Pen]::new((C 'B9F6CA' 130), [float](1.2 * $s)), $rr)
    $g.DrawImage((Get-IconBitmap), [System.Drawing.Rectangle]::new([int]($cardX + 14 * $s), [int]($cardY + 14 * $s), [int](64 * $s), [int](64 * $s)))

    $sf = New-Object System.Drawing.StringFormat; $sf.Alignment = 'Center'
    $fTitle = New-Object System.Drawing.Font 'Segoe UI Semibold', ([float](26 * $s)), ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
    $fSub   = New-Object System.Drawing.Font 'Segoe UI', ([float](11.5 * $s)), ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
    $g.DrawString('CoBu', $fTitle, [System.Drawing.Brushes]::White, (New-Object System.Drawing.RectangleF 0, ([float](152 * $s)), $w, ([float](40 * $s))), $sf)
    $g.DrawString("Copy Buffer", $fSub, (New-Object System.Drawing.SolidBrush (C '00E676')), (New-Object System.Drawing.RectangleF 0, ([float](190 * $s)), $w, ([float](20 * $s))), $sf)
    $g.DrawString("Умный буфер обмена`nдля Windows", $fSub, (New-Object System.Drawing.SolidBrush (C 'FFFFFF' 150)), (New-Object System.Drawing.RectangleF 0, ([float](214 * $s)), $w, ([float](40 * $s))), $sf)

    # specular rim on the right edge
    $g.DrawLine([System.Drawing.Pen]::new((C 'FFFFFF' 36), [float]1), ($w - 1), 0, ($w - 1), $h)

    $c.Bmp.Save((Join-Path $assets $name), [System.Drawing.Imaging.ImageFormat]::Bmp)
    $c.Bmp.Save((Join-Path $assets ($name -replace '\.bmp$', '.png')), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $c.Bmp.Dispose()
}

function Make-Small([double]$s, [string]$name) {
    $w = [int](55 * $s); $h = [int](55 * $s)
    $c = New-Canvas $w $h; $g = $c.G
    Draw-Background $g $w $h
    Add-Glow $g ($w * 0.8) ($h * 0.2) ($w * 0.9) ($h * 0.9) (C '00E676') 70
    $g.DrawImage((Get-IconBitmap), [System.Drawing.Rectangle]::new([int](7.5 * $s), [int](7.5 * $s), [int](40 * $s), [int](40 * $s)))
    $c.Bmp.Save((Join-Path $assets $name), [System.Drawing.Imaging.ImageFormat]::Bmp)
    $c.Bmp.Save((Join-Path $assets ($name -replace '\.bmp$', '.png')), [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $c.Bmp.Dispose()
}

# 100% / 125% / 150% / 200% DPI variants (Inno Setup picks the closest one)
Make-Large 1.0  'wizard-large.bmp'
Make-Large 1.25 'wizard-large-125.bmp'
Make-Large 1.5  'wizard-large-150.bmp'
Make-Large 2.0  'wizard-large-200.bmp'
Make-Small 1.0  'wizard-small.bmp'
Make-Small 1.25 'wizard-small-125.bmp'
Make-Small 1.5  'wizard-small-150.bmp'
Make-Small 2.0  'wizard-small-200.bmp'
"Wizard images written to $assets"
