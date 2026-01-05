# Snake Game - Project Specification

## Overview
A classic snake game implemented in Blazor WebAssembly where the player controls a snake moving around a grid using arrow keys or on-screen buttons.

## Game Components

### 1. Game Grid
- **Size**: 20x20 blocks
- **Visual**: Clear visible border around the grid
- **Rendering**: CSS Grid or similar layout system

### 2. Snake
- **Initial Length**: 3 segments (1 head + 2 body segments)
- **Starting Position**: Within the game grid (e.g., center or near center)
- **Head Color**: Blue
- **Body Color**: Green
- **Representation**: Simple geometric shapes (squares/rectangles)

### 3. Movement
- **Direction**: Up, Down, Left, Right
- **Control Methods**: 
  - Keyboard: Arrow keys
  - Mouse/Touch: Clickable arrow buttons on the page
- **Movement Type**: Manual - one block per arrow press (no automatic movement)
- **Mechanics**: 
  - User presses arrow to move the head one block
  - Each body segment follows the segment in front of it
  - Segments advance to the position of the segment they're attached to
  - **Self-Collision**: Snake cannot move onto its own body (effectively prevents moving into itself)

### 3.1 Gravity System
- **Automatic**: Gravity applies after each player move
- **Snake Behavior**:
  - Snake body is **rigid** - maintains its connected shape
  - If any segment is supported (on ground block or pushable block), entire snake stays in place
  - If no support, entire snake falls down as one rigid unit
  - Falls until any segment touches support
  - **Bottom edge (row 19) provides NO support** - falling to the bottom = **game over**
- **Pushable Block Behavior**:
  - Gray blocks fall independently
  - Each block falls until it hits ground, another block, snake, or **grid floor (row 19)**
  - Blocks CAN rest on the floor (unlike snake)
  - Can fall onto and cover bombs
- **Support Rules**:
  - **For Snake**: Only ground blocks and pushable blocks provide support (NOT floor)
  - **For Pushable Blocks**: Grid floor (row 19), ground blocks, other pushable blocks, and snake segments provide support
  - Portal and apples do NOT provide support for anything

### 4. Apple (Food)
- **Optional**: Not all levels require apples
- **Multiple**: Levels can have multiple apples
- **Visual**: Green circular objects on the grid
- **Behavior**: 
  - **Collision**: Acts as a solid block - snake body, pushable blocks, and other objects cannot pass through it
  - **Support**: Provides support for the snake and pushable blocks (prevents falling)
  - **Eating**: When snake **head** moves onto apple, apple disappears and snake grows by 1 segment
  - Snake length increases by 1 segment per apple eaten
  - Each apple collected grows the snake independently
  - Apple positions are fixed per level

### 5. End Portal
- **Required**: Every level has an end portal
- **Visual**: Darker purple gradient square (distinguishable from other blocks)
- **Behavior**: 
  - When snake head moves over portal, level is won
  - Player advances to next level or sees victory message

### 6. Block Types

#### Ground Blocks (Brown)
- **Visual**: Brown solid blocks
- **Behavior**: 
  - Immovable obstacles
  - Snake cannot move onto them
  - Gray blocks cannot be pushed onto them
  - Nothing can pass through them

