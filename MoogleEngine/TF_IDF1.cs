namespace MoogleEngine;
/* 
    Brief description to TF_IDF model:

    The TF_IDF class is the MoogleEngine namespace core because in here the resoult of a search (with no closing words) is made. Its about a mathematician 
    vectorial model that compouse by two fundamental parts, TF or term frecuency and IDF inverse document frecuency.
    
    => TF:its all about what frecuency has got a word in a j-document.The more frecuncy the bigger score.
    
    =>IDF:in how many document a term is repeated.The more the less score.

    Words like regular one does't make any sense to consider so them IDF is zero. If a word is a lot of times repeated in document but exist in a few document
    that score comes higher.

    About the algorithm:
    * This class is an instance class due to is a easy way to restart values query by query, so to its constructor is pass a parameter a string <query> that will be
    tokenized to separate it into words and operators
    
    * A trigger is press in order to initilize the process of calculations. There were no resolut the next step is to verify if the query is mispelt and show resolut
    under these non mispelt query under a label that ask if that query is the one you mean(Dis you mean __ ?).

    * On the trigger method is verified at least a query word exist to start calculations,if so, we'll check there were exclution,if so the exclution is made and
    stored in a variable to take into account so not to return snippet that start with the excluded one

    * If there are synonimus for these query words there need to be add to the calculations

    * After all above starts the calculations of the weight of the query on the database at <QueryInDatabase()>

    * if there are operators of existence or power then are indentify at the Operator class and instanced after calculating the weight of the query on the database.
    A method OperatorDetector() from Operator is called to power a query depending on what operator is found.This method modify directly the weight of specifics
    words of the query

    * To determine what documents are good for a query we call TF_IDF() and call Ranking() to rank the documents

    * To return a simple piece of essence of what the ranked document is about we will extract the list of words of the document(GetRankedDocs()) to use it for the snippet method 
    (Snippet()).

*/
public class TF_IDF1
{
    string[] query;
    string[] plus;//this variables controles all of the synonimus of the query incluring of curse the query
    public TF_IDF1(string query)//constructor that will inisilize the calculation to give the best document for the search
    {
        this.query = QueryTokenization(query);//tokenization of the query

        Trigger();//it is a funtion to inisialize the process of the calculation

        if (query.Length != 0 && query != null && query != "" && query != " ")

            if (ranking == null || ranking.Count == 0)//if there is no document to offer it is better to see if the query is misspelt
            {
                Suggestion misspelt = new Suggestion(query);//for that it is inisialized an object for the Suggestion class

                string[] aux = misspelt.suggestion2.Split(" ");

                suggestion = misspelt.suggestion2;

                this.query = aux;

                Trigger();//and we trigger again
            }
    }
    public string suggestion { get; set; }
    public string[]? snippet { get; set; }
    public static Dictionary<string, List<string>>? Gwords1 { get; set; }
    public static double[,]? WDB { get; set; }
    public Dictionary<string, double>? Tf_Idf { get; set; }
    public List<string>? ranking { get; set; }
    public double[,]? queryInDatabase { get; set; }
    public static List<string>? Documents { get; set; }
    public List<string>[]? GrankedDocs { get; set; }
    public List<string>? nonWanted { get; set; }
    void Trigger()
    {
        for (int i = 0; i < query.Length; i++)

            if (Reading.Index.Item1.ContainsKey(query[i]))//first lets ask if in our database exist at least one word to do the calculations
            {
                Tuple<List<string>, List<string>> T = Operator.NoExistenceDetector(query.ToList());//but first lets check if there is a no existence operator so to remove these words with the operator from the calculation

                query = T.Item1.ToArray();

                nonWanted = T.Item2;

                AddSynonimus();//let look for all of the synonimus that a word query has got

                QueryInDatabase();//and calculate the weight of the query into the query

                Operator operate = new Operator(plus.ToList(), queryInDatabase);//after this all done check if exist any other operator

                operate.OperatorDetector();

                TF_IDF();//calculate the TF_IDF of the current query

                Ranking();//lets make the recomendation of the search

                GetRankedDocs();//getting the the words without tokenization form the txt document

                Snippet();//and offer a snippet of the document which first word is any word of the query

                break;
            }
    }
    void AddSynonimus()
    {
        List<string> newQuery = new List<string>();

        for (int i = 0; i < query.Length; i++)//we will interact words by word in the query
        {
            newQuery.Add(query[i]);//and add it each words to the new query that will contains its synonimus

            for (int j = 0; j < Synonimus.SynonimuS.Length; j++)//lets watch if a word query has a synonimus in our database
            {
                if (Contains.Contain(query[i], Synonimus.SynonimuS[j]))//if it is found let add each word
                {
                    for (int x = 0; x < Synonimus.SynonimuS[j].Count; x++)
                    {
                        if (Synonimus.SynonimuS[j][x] == query[i]) continue;

                        newQuery.Add(Synonimus.SynonimuS[j][x]);
                    }
                }
            }
        }
        plus = newQuery.ToArray();
    }
    public static string[] QueryTokenization(string query)
    {
        List<string> query1 = new List<string>();

        string[] words = new string[0];

        char[] abcd = new char[0];

        words = query.Split(" ");

        for (int i = 0; i < words.Length; i++)
        {
            string speech = "";

            for (int j = 0; j < words[i].Length; j++)
            {
                if (words[i][j] == '!' || words[i][j] == '^' || words[i][j] == '*' || words[i][j] == '~')//iteraction char by char checking if exist any operator
                {
                    if (speech != "")//if so and speech is has got anything lets put it in a list and clean
                        query1.Add(speech.ToLower());

                    query1.Add(words[i][j].ToString());//lets add operators has a single word

                    speech = "";

                    continue;
                }
                if (char.IsLetterOrDigit(words[i][j]) || words[i][j] == '-')//finally if it is not about an operator we put it in the list
                    speech += words[i][j];
            }
            if (speech != "")//here is its controlled that case the speech variable were'nt empty
                query1.Add(speech.ToLower());
        }
        return query1.ToArray();
    }
    public static void VerifyPossibleNull()//if a document is null we dont need it
    {
        string[] document = Reading.ReadDrirectory(Directory.GetCurrentDirectory() + "/Content");

        List<string> NoNull = new List<string>();

        for (int i = 0; i < document.Length; i++)
        {
            if (document[i] == null) continue;

            NoNull.Add(document[i]);
        }
        if (NoNull == null)
            NoNull.Add("");

        Documents = NoNull;//lets put it in a property to initialize it when server is charging
    }
    public void Ranking()//gives and a list with a recomendation for the query
    {
        List<string> documents = new List<string>();

        Dictionary<string, double> suggetion = Tf_Idf;

        double[] array = new double[suggetion.Count];

        int count = 0;

        foreach (var i in suggetion)//lets get the value of the calculated tf_idf in order to sort
        {
            array[count] = i.Value;

            count++;
        }

        Array.Sort(array);

        Array.Reverse(array);//lets order by ascension

        List<int> auxiliar = new List<int>();

        double[] auxiliar1 = new double[array.Length];

        count = 0;

        foreach (var i in suggetion)//lets get the non-ordened tf_idf calculation
        {
            auxiliar1[count] = i.Value;

            count++;
        }

        for (int i = 0; i < array.Length; i++)
        {
            for (int index = 0; index < auxiliar1.Length; index++)
            {
                if (auxiliar1[index] == 0) continue;//exclution of thense documents no worth for the query

                if (array[i] == auxiliar1[index]) auxiliar.Add(index);//get the index of the best documents
            }
        }

        for (int i = 0; i < auxiliar.Count; i++)//there could be documents repeated so we delete them

            for (int j = i + 1; j < auxiliar.Count; j++)
            {
                if (auxiliar[i] == auxiliar[j])
                {
                    auxiliar.Remove(auxiliar[j]); j -= 1;
                }
            }

        for (int i = 0; i < auxiliar.Count; i++)//and finaly add the document that correspond to the index that correspond with the biggest TF_IDF calculation

            documents.Add(Documents[auxiliar[i]]);

        ranking = documents;
    }
    static double Max(Dictionary<string, int> diccionary)//maximal value in a dictionary
    {
        double count = 0;

        foreach (var term in diccionary)
        {
            if (count < diccionary[term.Key])

                count = diccionary[term.Key];
        }
        return count;
    }
    void TF_IDF()//TF_IDF resoult
    {
        List<string> document = Documents;

        Tuple<Dictionary<string, int>, List<string>, List<int>, Dictionary<string, int>[]> Index = Reading.Index;

        double[] queryWeight = QueryWeight(plus, document);

        double queryNorma = QueryNorma(queryWeight);

        double[] normaDocuments = NormaPerDocument(WDB);

        double[,] queryINDatabase = queryInDatabase;

        double[] numerator = new double[document.Count];

        Dictionary<string, double> resoult = new Dictionary<string, double>();

        for (int j = 0; j < document.Count; j++)//here it is calculated the numerator of the formula of the cos similarity

            for (int i = 0; i < plus.Length; i++)

                numerator[j] += queryINDatabase[j, i] * queryWeight[i];


        for (int j = 0; j < document.Count; j++)//and here is calculated and add as a resoult variable
        {
            resoult.Add(document[j], (numerator[j] / (normaDocuments[j] * queryNorma)));
        }
        Tf_Idf = resoult;
    }
    void QueryInDatabase()//the weight of the query in database
    {
        Tuple<Dictionary<string, int>, List<string>, List<int>, Dictionary<string, int>[]> Index = Reading.Index;

        double[,] weight_ij = new double[Documents.Count, plus.Length];//matrix that is the weight of the query by document

        for (int i = 0; i < Documents.Count; i++)
        {
            double maximo = Max(Index.Item4[i]);

            int index = 0;

            for (int j = 0; j < plus.Length; j++)
            {
                if (Index.Item4[i].ContainsKey(plus[j]))//if a word is not contained its weight is just 0
                {
                    index = Index.Item1[plus[j]];//it is need the index of the j-word in database

                    if (Contains.Contain(plus[j], query.ToList()))//if the word is equal to any of the query array, its weight is calculated as usual
                    {
                        weight_ij[i, j] = (Index.Item4[i][plus[j]] * Math.Log10((double)Documents.Count / (double)Index.Item3.ElementAt(index)) / maximo);

                        if (weight_ij[i, j] != 0) weight_ij[i, j] *= 100;//if a word query is the document this document is better so lets put it rank it higher
                    }

                    else  //otherwise its is divide by a big number in order to guarrantee a document that has'n got a query word were higher recomended than a document that has a query word
                        weight_ij[i, j] = (Index.Item4[i][plus[j]] * Math.Log10((double)Documents.Count / (double)Index.Item3.ElementAt(index)) / maximo) / 10000;
                }
            }
        }
        queryInDatabase = weight_ij;//and return it to a property
    }
    double QueryNorma(double[] WeightQuery)
    {
        double queryNorma = 0;

        for (int i = 0; i < WeightQuery.Length; i++)

            queryNorma += Math.Pow(WeightQuery[i], 2);


        queryNorma = Math.Sqrt(queryNorma);

        if (queryNorma == 0) queryNorma = 1;

        return queryNorma;
    }
    double[] QueryWeight(string[] plus, List<string> document)//lets calculate the weight of the query on the query as a document
    {
        //the procedure here is almost the same that the one to calculate the weight of a word on a database, but the diference here is that only exist a documents,
        //the query

        Tuple<Dictionary<string, int>, List<string>, List<int>, Dictionary<string, int>[]> Index = Reading.Index;

        double[] weightQuery = new double[plus.Length];

        Dictionary<string, int> Diccionario = Query(plus);//diccionary of the query containing the string and its frecuency in query

        double maximo = Max(Diccionario);

        int index = 0;

        for (int j = 0; j < plus.Length; j++)
        {
            if (Index.Item1.ContainsKey(plus[j]))//need to be watch a query word exist in database

                index = Index.Item1[plus[j]];

            else continue;

            //if so lets continue to the next step

            weightQuery[j] = (Diccionario[plus[j]] * Math.Log10((double)document.Count / (double)Index.Item3.ElementAt(index))) / maximo;
        }
        return weightQuery;
    }
    public static void WeightDatabase()//norma vector of a document that contain the weight of a word on a the database
    {
        Tuple<Dictionary<string, int>, List<string>, List<int>, Dictionary<string, int>[]> Index = Reading.Index;

        double[,] weight_ij = new double[Documents.Count, Index.Item1.Count];

        for (int i = 0; i < Documents.Count; i++)
        {
            double maximo = Max(Index.Item4[i]);

            for (int j = 0; j < Index.Item1.Count; j++)
            {
                if (Index.Item4[i].ContainsKey(Index.Item2.ElementAt(j)) == false) continue;//if a particular word that is contained in database but not in a document,
                                                                                            //it weight is just zero
                /* ...otherwise... */
                weight_ij[i, j] = (Index.Item4[i][Index.Item2.ElementAt(j)] * Math.Log10((double)Documents.Count / (double)Index.Item3.ElementAt(j))) / maximo;
            }
        }
        WDB = weight_ij;
    }
    Dictionary<string, int> Query(string[] plus)//getting the diccionary of the query
    {
        //this dictionary will keep the frecuency of the word into the query

        Dictionary<string, int> dicionario = new Dictionary<string, int>();

        bool[] auxiliar = new bool[plus.Length];//this varibale will help

        for (int i = 0; i < plus.Length; i++)

            if (auxiliar[i] == false)
            {
                dicionario.Add(plus[i], 1);//lets start making a label one frecuence found at the moment 

                for (int j = i + 1; j < plus.Length; j++)//and verify if there were a word in query like him
                    if (plus[i] == plus[j])
                    {
                        dicionario[plus[i]]++;//if so let give another frecuence

                        auxiliar[j] = true;//and mark that ocurrence so that not to repeat
                    }
            }
        return dicionario;
    }
    static double[] NormaPerDocument(double[,] WeightDatabase)
    {
        //the norma by document is a sum of the weight of its elements powered by the two root
        double[] norma = new double[WeightDatabase.GetLength(0)];

        for (int i = 0; i < WeightDatabase.GetLength(0); i++)
        {
            for (int j = 0; j < WeightDatabase.GetLength(1); j++)
            {
                norma[i] += Math.Pow(WeightDatabase[i, j], 2);
            }
            norma[i] = Math.Sqrt(norma[i]);
        }
        return norma;
    }
    public static void DocCopy()
    {
        //there is a copy here for each document for a snippet in case a recomendation exist
        Dictionary<string, List<string>> NoTokenWords = new Dictionary<string, List<string>>();

        List<string> documents = new List<string>();

        string[] aux1 = new string[0];

        char[] abcd = new char[0];

        string[] aux = new string[0];

        //the next procedure will try to separate words from any other character 
        for (int i = 0; i < Documents.Count; i++)
        {
            documents = new List<string>();

            aux = File.ReadAllLines(Documents[i]);//reading of the hole of the lines in a document

            for (int p = 0; p < aux.Length; p++)
            {
                aux1 = aux[p].Split(" ");//lets delete withespaces 

                for (int j = 0; j < aux1.Length; j++)
                {
                    string s1 = "";

                    abcd = aux1[j].ToCharArray();

                    for (int z = 0; z < abcd.Length; z++)
                    {
                        if ((char.IsLetterOrDigit(abcd[z]) == false) && abcd[z] != '-')
                        {
                            documents.Add(abcd[z].ToString());//put the char as a string in the document if is not a word
                            continue;
                        }
                        s1 += abcd[z];//otherwise add it to the the string to make a word
                    }
                    s1.ToLower();//and make it lower-case

                    if (s1 != "")
                        documents.Add(s1);
                }
            }
            NoTokenWords.Add(Documents[i], documents);//the document name and its corresponding list of words
        }
        Gwords1 = NoTokenWords;
    }
    void GetRankedDocs()
    {
        //this method is intented to capture these list of words of the document that are recomendation for the query 
        //in order to find a snippet to give a brief idea about what is it about

        List<string>[] words = new List<string>[ranking.Count];

        for (int i = 0; i < words.Length; i++)//initializing <words>
        {
            words[i] = new List<string>();
        }

        bool ToDo = true;

        int e = 0;
        foreach (var i in Gwords1)
        {
            for (int j = 0; j < ranking.Count; j++)
            {
                ToDo = true;

                if (ranking[j] == i.Key)//when the name of a ranked document is found lets add its list to <words>
                {
                    if (nonWanted.Count != 0)//if there is any document that needs to be remove from recomendation
                    {
                        for (int x = 0; x < nonWanted.Count; x++)
                        {
                            if (Contains.Contain(nonWanted[x], i.Value))//if any document contains any of the words that are'nt wanted,we removed from ranking
                            {
                                ranking.RemoveAt(j);
                                ToDo = false;
                                j = 0;
                                break;
                            }
                        }
                    }

                    if (ToDo == true) words[j] = i.Value;//if the document does't contains non-wanted words its recomended
                }
            }
            e++;
        }
        GrankedDocs = words;//put it in a property
    }
    public void Snippet()//snippet returned as a brief indication about what a document is about
    {
        List<string> querySnippet = new List<string>();

        for (int i = 0; i < plus.Length; i++)//it's better to delete operators

            if (plus[i] != "^" && plus[i] != "*" && plus[i] != "~")

                querySnippet.Add(plus[i]);


        List<string> docs = ranking;

        string[] snippet1 = new string[ranking.Count];

        string[] aux = new string[0];

        ////////////////
        bool val = false;
        //this boolean variable will help to break cicles when a snippet is found
        bool val0 = false;
        ////////////////
        for (int i = 0; i < snippet1.Length; i++)
        {
            val = false;

            val0 = false;

            for (int j = 0; j < querySnippet.Count; j++)
            {
                for (int v = 0; v < GrankedDocs[i].Count; v++)
                {
                    if (GrankedDocs[i][v].ToLower() == querySnippet[j].ToLower())
                    {
                        //check if the position of the word in <querySnippet> is in range
                        if (v + 30 > GrankedDocs[i].Count)

                            for (int h = v; h < GrankedDocs[i].Count; h++)

                                snippet1[i] += GrankedDocs[i][h] + " ";
                        else

                            for (int h = v; h < v + 30; h++)

                                snippet1[i] += GrankedDocs[i][h] + " ";

                        val = true;

                        break;//there is only need only one snippet per document
                    }
                    if (val == true) { val0 = true; break; }
                }
                if (val0 == true) break;
            }
        }
        snippet = snippet1;//put it in a property
    }
}