param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'build'))
$ErrorActionPreference = 'Stop'
$outputRoot = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
Add-Type -AssemblyName System.Drawing
$iconFile = Join-Path $PSScriptRoot 'app.ico'
if (-not (Test-Path -LiteralPath $iconFile)) {
    $bitmap = New-Object Drawing.Bitmap 64,64
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([Drawing.Color]::FromArgb(16,21,32))
    $points = [Drawing.PointF[]]@((New-Object Drawing.PointF 13,8),(New-Object Drawing.PointF 14,47),(New-Object Drawing.PointF 25,36),(New-Object Drawing.PointF 35,57),(New-Object Drawing.PointF 44,52),(New-Object Drawing.PointF 34,32),(New-Object Drawing.PointF 51,32))
    $brush = New-Object Drawing.SolidBrush ([Drawing.Color]::FromArgb(131,232,205))
    $graphics.FillPolygon($brush,$points)
    $icon = [Drawing.Icon]::FromHandle($bitmap.GetHicon())
    $stream = [IO.File]::Create($iconFile)
    $icon.Save($stream)
    $stream.Dispose(); $icon.Dispose(); $brush.Dispose(); $graphics.Dispose(); $bitmap.Dispose()
}
$sourceRoot = Join-Path $PSScriptRoot 'src'
$sources = @('Core.cs','Art.cs','ArtExtras.cs','ArtSecondExtras.cs','NewSkinsTests.cs','UI.cs','Features.cs','Reliability.cs','ReliabilityTests.cs','Sizing.cs','SizingTests.cs','ActualCursors.cs','App.cs','AssemblyInfo.cs') | ForEach-Object { Join-Path $sourceRoot $_ }
$installerSource = Join-Path $sourceRoot 'Installer.cs'
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ /utf8output "/win32icon:$iconFile" "/win32manifest:$PSScriptRoot\app.manifest" "/out:$outputRoot\CursorAtelier.exe" /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Web.Extensions.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Net.Http.dll $sources
if ($LASTEXITCODE -ne 0) { throw 'Application build failed' }
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ /utf8output "/win32icon:$iconFile" "/win32manifest:$PSScriptRoot\app.manifest" "/resource:$outputRoot\CursorAtelier.exe,CursorAtelier.exe" "/out:$outputRoot\CursorAtelier-Setup.exe" /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:Microsoft.CSharp.dll $installerSource
if ($LASTEXITCODE -ne 0) { throw 'Installer build failed' }
Write-Output 'Application and installer built.'