#### Pushable Blocks (Gray)
- **Visual**: Gray blocks
- **Behavior**: 
  - Can be pushed by the snake
  - When snake tries to move onto a gray block, the block is pushed in that direction
  - Block only moves if the destination is empty (no snake, no other blocks, no walls)
  - Can be pushed onto bombs (covering them safely)
  - Can be pushed off bombs (revealing the bomb again)
  - Multiple blocks cannot be pushed simultaneously (can't push a chain)

#### Bombs (Orange with Black)
- **Visual**: Orange center with black outer ring (radial gradient)
- **Behavior**: 
  - Instant game over if snake touches them
  - Can be covered by pushing a gray block onto them
  - Gray block on bomb is safe (bomb stays underneath)
  - If gray block is pushed off, bomb is revealed and dangerous again
  - Snake hitting an uncovered bomb ends the game

### 7. Level System
- **Level Selection**: Player can choose which level to play from main menu
- **Navigation**: Direct URL navigation to specific levels (e.g., `/snake/1`, `/snake/2`)
- **Pre-built Levels**: 12 built-in levels with increasing difficulty and various mechanics
- **Custom Level Creator**: 
  - Accessible from level select screen or via "Customize" button on any level
  - **Visual Editor**: Click-based interface with block type selector toolbar
    - Select a block type (Snake, Apple, Portal, Ground, Pushable, Bomb)
    - Click in grid to place selected block type
    - Click existing block to remove it (automatically switches to that block type)
    - Live preview shows level as you build it
    - Tooltips show coordinate and block type on hover
  - **Description Field**: Text input for level name/description
  - **Validation Rules**:
    - End portal position is required
    - Snake segments are required (at least 1 segment)
    - Ground blocks are required (minimum 3 blocks)
    - Apples, pushable blocks, and bombs are optional
    - All positions must be within grid bounds (0-19)
    - Position format validated (row,col)
    - Automatically filters empty/whitespace entries in position lists
  - **Error Messages**: Specific error messages indicating which section has issues
  - "Play" button to test the level (validates before starting)
  - "Retry" button available during gameplay to restart current level
  - "Share" button generates a shareable URL (validates before sharing)
  - "Save" button stores level to browser local storage
  - **Local Storage**: Custom levels persist in browser
    - Save levels with custom names
    - Load previously saved levels from list
    - Delete saved levels
    - Levels stored client-side (no server required)
- **URL Sharing**:
  - Levels encoded as URL query parameters
  - Share URL with others to play custom levels
  - Auto-play when visiting a shared level URL
  - No server storage required - level data in URL
- **Level Configuration**: Each level has predefined:
  - Description (level name)
  - Apple positions (optional - some levels may have no apples)
  - End portal position (required)
  - Snake starting position and initial segments
  - Ground blocks (brown, immovable)
  - Pushable blocks (gray, can be moved)
  - Bombs (dangerous obstacles)
- **Pre-built Levels**: 12 progressively challenging levels:
  - **Level 1**: `D=Jump the gap&E=17,14&S=17,3;17,2;17,1&G=18,2;18,3;18,4;18,5;18,6;18,9;18,10;18,11;18,12;18,13;18,14;18,15`
    - Tutorial: Jump a 2-block gap between platforms
  - **Level 2**: `D=Climb the cliff&E=14,14&S=17,2;17,1;18,1&G=18,1;18,2;18,3;18,4;18,5;18,6;18,7;17,8;16,8;15,8;15,9;15,10;15,11;15,12;15,13;15,14;15,15;15,16&P=17,5`
    - Push block against cliff to create stepping stone
  - **Level 3**: `D=Cover the bomb&E=17,15&S=17,2;17,1&G=18,1;18,2;18,3;18,4;18,5;18,6;18,7;18,8;18,9;18,10;18,11;18,12;18,13;18,14;18,15;18,16&P=17,5&B=17,10`
    - Push block over bomb to safely cross
  - **Level 4**: `D=Collect all apples&E=10,10&S=17,2;17,1;18,1&A=17,6;15,10;17,14&G=18,1;18,2;18,3;18,4;18,5;18,6;18,7;18,8;18,9;18,10;18,11;18,12;16,9;16,10;16,11;18,13;18,14&P=17,8`
    - Collect 3 apples at different heights using blocks
  - **Level 5-12**: Additional levels with cave navigation, order puzzles, trust falls, u-turns, shape challenges, stairways, and precision bomb avoidance

#### Compact Level String Format
- **Purpose**: Human-readable but compact level representation for easy level creation and sharing
- **Format**: URL query string style with sections separated by ampersands (`&`) and key-value pairs with equals (`=`):
  - `D` - Description (URL-encoded alphanumeric text)
  - `E` - End portal position (row,col)
  - `S` - Snake segments (row,col;row,col;... with head first)
  - `A` - Apples (row,col;row,col;...)
  - `G` - Ground blocks (row,col;row,col;...)
  - `P` - Pushable blocks (row,col;row,col;...)
  - `B` - Bombs (row,col;row,col;...)
- **Position Format**: `row,col` (comma-separated integers)
- **Multiple Positions**: Separated by semicolons (`;`)
- **Optional Sections**: Empty sections (like bombs or apples) can be omitted entirely
- **Example**: 
  ```
  D=Jump%20the%20gap&E=17,14&S=17,3;17,2;17,1&G=18,2;18,3;18,4;18,5;18,6;18,9;18,10;18,11;18,12;18,13;18,14;18,15
  ```
  This creates a level with:
  - Description: "Jump the gap"
  - Portal at (17,14)
  - Snake starting at (17,3) head, (17,2), (17,1) body
  - Ground blocks creating two platforms with a gap
  - No apples, pushable blocks, or bombs

### 8. Game State
- Snake position (array of coordinates)
- Current level
- Apple positions (list - can be multiple per level)
- End portal position
- Ground blocks (immovable obstacles)
- Pushable blocks (can be moved by snake)
- Bombs (game over if touched)
- Level completion status
- Game over state
- Gravity system (applies after each move)
- Manual movement (no automatic game loop)

## Implementation Details

### Completed Features
- [x] Create game grid with visible border (20x20 blocks)
- [x] Render snake with head (blue) and body segments (green)
- [x] Implement movement controls (keyboard arrow keys and on-screen buttons)
- [x] Snake movement logic (segments follow each other)
- [x] Manual movement - one block per arrow press
- [x] Self-collision prevention (snake cannot move onto itself)
- [x] Apple system - collect to grow snake (green circles)
- [x] Multiple apples per level support
- [x] End portal - reach to win level (darker purple gradient)
- [x] Level system - 12 playable levels with fixed layouts
- [x] Level selection UI with clickable level cards
- [x] Direct URL navigation to specific levels (`/snake/1`, `/snake/2`, etc.)
- [x] Level completion and progression
- [x] Ground blocks - immovable brown obstacles
- [x] Pushable gray blocks - can be pushed by snake
- [x] Bombs - game over on contact (orange/black radial gradient)
- [x] Block stacking (gray blocks can cover bombs)
- [x] Game over state with reason display
- [x] Retry button to restart current level
- [x] Gravity system - automatic after each move
  - [x] Snake falls as rigid body
  - [x] Pushable blocks fall independently
  - [x] Bottom edge (row 19) game over condition for snake
  - [x] Animated falling with 200ms delays between gravity ticks
- [x] Custom level creator with visual editor
  - [x] Block type selector toolbar (6 block types)
  - [x] Click to place/remove blocks in grid
  - [x] Live preview as you build
  - [x] Interactive tooltips showing coordinates and block types
  - [x] Description field for level naming
  - [x] Validation with specific error messages
- [x] URL-based level sharing (query string format)
  - [x] Auto-play levels from shared URLs
  - [x] "Share" button with copyable URL
- [x] Local storage for custom levels
  - [x] Save custom levels with names
  - [x] Load saved levels from list
  - [x] Delete saved levels
- [x] "Customize" button on preset levels to edit them
- [x] Navigation menu with level links and custom level link
- [x] Shared LevelConfig model (deduplicated code)
- [x] Consistent block styling across all components

### Future Enhancements (Not Yet Implemented)
- [ ] Undo/redo in level editor
- [ ] High score tracking (minimal moves executed)
- [ ] Sound effects
- [ ] Leaderboard
- [ ] More advanced puzzle mechanics

## Technical Stack
- **Framework**: Blazor WebAssembly (.NET 8)
- **Styling**: CSS
- **Movement**: Event-driven (manual arrow press)

## File Structure
- `/Pages/Home.razor` - Landing page with game introduction
- `/Pages/SnakeGame.razor` - Level selection and preset level gameplay
- `/Pages/SnakeGame.razor.css` - Level selection and game styling
- `/Pages/CustomLevel.razor` - Custom level creator and player with visual editor
- `/Pages/CustomLevel.razor.css` - Custom level editor styling
- `/Components/GameGrid.razor` - Reusable game grid component for rendering level state
- `/Components/GameGrid.razor.css` - Game grid styling
- `/Components/InteractiveGameGrid.razor` - Interactive grid for level editor with click handling
- `/Components/InteractiveGameGrid.razor.css` - Interactive grid styling
- `/Services/SnakeGameEngine.cs` - Core game logic service (movement, collision, gravity, state management)
- `/Models/Position.cs` - Position record with helper methods (Up, Down, Left, Right)
- `/Models/LevelConfig.cs` - Shared level configuration model with parsing/serialization
- `/Layout/NavMenu.razor` - Navigation with Snake Game submenu
- `/wwwroot/css/app.css` - Global styles including shared block styles
- `/wwwroot/index.html` - Main HTML with keyboard handler JavaScript
- Navigation link in NavMenu with submenu for individual levels

## Version History
- **v2.0** (Current): Visual level editor with block selector toolbar, click-to-place/remove blocks, local storage for saving/loading custom levels, navigation menu with level links, shared LevelConfig model, consistent green apples and darker purple portal across all components, 12 built-in levels
- **v1.5**: Added "Customize" button to edit preset levels, direct URL navigation to levels, animated gravity with 200ms delays, retry button, comprehensive game state management via SnakeGameEngine service
- **v1.1**: Enhanced custom level validation - required fields (portal, snake, min 3 ground blocks), specific error messages per section, automatic filtering of empty position entries, bounds checking
- **v1.0**: Added custom level creator with form inputs, URL-based level sharing, and auto-play from shared URLs; Updated compact format to use URL query string style (& delimiter, = for key-value pairs)
- **v0.9**: Added compact level string format for easier level definition - sections prefixed with single letters (D=description, E=portal, S=snake, A=apples, G=ground, P=pushables, B=bombs), separated by ampersands, positions as row,col
- **v0.8**: Apples now act as solid blocks for collision detection and provide support (can hold up snake and blocks), but snake head can still eat them
- **v0.7**: Removed snake direction tracking - snake has no concept of "facing" direction, can move freely in any direction each turn
- **v0.6**: Added support for multiple apples per level, created Level 4 with 3 apples and elevated platforms
- **v0.5**: Redesigned 3 levels - Level 1: gap jump, Level 2: cliff climb, Level 3: bomb cover puzzle
- **v0.4**: Added gravity system - rigid snake body falls as unit, pushable blocks fall independently, bottom edge game over, redesigned 3 levels
- **v0.3**: Added ground blocks, pushable gray blocks, bombs, block stacking mechanics, game over state
- **v0.2**: Added apple collection, end portal, level system with 3 levels, level selection
- **v0.1**: Initial implementation - Grid, snake rendering, manual movement controls (one block per arrow press)

