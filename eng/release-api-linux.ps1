param(
    [string]$Version = '1.0.0'
)

$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$publishScript = Join-Path $PSScriptRoot 'publish-api-linux.ps1'
$publishRoot = Join-Path $repositoryRoot 'artifacts\publish\api-linux-x64'
$releaseRoot = Join-Path $repositoryRoot ('artifacts\release\{0}' -f $Version)
$archive = Join-Path $releaseRoot ('OcrTool-{0}-api-linux-x64.tar.gz' -f $Version)

& $publishScript -Version $Version
if ($LASTEXITCODE -ne 0)
{
    throw 'Linux API 发布失败。'
}

$requiredFiles = @(
    'OcrTool.Api',
    'API使用说明.txt',
    'models\v6\PP-OCRv6_det_small.onnx',
    'models\v6\PP-OCRv6_rec_small.onnx',
    'models\v6\ppocrv6_dict.txt'
)

foreach ($relativePath in $requiredFiles)
{
    if (-not (Test-Path -LiteralPath (Join-Path $publishRoot $relativePath) -PathType 'Leaf'))
    {
        throw ('Linux API 发布目录缺少文件：{0}' -f $relativePath)
    }
}

New-Item -ItemType 'Directory' -Path $releaseRoot -Force | Out-Null
& 'tar.exe' -czf $archive -C $publishRoot '.'
if ($LASTEXITCODE -ne 0)
{
    throw 'Linux API 压缩包创建失败。'
}

& 'tar.exe' -tf $archive | Out-Null
if ($LASTEXITCODE -ne 0)
{
    throw 'Linux API 压缩包无法读取。'
}

Write-Host ('Linux API 发布包验证通过：{0}' -f $Version)
