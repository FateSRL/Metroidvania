Add-Type -AssemblyName System.Drawing

$outDir = Join-Path (Get-Location) "output\emf_vector_test"
New-Item -ItemType Directory -Force $outDir | Out-Null

$emfPath = Join-Path $outDir "system_module_vector_text.emf"
$previewPath = Join-Path $outDir "system_module_vector_text_preview.png"
$renderedPath = Join-Path $outDir "system_module_vector_text_rendered_from_emf.png"

function Add-PathText {
    param(
        [System.Drawing.Graphics] $Graphics,
        [string] $Text,
        [float] $X,
        [float] $Y,
        [float] $W,
        [float] $H,
        [float] $Size,
        [bool] $Bold = $false
    )
    $family = New-Object System.Drawing.FontFamily "Microsoft YaHei"
    $style = [System.Drawing.FontStyle]::Regular
    if ($Bold) { $style = [System.Drawing.FontStyle]::Bold }
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center
    $format.FormatFlags = [System.Drawing.StringFormatFlags]::NoClip
    $rect = New-Object System.Drawing.RectangleF $X, $Y, $W, $H
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddString($Text, $family, [int]$style, $Size, $rect, $format)
    $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(30, 30, 30))
    $Graphics.FillPath($brush, $path)
    $brush.Dispose()
    $path.Dispose()
    $format.Dispose()
    $family.Dispose()
}

function Draw-Diagram {
    param([System.Drawing.Graphics] $Graphics)
    $Graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $Graphics.Clear([System.Drawing.Color]::White)

    $border = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(45, 45, 45)), 3
    $thin = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(80, 80, 80)), 2
    $fillCenter = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(235, 244, 255))
    $fillNode = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(248, 248, 248))

    Add-PathText $Graphics "系统功能模块图" 250 28 420 55 28 $true

    $center = New-Object System.Drawing.Rectangle 330, 190, 260, 82
    $Graphics.FillRectangle($fillCenter, $center)
    $Graphics.DrawRectangle($border, $center)
    Add-PathText $Graphics "2D类恶魔城Demo" 330 190 260 82 19 $true

    $nodes = @(
        @("玩家控制", 70, 105),
        @("战斗技能", 70, 245),
        @("敌人AI", 380, 105),
        @("关卡机关", 380, 330),
        @("Boss战斗", 700, 105),
        @("UI与音效", 700, 245)
    )

    foreach ($n in $nodes) {
        $rect = New-Object System.Drawing.Rectangle ([int]$n[1]), ([int]$n[2]), 150, 58
        $Graphics.FillRectangle($fillNode, $rect)
        $Graphics.DrawRectangle($thin, $rect)
        Add-PathText $Graphics ([string]$n[0]) ([float]$n[1]) ([float]$n[2]) 150 58 17 $false
    }

    # connectors
    $Graphics.DrawLine($thin, 220, 134, 330, 212)
    $Graphics.DrawLine($thin, 220, 274, 330, 246)
    $Graphics.DrawLine($thin, 455, 163, 455, 190)
    $Graphics.DrawLine($thin, 455, 330, 455, 272)
    $Graphics.DrawLine($thin, 700, 134, 590, 212)
    $Graphics.DrawLine($thin, 700, 274, 590, 246)

    Add-PathText $Graphics "移动 / 跳跃 / 冲刺 / 墙跳" 35 166 220 36 11 $false
    Add-PathText $Graphics "飞剑 / 黑洞 / 弹反" 42 306 205 36 11 $false
    Add-PathText $Graphics "巡逻 / 追踪 / 攻击" 335 66 240 32 11 $false
    Add-PathText $Graphics "落石 / 骷髅王 / 开门" 320 390 270 32 11 $false
    Add-PathText $Graphics "传送 / 法术 / 二阶段激光" 660 166 225 36 11 $false
    Add-PathText $Graphics "血条 / 菜单 / 音效反馈" 650 306 245 36 11 $false

    $border.Dispose()
    $thin.Dispose()
    $fillCenter.Dispose()
    $fillNode.Dispose()
}

# EMF with text converted to vector outlines
$tmp = New-Object System.Drawing.Bitmap 1, 1
$tmpG = [System.Drawing.Graphics]::FromImage($tmp)
$hdc = $tmpG.GetHdc()
$rect = New-Object System.Drawing.Rectangle 0, 0, 920, 460
$metafile = New-Object System.Drawing.Imaging.Metafile $emfPath, $hdc, $rect, ([System.Drawing.Imaging.MetafileFrameUnit]::Pixel), ([System.Drawing.Imaging.EmfType]::EmfPlusDual)
$tmpG.ReleaseHdc($hdc)
$tmpG.Dispose()
$tmp.Dispose()

$g = [System.Drawing.Graphics]::FromImage($metafile)
Draw-Diagram $g
$g.Dispose()
$metafile.Dispose()

# Same drawing as PNG preview
$bmp = New-Object System.Drawing.Bitmap 1840, 920
$g2 = [System.Drawing.Graphics]::FromImage($bmp)
$g2.ScaleTransform(2, 2)
Draw-Diagram $g2
$g2.Dispose()
$bmp.Save($previewPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

# Render the generated EMF back to PNG for checking the actual metafile result
$emfImage = [System.Drawing.Image]::FromFile($emfPath)
$bmp2 = New-Object System.Drawing.Bitmap 1840, 920
$g3 = [System.Drawing.Graphics]::FromImage($bmp2)
$g3.Clear([System.Drawing.Color]::White)
$g3.DrawImage($emfImage, 0, 0, 1840, 920)
$g3.Dispose()
$bmp2.Save($renderedPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp2.Dispose()
$emfImage.Dispose()

Get-Item $emfPath, $previewPath, $renderedPath | Select-Object FullName,Length,LastWriteTime

