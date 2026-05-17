$ErrorActionPreference = "Stop"

$AssetsDir = "android\app\src\main\assets"

Write-Host "Publishing Blazor app..."
dotnet publish -c Release -o publish

Write-Host "Syncing to Android assets..."
if (Test-Path $AssetsDir) { Remove-Item -Recurse -Force $AssetsDir }
New-Item -ItemType Directory -Force -Path $AssetsDir | Out-Null
Copy-Item -Recurse -Force "publish\wwwroot\*" $AssetsDir

Write-Host "Removing pre-compressed files (not needed by Android WebView)..."
Get-ChildItem -Recurse -Path $AssetsDir -Include "*.gz","*.br" | Remove-Item -Force

Write-Host "Done. Assets ready at $AssetsDir"
