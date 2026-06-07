# Time: Animated Blazor WebAssembly Clocks

## Overview

**Time** is a Blazor WebAssembly application that displays and animates a grid of analog clocks in the browser. This project is inspired by the work of the art collective [Humans since 1982](https://www.humanssince1982.com/), particularly their iconic kinetic sculpture [ClockClock 24](https://www.humanssince1982.com/clockclock-24/). ClockClock 24 features 24 analog clocks whose hands are choreographed to create mesmerizing patterns and display digital time through coordinated movement. This project aims to digitally replicate the visual and conceptual experience of ClockClock 24, blending art, technology, form and function.

## Tech Stack

- **Frontend Framework:** [Blazor WebAssembly](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- **Language:** C# (.NET 9)
- **UI/Styling:** CSS (responsive layout)
- **Animation:**
  - C# animation engine (chaining, patterns, orchestration)
  - JavaScript interop for smooth browser-based animations
- **Build System:** .NET SDK 9.0+

## Project Structure

```
Time/
├── AnimationConfig/         # Animation configuration and distribution helpers
├── AnimationEngine/         # Animation engine: orchestrator, managers, patterns
├── Components/              # Clock and related C# components
├── Pages/
│   └── AnalogClock.razor    # Main page rendering the animated clocks
├── wwwroot/
│   ├── css/                 # App styles
│   └── js/animationInterop.js  # JS interop driving the WAAPI animations
├── android/                 # Android screen saver (DreamService + WebView)
├── windows/screensaver/     # Windows screen saver (.scr: WinForms + WebView2)
├── backlog/                 # Planned engineering work (e.g. animation improvements)
├── build-android-assets.{sh,ps1}  # Publish Blazor + sync into the Android assets
├── build-windows.{sh,ps1}         # Publish Blazor + build the Windows .scr
├── Program.cs               # App entry point
├── Time.csproj              # Project file
├── NuGet.config             # NuGet sources (includes .NET 9 preview feed)
└── README.MD                # This file
```

## Features

- 24 animated analog clocks in a responsive grid
- Smooth, orchestrated clock arm animations
- Fullscreen mode prompt for immersive experience
- Modular animation engine (easy to extend with new patterns)
- C# and JavaScript interop for high-performance rendering
- Ships as both an Android (DreamService) and a Windows (`.scr`) screen saver

## Getting Started

### Prerequisites
- [.NET 9 SDK (9.0.300+)](https://dotnet.microsoft.com/download/dotnet/9.0)
- Modern web browser (Edge, Chrome, Firefox, etc.)

### Setup
1. **Clone the repository:**
   ```sh
   git clone https://github.com/GiampaLab/Time.git
   cd Time
   ```
2. **Restore dependencies:**
   ```sh
   dotnet restore
   ```
3. **Build and run:**
   ```sh
   dotnet run
   ```
4. **Open in browser:**
   Visit the URL shown in the terminal (usually `https://localhost:5278`).

### Notes
- If you encounter package resolution errors, ensure you have the included `NuGet.config` with the .NET 9 preview feed.
- For best experience, enable fullscreen when prompted.

## Android Screen Saver

The project includes an Android screen saver (DreamService) that wraps the Blazor app in a WebView. It activates automatically when the device is charging or docked.

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- JDK 17 or 21 ([Microsoft build](https://learn.microsoft.com/en-us/java/openjdk/download) or [Adoptium](https://adoptium.net))
- Android SDK with `platforms;android-34`, `build-tools;34.0.0`, and `platform-tools` (for `adb`), installed via `sdkmanager`

**Android SDK setup** (command-line tools only, no Android Studio required) — download the [command-line tools](https://developer.android.com/studio#command-tools), extract them, then configure `sdkmanager`:

```sh
# Linux / macOS / WSL2
mkdir -p ~/android-sdk/cmdline-tools
cd ~/android-sdk/cmdline-tools && unzip commandlinetools-*.zip && mv cmdline-tools latest

# Add to your shell profile (~/.bashrc or ~/.zshrc)
export ANDROID_HOME=~/android-sdk
export PATH=$PATH:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools

sdkmanager "platforms;android-34" "build-tools;34.0.0" "platform-tools" && sdkmanager --licenses
```

```powershell
# Windows (PowerShell) — extract the tools to C:\android-sdk\cmdline-tools\latest first
$env:ANDROID_HOME = "C:\android-sdk"
$env:Path += ";C:\android-sdk\cmdline-tools\latest\bin;C:\android-sdk\platform-tools"

sdkmanager "platforms;android-34" "build-tools;34.0.0" "platform-tools"; sdkmanager --licenses
```

### Connect your device

Installing the APK needs a connected device, so set this up first. Enable **USB debugging** on the device (Developer options) and connect it over USB — `adb devices` should then list it.

**On WSL2** there's no direct USB access, so first forward the device from the Windows host with [`usbipd-win`](https://github.com/dorssel/usbipd-win). In **Windows PowerShell (Administrator)**:

```powershell
winget install usbipd
usbipd list                          # find your device's BUSID (e.g. 2-4)
usbipd bind --busid <BUSID>          # one-time
usbipd attach --wsl --busid <BUSID>  # repeat after each reconnect
```

`adb devices` inside WSL2 should now list the device. (Alternatively, build the APK in WSL2 and run only `adb install` from native Windows PowerShell, where `adb` is already on the PATH.)

### Build & install

The same three steps publish the app, build the APK, and deploy it to the connected device.

**Linux / macOS / WSL2:**
```sh
# 1. Publish the Blazor app and sync it into the Android assets folder
./build-android-assets.sh

# 2. Build the APK → android/app/build/outputs/apk/debug/app-debug.apk
cd android && ./gradlew assembleDebug

# 3. Install on the device
cd .. && adb install -r android/app/build/outputs/apk/debug/app-debug.apk
```

**Windows (native PowerShell)** — identical, using the `.ps1` / `.bat` equivalents:
```powershell
.\build-android-assets.ps1
cd android; .\gradlew.bat assembleDebug
cd ..; adb install -r android\app\build\outputs\apk\debug\app-debug.apk
```

> The `android/app/src/main/assets/` folder is gitignored — it is generated by `build-android-assets.sh` and must exist before building the APK.

### Enable on the device

1. **Settings → Display & touch → Screen saver**
2. Select **Time**
3. Tap **When to start** and choose **While charging** or **Either**

**Preview immediately** (without waiting for charging): tap **Preview** in the screen saver settings, or via adb:

```sh
adb shell cmd dreams set-active-dream com.giampalab.timescreensaver/.TimeScreenSaverService
adb shell cmd dreams start-dreaming
```

> The screen saver only auto-activates while charging or docked (an Android OS limitation). To trigger it on idle, use an automation app such as Tasker.

### Project Structure (Android)

```
android/
├── app/src/main/
│   ├── kotlin/.../TimeScreenSaverService.kt  # DreamService loading the Blazor WebView
│   ├── assets/                               # Populated by build-android-assets.sh (gitignored)
│   ├── res/xml/dream_info.xml                # Screen saver metadata
│   └── AndroidManifest.xml
└── build.gradle.kts
build-android-assets.sh                       # Publish + sync script
```

## Windows Screen Saver

The project also ships a native Windows screen saver (`windows/screensaver/`): a Windows Forms host that renders the Blazor app in an embedded [WebView2](https://developer.microsoft.com/microsoft-edge/webview2/) control and packages it as a standard `.scr`.

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) on **Windows** — the screen saver targets `net9.0-windows` (Windows Forms), so the executable must be compiled on Windows.
- [WebView2 runtime](https://developer.microsoft.com/microsoft-edge/webview2/) — preinstalled on Windows 11 and recent Windows 10.

### Build

From a **Windows PowerShell** terminal at the repo root:

```powershell
.\build-windows.ps1
```

This publishes the Blazor app, builds a self-contained single-file `win-x64` executable, copies the web assets, and produces `windows/build/Time.scr` alongside its `wwwroot/` folder.

> On Linux/macOS/WSL2 you can run `./build-windows.sh` to publish just the Blazor assets, but the `.scr` itself must still be compiled on Windows with `.\build-windows.ps1`.

### Install

Right-click `windows/build/Time.scr` → **Install** (this opens Screen Saver settings with *Time* selected), or copy `Time.scr` together with its `wwwroot/` folder into `C:\Windows\System32`.

Then choose **Time** under **Settings → Personalization → Lock screen → Screen saver**. The saver handles the standard screen-saver arguments: `/s` (run full-screen), `/p <hwnd>` (settings-dialog preview), and `/c` (configure — it has no options).

## Extending the Project

### Add a new animation pattern

A "pattern" is a posed shape the 24 clocks form, plus the continuous motion it animates
into before the clock returns to showing the time. Patterns are declarative: each one is a
self-contained class implementing `IClockPattern`
(`AnimationEngine/Patterns/IClockPattern.cs`). Adding one is **3 additive steps** — no
switch statements, no enums, no orchestrator edits:

1. **(Optional) Add a pose helper.** In `AnimationConfig/AnimationPatterns.cs`, add a
   `SetXxxPattern(Dictionary<int, Clock> clocks)` that positions the arms (or reuse an
   existing one). Clock IDs are `1..24` in a 3-row × 8-column, column-major grid
   (`column c, row r → id = c*3 + r + 1`; see the "Clocks IDs configuration" table below).

2. **Add the pattern class.** Create `AnimationEngine/Patterns/XxxPattern.cs`:

   ```csharp
   using Microsoft.JSInterop;
   using Time.AnimationConfig;
   using Time.Components;

   namespace Time.AnimationEngine.Patterns;

   public sealed class XxxPattern : IClockPattern
   {
       public string Name => "Xxx";

       public void Pose(Dictionary<int, Clock> clocks) => AnimationPatterns.SetXxxPattern(clocks);

       public IAnimationManager BuildInfinite(IJSRuntime js, Dictionary<int, Clock> clocks) =>
           new InfiniteAnimationManager(js, clocks, () =>
           {
               Components.AnimationConfig Config(int index) => new()
               {
                   Direction = Direction.Anticlockwise,
                   EasingFunction = "ease-in",
                   Duration = 5000,
                   Delay = AnimationConfigs.StaggeredAnimation(index, 0, 40)
               };
               // Distribute the motion using a helper from AnimationConfigs:
               // SetDefaultConfig / SetSpiralConfig / SetCenterOutConfig / SetSnakeConfig ...
               AnimationConfigs.SetDefaultConfig(clocks, Config, Config);
           });
           // Pass jsFunctionName: "..." only when the pattern needs a bespoke JS motion primitive.
   }
   ```

3. **Register it.** Add one line to `PatternRegistry.All`
   (`AnimationEngine/Patterns/PatternRegistry.cs`):

   ```csharp
   new XxxPattern(),
   ```

That's it — the orchestrator picks it up automatically (random selection, no repeats until
all patterns have been shown). The compiler enforces that both `Pose` and `BuildInfinite`
exist, so a pattern can never be half-wired.

## Contribution Guidelines

Contributions are welcomed! To get started:

1. **Fork the repository** and create your feature branch (`git checkout -b feature/YourFeature`).
2. **Write clear, well-documented code** and include tests or demos if possible.
3. **Follow the existing code style** and structure.
4. **Open a pull request** with a clear description of your changes.
5. For major changes, please open an issue first to discuss your proposal.

**Code of Conduct:**
- Be respectful and constructive in all interactions.
- No hate speech, harassment, or discrimination.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

Enjoy building and animating with Time!

## Clocks IDs configuration

| IDs |  |  |  |  |  |  |  |
| ---- | ---- | ---- | ---- | ---- | ---- | ---- | ---- |
| 1 | 4 | 7 | 10 | 13 | 16 | 19 | 22 |
| 2 | 5 | 8 | 11 | 14 | 17 | 20 | 23 |
| 3 | 6 | 9 | 12 | 15 | 18 | 21 | 24 |