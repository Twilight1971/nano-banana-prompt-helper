# Nano Banana Prompt Helper

Windows desktop wrapper and static WebApp for a local HTML prompt-configuration dashboard. The Windows app opens as a single `.exe`, uses Microsoft Edge WebView2 for modern browser rendering, includes a custom icon/logo/splashscreen, and exports the generated configuration as JSON.

## About

Nano Banana Prompt Helper is a compact desktop tool for building structured image-generation prompt configurations. It combines an editable form-based dashboard with a live JSON preview, so prompt parameters can be adjusted, imported, copied, reset, and exported without manually editing raw JSON.

The project is built around a self-contained HTML app. The same interface is available as a static WebApp in `web/` and as a lightweight Windows WebView2 wrapper in `src/`.

## Screenshot

![Nano Banana Prompt Helper screenshot](docs/app-screenshot.png)

## Features

- Single-file Windows executable output
- Static WebApp with the same design and core functions
- Embedded WebView2 wrapper for the HTML app
- Custom app icon, in-app logo, and centered splashscreen
- Dark technical dashboard UI
- Section navigation, search, live JSON preview, import, copy, download, and reset
- Motion-enhanced UI with reduced-motion accessibility support
- No backend server required

## Repository Structure

```text
.
|-- assets/
|   |-- AppIcon.ico
|   |-- Logo.png
|   `-- Splash.jpg
|-- docs/
|   |-- BUILD.md
|   `-- app-screenshot.png
|-- scripts/
|   `-- build.ps1
|-- src/
|   |-- NanoBananaPromptHelper.App.cs
|   `-- NanoBananaPromptHelper.html
|-- web/
|   |-- index.html
|   |-- Logo.png
|   `-- manifest.webmanifest
|-- .gitignore
`-- README.md
```

## Requirements

- Windows 10 or newer
- Microsoft Edge WebView2 Runtime
- .NET Framework 4.x runtime
- Visual Studio 2022 Community or another installation that provides the WebView2 SDK DLLs

The current build script looks for `csc.exe` in the Windows .NET Framework folder and WebView2 DLLs in common Visual Studio / local package locations.

## Build

Open PowerShell in the repository root and run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

The executable will be created here:

```text
dist-single\NanoBananaPromptHelper.exe
```

Optional: if WebView2 SDK files are stored somewhere else, pass the path:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1 -WebView2SdkDir "C:\path\to\webview2"
```

## Build & Run

1. Clone or download this repository.
2. Open PowerShell in the repository root.
3. Build the Windows app:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

4. Start the generated executable:

```powershell
.\dist-single\NanoBananaPromptHelper.exe
```

## WebApp

The static WebApp lives in:

```text
web\index.html
```

Run it locally with any static file server. For example:

```powershell
python -m http.server 8080 --directory web
```

Then open:

```text
http://127.0.0.1:8080
```

The WebApp uses the same dashboard UI and JSON functions as the Windows app: search, form editing, live preview, import, copy, download, and reset.

To sync the WebApp from the Windows app HTML source after future UI edits, run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\sync-web.ps1
```

## GitHub Pages

This repository includes a GitHub Actions workflow for publishing the WebApp from the `web/` folder:

```text
.github/workflows/pages.yml
```

In GitHub, enable Pages with GitHub Actions as the source, then run the `Deploy Web App` workflow or push changes to `web/`.

Required GitHub setting:

```text
Settings -> Pages -> Build and deployment -> Source -> GitHub Actions
```

After the workflow succeeds, the WebApp will be available at:

```text
https://twilight1971.github.io/nano-banana-prompt-helper/
```

## Development

Most of the UI lives in:

```text
src\NanoBananaPromptHelper.html
```

The WebApp copy lives in:

```text
web\index.html
```

The Windows wrapper lives in:

```text
src\NanoBananaPromptHelper.App.cs
```

After editing the main HTML source, run `scripts\sync-web.ps1` to update the WebApp copy, then run the Windows build script if you also need a fresh `.exe`.

## Notes

- The generated `.exe` embeds the HTML app, logo, splash image, icon resources, and WebView2 loader files.
- At runtime the app extracts internal files into `%LocalAppData%\NanoBananaPromptHelper\Runtime\<process-id>`.
- `dist/` and `dist-single/` are build outputs and are intentionally ignored by Git.
- No generated `.exe` file is committed to the repository. Build locally with `scripts/build.ps1`.
- The WebApp is committed as static files under `web/`.

## License

This project is licensed under the [MIT License](LICENSE).
