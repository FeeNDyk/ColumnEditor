using System;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    static void Main()
    {
        Console.Title = "luvlama's Column Editor";
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("=== Column Editor for osu!mania by luvlama ===");
        Console.WriteLine("Supported modes: 2K to 9K");
        Console.WriteLine();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter path to .osu file: ");
            Console.ForegroundColor = ConsoleColor.White;
            string path = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Path cannot be empty.");
                continue;
            }

            if (path.ToLower() == "exit") break;

            if (!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("File not found. Try again.");
                continue;
            }

            if (Path.GetExtension(path).ToLower() != ".osu")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: File must have .osu extension.");
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"File found: {Path.GetFileName(path)}");

            string[] lines = File.ReadAllLines(path);
            int columnCount = 0;

            // Find CircleSize
            foreach (string line in lines)
            {
                if (line.StartsWith("CircleSize:"))
                {
                    string value = line.Split(':')[1].Trim();
                    if (int.TryParse(value, out int cs))
                    {
                        columnCount = cs;
                    }
                    break;
                }
            }

            if (columnCount == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Could not find CircleSize in file.");
                continue;
            }

            // Check if supported (2K-9K)
            if (columnCount < 2 || columnCount > 9)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Unsupported column count {columnCount}K.");
                Console.WriteLine("This program only supports 2K to 9K.");
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Detected column count: {columnCount}K");

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write($"Enter new column order (e.g., for {columnCount}K = {GetExampleOrder(columnCount)}) or 'new' for different file: ");
                Console.ForegroundColor = ConsoleColor.White;
                string orderInput = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(orderInput))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Order cannot be empty.");
                    continue;
                }

                if (orderInput.ToLower() == "new") break;

                // Remove any spaces or slashes
                string order = orderInput.Replace("/", "").Replace(" ", "");

                // Validate order length
                if (order.Length != columnCount)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Order must have exactly {columnCount} digits.");
                    Console.WriteLine($"Example: {GetExampleOrder(columnCount)}");
                    continue;
                }

                if (!order.All(char.IsDigit))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Order must contain only digits.");
                    continue;
                }

                int[] mapping = order.Select(c => int.Parse(c.ToString())).ToArray();

                // Validate permutation
                if (mapping.Distinct().Count() != columnCount ||
                    mapping.Any(n => n < 1 || n > columnCount))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Order must be a valid permutation of digits 1–{columnCount}.");
                    Console.WriteLine($"Example: {GetExampleOrder(columnCount)}");
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"Applying column order: {order}");

                try
                {
                    // Process the file
                    string result = ProcessOsuFile(lines, columnCount, mapping, order);

                    // Create new filename
                    string directory = Path.GetDirectoryName(path);
                    string filename = Path.GetFileNameWithoutExtension(path);
                    string newPath = Path.Combine(directory, $"{filename} [{order}].osu");

                    File.WriteAllText(newPath, result, Encoding.UTF8);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Successfully created: {Path.GetFileName(newPath)}");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error processing file: {ex.Message}");
                }
            }
        }

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Thank you for using Column Editor!");
        Console.ResetColor();
    }

    static string GetExampleOrder(int columnCount)
    {
        // Generate example order like "2143" for 4K
        var digits = Enumerable.Range(1, columnCount).ToArray();
        // Swap first two digits for a simple example
        if (columnCount >= 2)
        {
            (digits[0], digits[1]) = (digits[1], digits[0]);
        }
        return string.Join("", digits);
    }

    static string ProcessOsuFile(string[] lines, int columnCount, int[] mapping, string order)
    {
        StringBuilder result = new StringBuilder();
        bool inHitObjects = false;
        double columnWidth = 512.0 / columnCount;

        foreach (string line in lines)
        {
            if (line.StartsWith("Version:"))
            {
                // Update difficulty name by adding column order at the end
                string originalVersion = line.Substring("Version:".Length).Trim();
                string newVersion = $"{originalVersion} [{order}]";
                result.AppendLine($"Version:{newVersion}");
            }
            else if (line.Trim() == "[HitObjects]")
            {
                inHitObjects = true;
                result.AppendLine(line);
            }
            else if (inHitObjects && line.StartsWith("["))
            {
                // New section after HitObjects
                inHitObjects = false;
                result.AppendLine(line);
            }
            else if (inHitObjects && !string.IsNullOrWhiteSpace(line))
            {
                // Process hit objects
                string[] parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    if (int.TryParse(parts[0], out int x))
                    {
                        // Calculate current column (0-based)
                        int currentColumn = Math.Min(columnCount - 1, (int)(x / columnWidth));

                        // Convert to 1-based and apply mapping
                        int newColumn = mapping[currentColumn] - 1;

                        // Calculate new X position (center of column)
                        int newX = (int)(newColumn * columnWidth + columnWidth / 2);
                        newX = Math.Max(0, Math.Min(511, newX)); // Clamp to 0-511

                        parts[0] = newX.ToString();
                    }
                }
                result.AppendLine(string.Join(",", parts));
            }
            else
            {
                result.AppendLine(line);
            }
        }

        return result.ToString();
    }
}