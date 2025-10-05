# Column Editor for osu!mania

A console application for swapping note columns in osu!mania beatmaps.

## Features

- Supports 2K to 9K mania
- Simple console interface with color-coded messages
- Creates new .osu files with modified column order
- Preserves original difficulty name while adding column order suffix
- Continuous operation - edit multiple files without restarting

## Usage

1. Run `ColumnEditor.exe`
2. Enter the path to your .osu file
3. Enter the new column order when prompted
   - For 4K: `2143` (swaps columns 1↔2 and 3↔4)
   - For 7K: `7654321` (reverses all columns)
4. A new .osu file will be created with the column order appended to the difficulty name
5. Continue editing or type `new` to select a different file

## Examples

**4K Example:**
Original columns: 1 2 3 4
Input order: 2 1 4 3
Result: Column 1→2, 2→1, 3→4, 4→3

**File naming:**
- Original: `My Difficulty.osu`
- After editing: `My Difficulty [2143].osu`

## Color Guide

- **Blue**: Information and prompts
- **Green**: Success messages
- **Red**: Error messages
- **Yellow**: Warnings and notes

## Commands

- `new` - Return to file selection
- `exit` - Close the application (when entering file path)

## Requirements

- Windows OS
- .NET Framework (included in standalone version)

## Building from Source

1. Open project in Visual Studio 2022
2. Build → Publish
3. Select "Self-Contained" and "Single File" options

## Notes

- Works with any .osu file that has proper CircleSize setting
- Preserves all original beatmap data except note positions and difficulty name
- Tested with osu!mania beatmaps from 2K to 9K