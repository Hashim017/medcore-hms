namespace MedCore.Helpers
{
    public static class StringHelpers
    {
        public static string ToTitleCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            var words = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", words.Select(w =>
                char.ToUpper(w[0]) + (w.Length > 1 ? w.Substring(1).ToLower() : "")));
        }
    }
}