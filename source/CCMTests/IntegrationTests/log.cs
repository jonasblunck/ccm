
namespace XXX.XXX.YYYY.ZZZ.Baml
{
  /// <summary>
  /// Logs messages to console
  /// Can be modified for logging to other places in the future
  /// </summary>
  internal abstract class Log
  {
    private const string strBackslash = @"\";
    private static readonly Regex pathRegEx =
      new Regex ("^(([a-zA-Z]\\:)|(\\\\))(\\\\{1}|((\\\\{1})[^\\\\]([^/:*?<>\"|]*))+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static void Write(string text)
    {
      Console.WriteLine(text);
    }
  }
}
