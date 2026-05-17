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
├── AnimationConfig/         # Animation configuration and utilities
├── AnimationEngine/         # Animation engine, orchestrators, managers
├── Components/              # Clock and related C# components
├── Pages/
│   └── AnalogClock.razor    # Main page rendering the animated clocks
├── wwwroot/
│   ├── css/                 # App styles
│   └── js/animationInterop.js # JS interop for animation
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
- JDK 17 or 21
- Android SDK with the following components (install via `sdkmanager`):
  - `platforms;android-34`
  - `build-tools;34.0.0`
  - `platform-tools` (for `adb`)

**Android SDK quick setup (command-line tools only, no Android Studio required):**
```sh
# Download command-line tools from https://developer.android.com/studio#command-tools
mkdir -p ~/android-sdk/cmdline-tools
cd ~/android-sdk/cmdline-tools
unzip commandlinetools-linux-*.zip
mv cmdline-tools latest

# Add to your shell profile (~/.bashrc or ~/.zshrc)
export ANDROID_HOME=~/android-sdk
export PATH=$PATH:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools

# Install required SDK components
sdkmanager "platforms;android-34" "build-tools;34.0.0" "platform-tools"
sdkmanager --licenses
```

### Build

```sh
# 1. Publish the Blazor app and sync it into the Android assets folder
./build-android-assets.sh

# 2. Build the APK
cd android
./gradlew assembleDebug
```

The APK is output to `android/app/build/outputs/apk/debug/app-debug.apk`.

### Install & Enable

**Install via adb** (USB debugging must be enabled on the device):
```sh
# Run from the repo root
adb install -r android/app/build/outputs/apk/debug/app-debug.apk
```

**Enable the screen saver on the device:**
1. Go to **Settings → Display & touch → Screen saver**
2. Select **Time**
3. Tap **When to start** and choose **While charging** or **Either**

**Test immediately without waiting for charging:**

Option A — via adb:
```sh
adb shell cmd dreams set-active-dream com.giampalab.timescreensaver/.TimeScreenSaverService
adb shell cmd dreams start-dreaming
```

Option B — on the device: tap **Preview** in the Screen saver settings.

**When iterating** (after any change to the Blazor or Android code), run all three steps from the repo root:
```sh
./build-android-assets.sh
cd android && ./gradlew assembleDebug
cd .. && adb install -r android/app/build/outputs/apk/debug/app-debug.apk
```

> **Note:** The screen saver only activates automatically while charging or docked — this is an Android OS limitation. To trigger it on idle without charging, use a third-party automation app such as Tasker.

### Troubleshooting

| Symptom | Fix |
|---|---|
| `adb: no devices/emulators found` on WSL2 | Re-run `usbipd attach --wsl --busid <BUSID>` in PowerShell (Admin) — the attachment drops on device reconnect |
| Device shows as `unauthorized` | On the device go to Developer Options → Revoke USB debugging authorizations, then re-run `adb devices` and accept the prompt |
| Device shows `no permissions` | Run the udev rules setup (see WSL2 section below) |
| Build fails with `25.0.3-ea` error | Java 25-ea is not supported — ensure `JAVA_HOME` points to JDK 17 or 21 |
| `ANDROID_HOME not set` | Add `export ANDROID_HOME=~/android-sdk` to `~/.bashrc` and run `source ~/.bashrc` |

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

> **Note:** The `android/app/src/main/assets/` folder is gitignored — it is generated by `build-android-assets.sh` and must be present before building the APK.

### Building on Windows

#### Option A — WSL2 (recommended)

All Linux instructions above apply unchanged inside a WSL2 terminal. The only extra step is getting `adb` to see your USB-connected Android device, since WSL2 doesn't have direct USB access.

**Install `usbipd-win` on the Windows host** to forward USB devices into WSL2:

```powershell
# In Windows PowerShell (run as Administrator)
winget install usbipd
```

Then attach your device:

```powershell
# List USB devices — find your Android device's BUSID (e.g. 2-4)
usbipd list

# Bind once (one-time, requires admin)
usbipd bind --busid <BUSID>

# Attach to WSL2 (repeat each time you reconnect the device)
usbipd attach --wsl --busid <BUSID>
```

Inside WSL2, confirm the device is visible:
```sh
adb devices
```

Then install the APK from WSL2 as normal:
```sh
adb install android/app/build/outputs/apk/debug/app-debug.apk
```

> **Tip:** If you'd rather skip `usbipd-win`, build the APK in WSL2 and run only the `adb install` step from a native Windows PowerShell terminal where `adb` is already on the PATH.

#### Option B — Native PowerShell

If you prefer to stay entirely on Windows without WSL2:

**Prerequisites:**
- [.NET 9 SDK for Windows](https://dotnet.microsoft.com/download/dotnet/9.0)
- JDK 17 or 21 ([Microsoft build](https://learn.microsoft.com/en-us/java/openjdk/download) or [Adoptium](https://adoptium.net))
- Android command-line tools ([download zip](https://developer.android.com/studio#command-tools)), extracted to e.g. `C:\android-sdk\cmdline-tools\latest`

Set environment variables (add to your PowerShell profile or System Properties):
```powershell
$env:ANDROID_HOME = "C:\android-sdk"
$env:Path += ";C:\android-sdk\cmdline-tools\latest\bin;C:\android-sdk\platform-tools"
```

Install SDK components:
```powershell
sdkmanager "platforms;android-34" "build-tools;34.0.0" "platform-tools"
sdkmanager --licenses
```

**Build:**
```powershell
# Publish Blazor and sync assets
.\build-android-assets.ps1

# Build the APK
cd android
.\gradlew.bat assembleDebug
```

**Install:**
```powershell
adb install app\build\outputs\apk\debug\app-debug.apk
```

## Extending the Project

- **Add new animation patterns:**
  - Implement new classes in `AnimationConfig/` and `AnimationEngine/`.
  - Register and orchestrate them via the `AnimationOrchestrator`.

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