using System.Drawing;
using System.Drawing.Imaging;
using Tesseract;

namespace rapidclick;

#pragma warning disable CA1416

class Program
{
    static TesseractEngine engine = new(@"tessdata", "eng", EngineMode.Default);

    static void Main(string[] args)
    {
        var img = ScreenCapture.CaptureActiveWindow();

        using var page = engine.Process(img, PageSegMode.Auto);
        // get coodinates of text

        string lookingFor = string.Join(" ", args);

        const PageIteratorLevel detailLevel = PageIteratorLevel.Word;

        List<(string, Rect)> found = new();

        using ResultIterator? iterator = page.GetIterator();
        iterator.Begin();
        do
        {
            string foundText = iterator.GetText(detailLevel);
            Console.WriteLine(foundText);

            iterator.TryGetBoundingBox(detailLevel, out Rect rect);
            found.Add((foundText, rect));
        } while (iterator.Next(detailLevel));
        
        // get the best match
        Rect? bestMatch = PickBestMatch(found, lookingFor);
        
        if (bestMatch != null)
        {
            Console.WriteLine($"Best match found at {bestMatch.Value.X1}, {bestMatch.Value.Y1}, {bestMatch.Value.Width}, {bestMatch.Value.Height}");
            Rectangle windowRect = ScreenCapture.GetWindowRect(ScreenCapture.GetForegroundWindow());
            
            // Make a rectnalge of the center of the text
            Point center = new(bestMatch.Value.X1 + bestMatch.Value.Width / 2, bestMatch.Value.Y1 + bestMatch.Value.Height / 2);
            
            MouseOperations.SetCursorPosition(windowRect.Left + center.X, windowRect.Top + center.Y);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
            MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
        }
        else
            Console.WriteLine("No match found");
    }

    static T? PickBestMatch<T>(List<(string key, T value)> haystack, string needle)
    {
        // Find exact match
        (string key, T value)? exactMatch = haystack.FirstOrDefault(x => x.key == needle);

        if (exactMatch != null)
            return exactMatch.Value.value;

        // Find case-insensitive match
        (string key, T value)? caseInsensitiveMatch = haystack.FirstOrDefault(x => x.key.ToLower() == needle.ToLower());

        if (caseInsensitiveMatch != null)
            return caseInsensitiveMatch.Value.value;

        // Find partial match
        (string key, T value)? partialMatch = haystack.FirstOrDefault(x => x.key.Contains(needle));

        if (partialMatch != null)
            return partialMatch.Value.value;

        // Find case-insensitive partial match
        (string key, T value)? caseInsensitivePartialMatch =
            haystack.FirstOrDefault(x => x.key.ToLower().Contains(needle.ToLower()));

        if (caseInsensitivePartialMatch != null)
            return caseInsensitivePartialMatch.Value.value;

        return default;
    }
}