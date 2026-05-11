$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$WebDir = Join-Path $RepoRoot "web"

New-Item -ItemType Directory -Force -Path $WebDir | Out-Null
Copy-Item -LiteralPath (Join-Path $RepoRoot "src\NanoBananaPromptHelper.html") -Destination (Join-Path $WebDir "index.html") -Force
Copy-Item -LiteralPath (Join-Path $RepoRoot "assets\Logo.png") -Destination (Join-Path $WebDir "Logo.png") -Force

$IndexPath = Join-Path $WebDir "index.html"
$Html = Get-Content -Raw -Path $IndexPath
if ($Html -notmatch 'manifest\.webmanifest') {
    $Html = $Html -replace '<meta name="viewport" content="width=device-width, initial-scale=1.0">\s*<title>',
        "<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">`r`n<meta name=""theme-color"" content=""#020617"">`r`n<title>"
    $Html = $Html -replace '</title>\s*<style>',
        "</title>`r`n<link rel=""manifest"" href=""manifest.webmanifest"">`r`n<link rel=""icon"" href=""Logo.png"" type=""image/png"">`r`n<style>"
    Set-Content -Path $IndexPath -Value $Html -Encoding UTF8
}

Write-Host "Web app synced to: $WebDir"
