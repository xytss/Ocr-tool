$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$releaseScript = Get-Content -LiteralPath (Join-Path $repositoryRoot 'eng\release-windows.ps1') -Raw
$apiGuide = Join-Path $repositoryRoot 'docs\API使用说明.txt'

if ($releaseScript -notmatch [regex]::Escape("OcrTool.Api.csproj"))
{
    throw 'Windows 发布脚本未发布 API 项目。'
}

if ($releaseScript -notmatch [regex]::Escape("api"))
{
    throw 'Windows 发布脚本未将 API 放入独立目录。'
}

if (-not (Test-Path -LiteralPath $apiGuide -PathType Leaf))
{
    throw 'API 文档不存在。'
}

$guide = Get-Content -LiteralPath $apiGuide -Raw
if ($guide -notmatch 'OcrTool\.Api\.exe')
{
    throw 'API 文档未包含 Windows 启动方式。'
}

Write-Output 'Windows API 发布测试通过。'
