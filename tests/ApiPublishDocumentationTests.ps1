$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$publishScript = Get-Content -LiteralPath (Join-Path $repositoryRoot 'eng\publish-api-linux.ps1') -Raw
$guidePath = Join-Path $repositoryRoot 'docs\API使用说明.txt'

if (-not (Test-Path -LiteralPath $guidePath -PathType Leaf))
{
    throw 'API 发布说明文件不存在。'
}

if ($publishScript -notmatch [regex]::Escape("API使用说明.txt"))
{
    throw 'API 发布脚本未包含 API 发布说明。'
}

Write-Output 'API 发布说明测试通过。'
