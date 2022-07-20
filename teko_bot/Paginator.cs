namespace teko_bot;

using System.Text;

public static class Paginator
{
    // Получить из некоторого List адекватное сообщение со списком
    private static string GetTextFromList<T>(List<T> list, string sep = "\n")
    {
        if (list.Count == 0)
            return Answers.EmptyList;
        var result = new StringBuilder();
        for (int i = 0; i < list.Count - 1; ++i)
        {
            result.Append(list[i]);
            result.Append(sep);
        }
        
        result.Append(list[^1]);
        return result.ToString();
    }

    // Добавить в конец строку с текущнй страницей и сколько страниц всего
    private static string GetPageInfo(int totalAmount, int page)
    {
        return "страница " + page.ToString() + " из " +
               Math.Ceiling((double)totalAmount / BotConfiguration.PageSize);
    }

    // генерация полноценной страницы для просмотра данных с БД 
    public static string GetPageFromList<T>(List<T> list, int totalAmount, int page, string sep = "\n")
    {
        var result = GetTextFromList(list, sep);
        result += "\n";
        result += GetPageInfo(totalAmount, page);
        return result;
    }
}