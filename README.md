# Snake Game - Blazor WebAssembly

A puzzle-platformer snake game built with Blazor WebAssembly.

## Game Features

- **Gravity-based puzzle mechanics**: The snake falls with gravity and must navigate platforms
- **Multiple block types**: Ground blocks, pushable blocks, bombs, and apples
- **Custom level editor**: Create and share your own levels with a URL-based system
- **Level progression**: Built-in levels with increasing difficulty
- **Animated gravity**: Smooth falling animations for immersive gameplay
- **Local storage**: Save custom levels in your browser

## Gameplay

- Use arrow keys or click the arrow buttons to move the snake
- Reach the purple portal to complete each level
- Eat green apples to grow your snake
- Push gray blocks to solve puzzles
- Avoid bombs (red) and falling off the bottom
- The snake is rigid - if any part is supported, the whole snake stays in place

## How to Play Locally

1. Clone this repository
2. Install .NET 8.0 SDK or later
3. Run `dotnet run --project BlazorApp1`
4. Open your browser to the URL shown in the terminal

## Publishing to GitHub Pages

This project is configured for GitHub Pages deployment.

### Option 1: Manual Deployment

1. Run `dotnet publish -c Release -o publish` from the project root
2. Copy the contents of `publish/wwwroot` to your GitHub Pages repository
3. Commit and push to GitHub

### Option 2: GitHub Actions (Recommended)

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to GitHub Pages

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Publish
      run: dotnet publish BlazorApp1/BlazorApp1.csproj -c Release -o publish
    
    - name: Add .nojekyll
      run: touch publish/wwwroot/.nojekyll
    
    - name: Deploy to GitHub Pages
      uses: peaceiris/actions-gh-pages@v3
      with:
        github_token: ${{ secrets.GITHUB_TOKEN }}
        publish_dir: ./publish/wwwroot
        force_orphan: true
```

### Configuring GitHub Pages

1. Go to your repository Settings → Pages
2. Set Source to "Deploy from a branch"
3. Select the `gh-pages` branch (created by the GitHub Action)
4. Save

Your site will be available at `https://yourusername.github.io/repository-name/`

## Project Structure

- **BlazorApp1/Pages**: Game pages (SnakeGame, CustomLevel, Home)
- **BlazorApp1/Components**: Reusable components (GameGrid)
- **BlazorApp1/Services**: Game engine logic (SnakeGameEngine)
- **BlazorApp1/Models**: Data models (Position, LevelConfig)
- **BlazorApp1/wwwroot**: Static assets (CSS, images)

## Technologies

- Blazor WebAssembly (.NET 8.0)
- C#
- CSS3
- JavaScript Interop (for keyboard handling)
- Local Storage API

## License

This project is open source. Feel free to use and modify it.

