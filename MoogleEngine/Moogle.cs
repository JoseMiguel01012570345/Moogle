namespace MoogleEngine;
/*
    Once we get the query has string of characters we have to recomend documets. But there are two kind of documents we will have to return;
    if the query ask for close words we will have to return documents where are the closest words. If in query there is no closing operator
    we shall recomend documents where query is best accepted. We will follow the next steps:

    ->We will calculate those documents as if there weren't closing operators ,so to find those documents where exist at least one word of the query

    ->If exist those documents we will look for closing operator at line 31. If exist the closing operator we will return those documents, if not we will say
    "No resoult found or the words are too separated from each other 😕", at line 57 we will add these document ranked by the TF_IDF class

    ->if there is not resoult at all we will say "Did you mean something?"
*/

public static class Moogle
{
    public static SearchResult Query(string query)//the user query
    {
        TF_IDF1 search = new TF_IDF1(query);//instance object that will do the calculations and return the best documents.

        List<SearchItem> Object1 = new List<SearchItem>();

        BestDistance closeWords = new BestDistance("", new List<string>[0], new List<string>());

        string suggestion = "";

        if (search.ranking != null)//if there were resoult
        {
            closeWords = new BestDistance(query, search.GrankedDocs, search.ranking);//check for closing words and return the best documents

            if (closeWords.Existence == true)//if closing words exist
            {

                for (int i = 0; i < closeWords.RecomendSnippet.Item2.Count; i++)//we will put the snippet in string for the user
                {
                    string snippet = "";

                    if (closeWords.RecomendSnippet.Item2[i].Length == 0) continue;

                    for (int j = 0; j < closeWords.RecomendSnippet.Item2[i].Length; j++)

                        snippet += closeWords.RecomendSnippet.Item2[i][j] + " ";

                    Object1.Add(new SearchItem(Reading.FindName(closeWords.RecomendSnippet.Item1[i]), snippet));//and store resoult
                }

                if (Object1.Count == 0 && closeWords.NoDistance == false)//control the case there is no close words
                {
                    Object1 = new List<SearchItem>();

                    Object1.Add(new SearchItem("", "No resoult found or the words are too separated from each other 😕"));//the resoult does not exist
                }
                suggestion = closeWords.suggestion;
            }

            for (int i = 0; i < search.ranking.Count; i++)//we will add these words from ranking but were closed words doesn't take part in
            {
                if (closeWords.RecomendSnippet.Item1.Contains(search.ranking[i])) continue;

                Object1.Add(new SearchItem(Reading.FindName(search.ranking[i]), "🎯 " + search.snippet[i] + " 🎯"));
            }
            if (suggestion == "") suggestion = search.suggestion;

            return new SearchResult(Object1.ToArray(), suggestion);
        }

        //if we reach this line it means that nothing is found,we don't even have a suggestion to offer
        SearchItem[] Object = new SearchItem[1];

        Object[0] = new SearchItem("", "Did you mean something ❓  😅");

        return new SearchResult(Object, "");
    }
}