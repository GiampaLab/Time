#!/usr/bin/env bash
set -e

echo "Publishing Blazor app..."
dotnet publish Time.csproj -c Release -o publish -p:BlazorEnableCompression=false

echo "Copying Blazor assets to windows/build/wwwroot..."
rm -rf windows/build/wwwroot
mkdir -p windows/build
cp -r publish/wwwroot windows/build/wwwroot

echo ""
echo "Blazor assets ready at windows/build/wwwroot/"
echo ""
echo "The screensaver .exe requires the Windows Desktop SDK and must be"
echo "compiled on Windows. Run this from a Windows PowerShell terminal:"
echo ""
echo "  .\\build-windows.ps1"
