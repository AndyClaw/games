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
- **Visual**: Red circular objects on the grid
- **Behavior**: 
  - **Collision**: Acts as a solid block - snake body, pushable blocks, and other objects cannot pass through it
  - **Support**: Provides support for the snake and pushable blocks (prevents falling)
  - **Eating**: When snake **head** moves onto apple, apple disappears and snake grows by 1 segment
  - Snake length increases by 1 segment per apple eaten
  - Each apple collected grows the snake independently
  - Apple positions are fixed per level

### 5. End Portal
- **Required**: Every level has an end portal
- **Visual**: Distinct from snake and apple (e.g., purple/gold square)
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

#### Bombs (Red with warning symbol)
- **Visual**: Red/orange with explosion or warning indicator
- **Behavior**: 
  - Instant game over if snake touches them
  - Can be covered by pushing a gray block onto them
  - Gray block on bomb is safe (bomb stays underneath)
  - If gray block is pushed off, bomb is revealed and dangerous again
  - Snake hitting an uncovered bomb ends the game

### 7. Level System
- **Level Selection**: Player can choose which level to play
- **Pre-built Levels**: 4 built-in levels with increasing difficulty
- **Custom Level Creator**: 
  - Accessible from level select screen
  - Form-based editor with text inputs for each element type
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
  - "Share" button generates a shareable URL (validates before sharing)
- **URL Sharing**:
  - Levels encoded as URL query parameters
  - Share URL with others to play custom levels
  - Auto-play when visiting a shared level URL
  - No server storage required - level data in URL
- **Level Configuration**: Each level has predefined:
  - Apple position (optional - some levels may have no apple)
  - End portal position (required)
  - Snake starting position
  - Ground blocks (brown, immovable)
  - Pushable blocks (gray, can be moved)
  - Bombs (dangerous obstacles)
- **Initial Levels**: 4 levels to start
  - Level 1: Jump a 2-block gap between platforms (tutorial)
  - Level 2: Climb a cliff by pushing a block against it to create a stepping stone
  - Level 3: Push a block over a bomb to safely cross and reach the portal
  - Level 4: Collect 3 apples placed at different heights, use blocks to reach elevated apples

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

### Phase 1 (Current)
- [x] Create game grid with visible border
- [x] Render snake with head (blue) and body segments (green)
- [x] Implement movement controls (keyboard and buttons)
- [x] Snake movement logic (segments follow each other)
- [x] Manual movement - one block per arrow press
- [x] Apple system - collect to grow snake
- [x] End portal - reach to win level
- [x] Level system - 4 playable levels with fixed layouts
- [x] Level selection UI
- [x] Level completion and progression
- [x] Ground blocks - immovable brown obstacles
- [x] Pushable gray blocks - can be pushed by snake
- [x] Bombs - game over on contact
- [x] Block stacking (gray blocks on bombs)
- [x] Game over state
- [x] Gravity system - snake and blocks fall
- [x] Rigid snake body mechanics
- [x] Bottom edge game over condition
- [x] Custom level creator with text input fields
- [x] URL-based level sharing (query string format)
- [x] Auto-play levels from shared URLs

### Future Phases (Not Yet Implemented)
- [ ] More levels
- [ ] Collision detection (walls, self)
- [ ] Level editor
- [ ] Score tracking

## Technical Stack
- **Framework**: Blazor WebAssembly (.NET 8)
- **Styling**: CSS
- **Movement**: Event-driven (manual arrow press)

## File Structure
- `/Pages/SnakeGame.razor` - Main game component with level selection
- `/Pages/SnakeGame.razor.css` - Game styling
- `/Pages/CustomLevel.razor` - Custom level creator and player
- `/Pages/CustomLevel.razor.css` - Custom level editor styling
- Navigation link in NavMenu

## Version History
- **v1.1**: Enhanced custom level validation - required fields (portal, snake, min 3 ground blocks), specific error messages per section, automatic filtering of empty position entries, bounds checking
- **v1.0**: Added custom level creator with form inputs, URL-based level sharing, and auto-play from shared URLs; Updated compact format to use URL query string style (& delimiter, = for key-value pairs)
- **v0.9**: Added compact level string format for easier level definition - sections prefixed with single letters (D=description, E=portal, S=snake, A=apples, G=ground, P=pushables, B=bombs), separated by periods, positions as row,col
- **v0.8**: Apples now act as solid blocks for collision detection and provide support (can hold up snake and blocks), but snake head can still eat them
- **v0.7**: Removed snake direction tracking - snake has no concept of "facing" direction, can move freely in any direction each turn
- **v0.6**: Added support for multiple apples per level, created Level 4 with 3 apples and elevated platforms
- **v0.5**: Redesigned 3 levels - Level 1: gap jump, Level 2: cliff climb, Level 3: bomb cover puzzle
- **v0.4**: Added gravity system - rigid snake body falls as unit, pushable blocks fall independently, bottom edge game over, redesigned 3 levels
- **v0.3**: Added ground blocks, pushable gray blocks, bombs, block stacking mechanics, game over state
- **v0.2**: Added apple collection, end portal, level system with 3 levels, level selection
- **v0.1**: Initial implementation - Grid, snake rendering, manual movement controls (one block per arrow press)

