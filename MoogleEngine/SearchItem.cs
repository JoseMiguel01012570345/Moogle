namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string snippet)
    {
        this.Title = title;
        this.Snippet = snippet;
    }
    public string Title { get; private set; }
    public string Snippet { get; private set; }

}
