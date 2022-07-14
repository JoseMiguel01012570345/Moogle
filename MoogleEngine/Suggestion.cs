namespace MoogleEngine;

/*
    This class is thought to make suggestions in case the user has mispelt his query
*/
public class Suggestion
{
    string[] query;
    public Suggestion(string query)
    {
        if (query != null)//if a query is null we can suggest nothing
        {
            this.query = TF_IDF1.QueryTokenization(query);

            suggestion();
        }
    }
    public string? suggestion2 { get; set; }
    public void suggestion()
    {
        string[] suggestion1 = new string[query.Length];

        double best = int.MaxValue;

        double aux = 0;

        string Real = "";

        for (int i = 0; i < query.Length; i++)//we will try to make the best suggestion for a specific word
        {
            if (query[i] == "~" || query[i] == "!" || query[i] == "*" || query[i] == "^") { suggestion1[i] = query[i]; continue; }//if a query is an operator there is nothing to do

            best = int.MaxValue;

            for (int j = 0; j < Reading.Index.Item2.Count; j++)
            {
                aux = LevenshteinDistance(Reading.Index.Item2[j].ToLower(), query[i]);

                if (aux < best && aux <= 5)//a word distanced by more than five unities is a very mispelt word so perhaps the user meant something else
                {
                    best = aux;
                    suggestion1[i] = Reading.Index.Item2[j];
                }
            }
        }
        for (int i = 0; i < suggestion1.Length; i++)//lets rebuilt the query for the user
        {
            if (i == suggestion1.Length - 1)
            {
                Real += suggestion1[i];
                break;
            }

            Real += suggestion1[i] + " ";
        }
        if (Real.Length == 0)//in case there were no suggestion we say nothing as ""
            Real = "";

        suggestion2 = Real;
    }
    public static int LevenshteinDistance(string s, string t)
    {
        // d is a table with m+1 rows and n+1 colums
        int cost = 0;

        int m = s.Length;

        int n = t.Length;

        int[,] d = new int[m + 1, n + 1];

        // Verify something to compare
        if (n == 0) return m;
        if (m == 0) return n;

        // Fill the first colum and the first row.
        for (int i = 0; i <= m; d[i, 0] = i++) ;
        for (int j = 0; j <= n; d[0, j] = j++) ;

        /// lets go throught the matrix filling each weight.
        /// i colum, j row
        for (int i = 1; i <= m; i++)
        {
            // pass throught j
            for (int j = 1; j <= n; j++)
            {
                /// if equals in similar posicions 0 weight
                /// otherwise weight plus one.
                if (s[i - 1] == t[j - 1]) cost = 0;
                else
                    cost = 1;

                d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1,  //deletion
                              d[i, j - 1] + 1),                             //insertion 
                              d[i - 1, j - 1] + cost);                     //substitution
            }
        }
        return d[m, n];
    }
}