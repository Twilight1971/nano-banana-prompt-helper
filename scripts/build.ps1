param(
    [string]$OutputName = "NanoBananaPromptHelper.exe",
    [string]$OutputDir = "dist-single",
    [string]$WebView2SdkDir = ""
)

$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$SrcDir = Join-Path $RepoRoot "src"
$AssetsDir = Join-Path $RepoRoot "assets"
$OutDir = Join-Path $RepoRoot $OutputDir

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$Csc = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $Csc)) {
    throw "csc.exe wurde nicht gefunden: $Csc"
}

$WpfDir = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\WPF"
$SystemXaml = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\System.Xaml.dll"

function Find-WebView2File {
    param(
        [string]$FileName
    )

    $candidates = @()

    if ($WebView2SdkDir) {
        $candidates += Get-ChildItem -Path $WebView2SdkDir -Recurse -Filter $FileName -ErrorAction SilentlyContinue
    }

    $packagesDir = Join-Path $RepoRoot "packages"
    if (Test-Path $packagesDir) {
        $candidates += Get-ChildItem -Path $packagesDir -Recurse -Filter $FileName -ErrorAction SilentlyContinue
    }

    $vsPrivate = Join-Path ${env:ProgramFiles} "Microsoft Visual Studio\2022\Community\Common7\IDE\PrivateAssemblies\$FileName"
    if (Test-Path $vsPrivate) {
        return $vsPrivate
    }

    $wslSettings = Join-Path ${env:ProgramFiles} "WSL\wslsettings\$FileName"
    if (Test-Path $wslSettings) {
        return $wslSettings
    }

    if ($candidates.Count -gt 0) {
        return $candidates[0].FullName
    }

    throw "$FileName wurde nicht gefunden. Installiere Visual Studio 2022 mit WebView2 SDK oder uebergib -WebView2SdkDir."
}

$WebView2Core = Find-WebView2File "Microsoft.Web.WebView2.Core.dll"
$WebView2Wpf = Find-WebView2File "Microsoft.Web.WebView2.Wpf.dll"
$WebView2Loader = Find-WebView2File "WebView2Loader.dll"
$OutputPath = Join-Path $OutDir $OutputName

& $Csc `
    /nologo `
    /target:winexe `
    /platform:x64 `
    /win32icon:"$(Join-Path $AssetsDir 'AppIcon.ico')" `
    /out:"$OutputPath" `
    /resource:"$(Join-Path $SrcDir 'NanoBananaPromptHelper.html'),NanoBananaPromptHelper.Resources.NanoBananaPromptHelper.html" `
    /resource:"$(Join-Path $AssetsDir 'Logo.png'),NanoBananaPromptHelper.Resources.Logo.png" `
    /resource:"$(Join-Path $AssetsDir 'Splash.jpg'),NanoBananaPromptHelper.Resources.Splash.jpg" `
    /resource:"$WebView2Core,NanoBananaPromptHelper.Resources.Microsoft.Web.WebView2.Core.dll" `
    /resource:"$WebView2Wpf,NanoBananaPromptHelper.Resources.Microsoft.Web.WebView2.Wpf.dll" `
    /resource:"$WebView2Loader,NanoBananaPromptHelper.Resources.WebView2Loader.dll" `
    /reference:"$WebView2Core" `
    /reference:"$WebView2Wpf" `
    /reference:"$(Join-Path $WpfDir 'PresentationCore.dll')" `
    /reference:"$(Join-Path $WpfDir 'PresentationFramework.dll')" `
    /reference:"$(Join-Path $WpfDir 'WindowsBase.dll')" `
    /reference:"$SystemXaml" `
    "$(Join-Path $SrcDir 'NanoBananaPromptHelper.App.cs')"

if ($LASTEXITCODE -ne 0) {
    throw "Build fehlgeschlagen."
}

Write-Host "Build erfolgreich: $OutputPath"
