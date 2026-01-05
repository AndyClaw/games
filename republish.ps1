# Republish Script for Snake Game
# Run this script whenever you make changes and want to republish

Write-Host "🎮 Snake Game - Republish Script" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to project root
$projectRoot = "C:\Projects.Net\BlazorApp1"
Set-Location $projectRoot

# Clean previous publish
Write-Host "🧹 Cleaning previous publish..." -ForegroundColor Yellow
if (Test-Path "publish") {
    Remove-Item -Recurse -Force "publish"
}

# Publish project
Write-Host "📦 Publishing project in Release mode..." -ForegroundColor Yellow
dotnet publish -c Release -o publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Add .nojekyll
Write-Host "📄 Adding .nojekyll file..." -ForegroundColor Yellow
New-Item -Path "publish\wwwroot\.nojekyll" -ItemType File -Force | Out-Null

# Add 404.html for SPA routing
Write-Host "📄 Creating 404.html..." -ForegroundColor Yellow
$html404 = @'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>BlazorApp1</title>
    <script>
        // Single Page Apps for GitHub Pages
        var pathSegmentsToKeep = 0;
        var l = window.location;
        l.replace(
            l.protocol + '//' + l.hostname + (l.port ? ':' + l.port : '') +
            l.pathname.split('/').slice(0, 1 + pathSegmentsToKeep).join('/') + '/?/' +
            l.pathname.slice(1).split('/').slice(pathSegmentsToKeep).join('/').replace(/&/g, '~and~') +
            (l.search ? '&' + l.search.slice(1).replace(/&/g, '~and~') : '') +
            l.hash
        );
    </script>
</head>
<body>
</body>
</html>
'@
$html404 | Out-File -FilePath "publish\wwwroot\404.html" -Encoding UTF8

Write-Host ""
Write-Host "✅ Publish complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📁 Published files location:" -ForegroundColor Cyan
Write-Host "   $projectRoot\publish\wwwroot" -ForegroundColor White
Write-Host ""
Write-Host "📊 File summary:" -ForegroundColor Cyan
Get-ChildItem "publish\wwwroot" | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize

Write-Host ""
Write-Host "🚀 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Test locally: cd publish\wwwroot && python -m http.server" -ForegroundColor White
Write-Host "   2. Or commit and push to GitHub for automatic deployment" -ForegroundColor White
Write-Host ""
Write-Host "💡 Tip: The GitHub Action will automatically deploy when you push to main!" -ForegroundColor Yellow

