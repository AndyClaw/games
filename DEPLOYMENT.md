# GitHub Pages Deployment Guide

## Prerequisites

- Git installed on your computer
- A GitHub account
- Your Blazor WebAssembly project published

## Step-by-Step Deployment

### 1. Initialize Git Repository (if not already done)

```bash
cd C:\Projects.Net\BlazorApp1
git init
git add .
git commit -m "Initial commit - Snake Game Blazor WebAssembly"
```

### 2. Create GitHub Repository

1. Go to https://github.com/new
2. Create a new repository (e.g., "snake-game" or "blazor-snake-puzzle")
3. **Do NOT** initialize with README, .gitignore, or license (we already have these)
4. Copy the repository URL (e.g., `https://github.com/yourusername/snake-game.git`)

### 3. Connect Local Repository to GitHub

```bash
git remote add origin https://github.com/yourusername/snake-game.git
git branch -M main
git push -u origin main
```

### 4. Enable GitHub Pages

1. Go to your repository on GitHub
2. Click **Settings** → **Pages**
3. Under "Build and deployment":
   - **Source**: Select "GitHub Actions"
   - (The workflow will automatically deploy when you push to main)

### 5. Wait for Deployment

1. Go to the **Actions** tab in your repository
2. You should see "Deploy to GitHub Pages" workflow running
3. Wait for it to complete (usually 1-2 minutes)
4. Once complete, your site will be live at:
   `https://yourusername.github.io/repository-name/`

## Making Updates

After the initial setup, whenever you want to update your site:

```bash
# Make your changes to the code
# Then commit and push:
git add .
git commit -m "Description of your changes"
git push
```

The GitHub Action will automatically build and deploy your changes!

## Troubleshooting

### Site shows 404 or blank page

**Issue**: Base href might not be set correctly.

**Solution**: The GitHub Action automatically updates the base href. Make sure:
- Your repository name matches what's expected
- The workflow completed successfully
- Clear your browser cache and refresh

### Routing doesn't work (404 on refresh)

**Issue**: Single Page Application routing needs configuration.

**Solution**: The 404.html file is already set up. If you still have issues:
- Make sure 404.html exists in your published wwwroot
- Check that .nojekyll file is present

### GitHub Action fails

**Issue**: Build or deployment errors.

**Solution**: Check the Actions tab for error details. Common fixes:
- Make sure your project builds locally: `dotnet build`
- Verify .NET 8.0 SDK compatibility
- Check the workflow file for correct project path

### CSS or JavaScript not loading

**Issue**: File paths might be incorrect for the subdirectory.

**Solution**: The workflow sets the correct base href. If issues persist:
- Check browser console for 404 errors
- Verify all resource paths in index.html are relative (no leading /)

## Local Testing

To test the published version locally before deploying:

```bash
# Publish the project
dotnet publish -c Release -o publish

# Serve the wwwroot folder
cd publish/wwwroot
python -m http.server 8080
# Or use any static file server
```

Open http://localhost:8080 in your browser.

## Custom Domain (Optional)

To use a custom domain:

1. Add a `CNAME` file to `publish/wwwroot` with your domain name
2. Configure your domain's DNS settings to point to GitHub Pages
3. In GitHub Settings → Pages, enter your custom domain

## Files Included for Deployment

- **.github/workflows/deploy.yml**: Automated deployment workflow
- **.gitignore**: Prevents committing build artifacts
- **README.md**: Project documentation
- **publish/wwwroot/.nojekyll**: Tells GitHub Pages not to use Jekyll
- **publish/wwwroot/404.html**: Handles SPA routing

## What Gets Deployed

The GitHub Action:
1. ✅ Checks out your code
2. ✅ Sets up .NET 8.0
3. ✅ Restores dependencies
4. ✅ Publishes in Release mode (optimized)
5. ✅ Updates base href for your repository name
6. ✅ Adds .nojekyll file
7. ✅ Deploys to GitHub Pages

Your users will get:
- 📦 Optimized WebAssembly bundles
- ⚡ Fast static file serving from GitHub's CDN
- 🔒 HTTPS enabled by default
- 🌍 Global availability

## Current Status

✅ Project published to: `C:\Projects.Net\BlazorApp1\publish\wwwroot`
✅ GitHub Actions workflow created
✅ .nojekyll file created
✅ 404.html created for SPA routing
✅ .gitignore configured
✅ README.md with documentation

## Next Steps

1. Create GitHub repository
2. Push your code to GitHub
3. Enable GitHub Pages with GitHub Actions
4. Share your game URL!

Your Snake Puzzle Game will be live and ready to play! 🎮

