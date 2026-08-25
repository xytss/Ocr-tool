$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$ciPath = Join-Path $repositoryRoot '.github\workflows\ci.yml'
$releasePath = Join-Path $repositoryRoot '.github\workflows\release.yml'
$linuxReleasePath = Join-Path $repositoryRoot 'eng\release-api-linux.ps1'
$windowsReleasePath = Join-Path $repositoryRoot 'eng\release-windows.ps1'
$linuxPublishPath = Join-Path $repositoryRoot 'eng\publish-api-linux.ps1'

foreach ($path in @($ciPath, $releasePath, $linuxReleasePath))
{
    if (-not (Test-Path -LiteralPath $path -PathType 'Leaf'))
    {
        throw ('发布自动化缺少文件：{0}' -f $path)
    }
}

$ci = Get-Content -LiteralPath $ciPath -Raw
foreach ($requiredText in @('pull_request:', 'push:', 'eng\dotnet.ps1', 'test', 'OcrTool.slnx'))
{
    if ($ci -notmatch [regex]::Escape($requiredText))
    {
        throw ('CI 工作流缺少内容：{0}' -f $requiredText)
    }
}

$release = Get-Content -LiteralPath $releasePath -Raw
foreach ($requiredText in @(
    'tags:',
    'contents: write',
    'eng\release-windows.ps1',
    'eng\release-api-linux.ps1',
    'gh release create',
    'win-x64-portable.zip',
    'win-x64-setup.msi',
    'api-linux-x64.tar.gz'
))
{
    if ($release -notmatch [regex]::Escape($requiredText))
    {
        throw ('Release 工作流缺少内容：{0}' -f $requiredText)
    }
}

foreach ($workflow in @($ci, $release))
{
    if ($workflow -match 'run:\s*&')
    {
        throw 'PowerShell 调用运算符必须放在 YAML 块标量中。'
    }
}

if ($release -match [regex]::Escape('actions/upload-artifact'))
{
    throw 'Release 工作流不应保存大型 Actions Artifact。'
}

$linuxRelease = Get-Content -LiteralPath $linuxReleasePath -Raw
foreach ($requiredText in @('publish-api-linux.ps1', 'api-linux-x64.tar.gz', 'tar.exe'))
{
    if ($linuxRelease -notmatch [regex]::Escape($requiredText))
    {
        throw ('Linux API 发布脚本缺少内容：{0}' -f $requiredText)
    }
}

$windowsRelease = Get-Content -LiteralPath $windowsReleasePath -Raw
$linuxPublish = Get-Content -LiteralPath $linuxPublishPath -Raw
foreach ($script in @($windowsRelease, $linuxPublish))
{
    if ($script -notmatch [regex]::Escape('-p:InformationalVersion'))
    {
        throw '发布脚本未将标签版本写入程序元数据。'
    }

    if ($script -notmatch [regex]::Escape('-p:IncludeSourceRevisionInInformationalVersion=false'))
    {
        throw '发布脚本未关闭版本元数据中的提交指纹。'
    }
}

Write-Output '发布自动化测试通过。'
