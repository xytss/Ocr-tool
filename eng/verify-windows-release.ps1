param(
    [string]$Version = '1.0.0'
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$releaseRoot = Join-Path $repositoryRoot "artifacts\release\$Version"
$portableRoot = Join-Path $releaseRoot 'OcrTool'
$portableArchive = Join-Path $releaseRoot "OcrTool-$Version-win-x64-portable.zip"
$installer = Join-Path $releaseRoot "OcrTool-$Version-win-x64-setup.msi"

$requiredPortableFiles = @(
    'OcrTool.App.exe',
    'api\OcrTool.Api.exe',
    'coreclr.dll',
    'hostfxr.dll',
    'hostpolicy.dll',
    'models\v5\ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx',
    'models\v6\PP-OCRv6_det_small.onnx',
    'models\v6\PP-OCRv6_rec_small.onnx',
    'models\v6\ppocrv6_dict.txt',
    '使用说明.txt',
    'API使用说明.txt',
    'THIRD-PARTY-NOTICES.txt',
    'portable.flag'
)

foreach ($relativePath in $requiredPortableFiles)
{
    $path = Join-Path $portableRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType 'Leaf'))
    {
        throw "绿色版缺少文件：$relativePath"
    }
}

$apiRootPattern = Join-Path $portableRoot 'api\*'
$unexpectedFiles = Get-ChildItem -LiteralPath $portableRoot -Recurse -File |
    Where-Object {
        ($_.Extension -eq '.pdb' -and $_.FullName -notlike $apiRootPattern) -or
        $_.Name -eq 'settings.json'
    }
if ($unexpectedFiles)
{
    throw "绿色版包含不应分发的文件：$($unexpectedFiles.FullName -join ', ')"
}

if (-not (Test-Path -LiteralPath $portableArchive -PathType 'Leaf'))
{
    throw "缺少绿色版压缩包：$portableArchive"
}

Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
$archive = [System.IO.Compression.ZipFile]::OpenRead($portableArchive)
try
{
    $entryNames = $archive.Entries.FullName
    foreach ($relativePath in $requiredPortableFiles)
    {
        $entryName = "OcrTool/$($relativePath.Replace('\', '/'))"
        if ($entryName -notin $entryNames)
        {
            throw "绿色版压缩包缺少文件：$entryName"
        }
    }
}
finally
{
    $archive.Dispose()
}

if (-not (Test-Path -LiteralPath $installer -PathType 'Leaf'))
{
    throw "缺少安装包：$installer"
}

$executable = Get-Item -LiteralPath (Join-Path $portableRoot 'OcrTool.App.exe')
if (-not $executable.VersionInfo.ProductVersion.StartsWith($Version))
{
    throw "程序版本不正确：$($executable.VersionInfo.ProductVersion)"
}

Write-Host "Windows 发布包验证通过：$Version"
