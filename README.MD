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