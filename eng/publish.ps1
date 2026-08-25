$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$dotnetScript = Join-Path $PSScriptRoot 'dotnet.ps1'
$project = Join-Path $repositoryRoot 'src\OcrTool.App\OcrTool.App.csproj'
$output = Join-Path $repositoryRoot 'artifacts\publish\win-x64'

& $dotnetScript publish $project -c 'Release' -r 'win-x64' --self-contained 'false' -o $output

Copy-Item (Join-Path $repositoryRoot 'docs\portable.flag') (Join-Path $output 'portable.flag') -Force
