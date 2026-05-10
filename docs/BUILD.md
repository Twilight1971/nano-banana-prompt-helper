# Build Notes

This project intentionally avoids a large Electron or installer setup. It compiles a small WPF host that loads the HTML interface inside WebView2.

## Local Build

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

## Common Issues

### WebView2 DLLs not found

Install Visual Studio 2022 Community with WebView2 tooling, or download the Microsoft WebView2 SDK and pass its folder:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1 -WebView2SdkDir "C:\path\to\webview2"
```

### EXE cannot be overwritten

Close any running `NanoBananaPromptHelper.exe` instance, then build again.

### PowerShell execution policy blocks scripts

Use:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```
