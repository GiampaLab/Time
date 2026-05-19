#!/usr/bin/env bash
set -e

echo "Publishing Blazor app..."
dotnet publish -c Release -o publish -p:BlazorEnableCompression=false

echo "Building Windows screensaver..."
dotnet publish windows/screensaver/TimeScreensaver.csproj \
    -c Release -r win-x64 --self-contained false \
    -o windows/build

echo "Copying Blazor assets..."
rm -rf windows/build/wwwroot
cp -r publish/wwwroot windows/build/wwwroot

echo "Creating .scr..."
cp windows/build/TimeScreensaver.exe windows/build/Time.scr

echo ""
echo "Done. Output: windows/build/"
echo "Install: right-click Time.scr -> Install"
echo "  or copy Time.scr + wwwroot/ folder to C:\\Windows\\System32"
