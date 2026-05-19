#!/usr/bin/env bash
set -e

ASSETS_DIR="android/app/src/main/assets"

echo "Publishing Blazor app..."
dotnet publish Time.csproj -c Release -o publish -p:BlazorEnableCompression=false

echo "Syncing to Android assets..."
rm -rf "$ASSETS_DIR"
mkdir -p "$ASSETS_DIR"
cp -r publish/wwwroot/. "$ASSETS_DIR/"

echo "Building APK..."
cd android
./gradlew assembleDebug
cd ..

echo "Done. APK at android/app/build/outputs/apk/debug/app-debug.apk"
