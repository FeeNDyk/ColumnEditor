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
        Console.WriteLine("=== Column Editor for osu!mania ===");
        Console.WriteLine("Drag and drop .osu file onto this window...");
        Console.WriteLine();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("File path: ");
            Console.ForegroundColor = ConsoleColor.White;
            string path = Console.ReadLine()?.Trim('"').Trim();

            if (string.IsNullOrWhiteSpace(path))
            {
                ShowError("Path cannot be empty.");
                continue;
            }

            if (path.ToLower() == "exit") break;

            if (!File.Exists(path))
            {
                ShowError("File not found.");
                continue;
            }

            if (Path.GetExtension(path).ToLower() != ".osu")
            {
                ShowError("Error: File must have .osu extension.");
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"File found: {Path.GetFileName(path)}");

            try
            {
                ProcessOsuFile(path);
            }
            catch (Exception ex)
            {
                ShowError($"Error processing file: {ex.Message}");
            }
        }

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Thank you for using Column Editor!");
        Console.ResetColor();
    }

    static void ProcessOsuFile(string path)
    {
        string[] lines = File.ReadAllLines(path);
        int columnCount = 0;

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
            ShowError("Error: Could not find CircleSize in file.");
            return;
        }

        if (columnCount < 3 || columnCount > 9)
        {
            ShowError($"Error: Unsupported column count {columnCount}K.");
            Console.WriteLine("This program only supports 3K to 9K.");
            return;
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
                ShowError("Order cannot be empty.");
                continue;
            }

            if (orderInput.ToLower() == "new") break;

            string order = orderInput.Replace("/", "").Replace(" ", "");

            if (order.Length != columnCount)
            {
                ShowError($"Order must have exactly {columnCount} digits.");
                Console.WriteLine($"Example: {GetExampleOrder(columnCount)}");
                continue;
            }

            if (!order.All(char.IsDigit))
            {
                ShowError("Order must contain only digits.");
                continue;
            }

            int[] mapping = order.Select(c => int.Parse(c.ToString())).ToArray();

            if (mapping.Distinct().Count() != columnCount ||
                mapping.Any(n => n < 1 || n > columnCount))
            {
                ShowError($"Order must be a valid permutation of digits 1–{columnCount}.");
                Console.WriteLine($"Example: {GetExampleOrder(columnCount)}");
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Applying column order: {order}");

            StringBuilder result = new StringBuilder();
            bool inHitObjects = false;
            double columnWidth = 512.0 / columnCount;

            foreach (string line in lines)
            {
                if (line.StartsWith("Version:"))
                {
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
                    inHitObjects = false;
                    result.AppendLine(line);
                }
                else if (inHitObjects && !string.IsNullOrWhiteSpace(line))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        if (int.TryParse(parts[0], out int x))
                        {
                            int currentColumn = Math.Min(columnCount - 1, (int)(x / columnWidth));
                            int newColumn = mapping[currentColumn] - 1;
                            int newX = (int)(newColumn * columnWidth + columnWidth / 2);
                            newX = Math.Max(0, Math.Min(511, newX));
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

            string directory = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string newPath = Path.Combine(directory, $"{filename} [{order}].osu");

            File.WriteAllText(newPath, result.ToString(), Encoding.UTF8);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Successfully created: {Path.GetFileName(newPath)}");
            Console.WriteLine();
            break;
        }
    }

    static string GetExampleOrder(int columnCount)
    {
        var digits = Enumerable.Range(1, columnCount).ToArray();
        if (columnCount >= 2)
        {
            int temp = digits[0];
            digits[0] = digits[1];
            digits[1] = temp;
        }
        return string.Join("", digits);
    }

    static void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
    }

    static void ShowSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
    }
}