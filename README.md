# Column Editor for osu!mania

A console application for swapping note columns in osu!mania beatmaps.

*This project was generated with AI*

## Download


## Features

- Supports 3K to 9K mania modes
- Creates new .osu files with modified column order
- Preserves original difficulty name while adding column order suffix
- Continuous operation - edit multiple files without restarting
- Drag & drop support - simply drop .osu files onto the console window

## Usage

1. Run `ColumnEditor.exe`
2. Enter the path to your .osu file (or drag & drop file onto the console)
3. Enter the new column order when prompted
   - For 4K: `2143` (swaps columns 1↔2 and 3↔4)
   - For 7K: `7654321` (reverses all columns)
4. A new .osu file will be created with the column order appended to the difficulty name
5. Continue editing or type `new` to select a different file