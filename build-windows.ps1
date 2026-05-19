$ErrorActionPreference = "Stop"

Write-Host "Stopping any running screensaver instances..."
Get-Process -Name "TimeScreensaver" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "Time" -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -like "*windows\build*" } | Stop-Process -Force
Start-Sleep -Milliseconds 500

Write-Host "Publishing Blazor app..."
dotnet publish Time.csproj -c Release -o publish -p:BlazorEnableCompression=false

Write-Host "Building Windows screensaver..."
dotnet publish windows/screensaver/TimeScreensaver.csproj `
    -c Release -r win-x64 --self-contained false `
    -o windows/build

Write-Host "Copying Blazor assets..."
if (Test-Path "windows/build/wwwroot") { Remove-Item -Recurse -Force "windows/build/wwwroot" }
Copy-Item -Recurse "publish/wwwroot" "windows/build/wwwroot"

Write-Host "Creating .scr..."
Copy-Item -Force "windows/build/TimeScreensaver.exe" "windows/build/Time.scr"

Write-Host ""
Write-Host "Done. Output: windows/build/"
Write-Host "Install: right-click Time.scr -> Install"
Write-Host "  or copy Time.scr + wwwroot/ folder to C:\Windows\System32"
