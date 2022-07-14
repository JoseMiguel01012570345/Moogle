namespace MoogleEngine;
/*
    This class has got the propouse to fulfill with the closing operator:

   * The first thing here is to tokenize the query properly.
    
   * After that verify if closing operators exist , find the words to get close , get the closest snippet for every recomended document, if there is no snippet here
     we will try with the recomendation. That all we do it at line 47.

   About code:

    At line 47: we fistly have to verify if the words are in a document (the hole of them), if they are we will try to find the best distance in that document.
    In case the words aren't in we will try we a suggestion. This suggestion is the best Levenshtein distance in a document and that document is the one we will 
    analyze if there are closing words.In any case, we will order the the resoult,if there is resoult, judging by its lenght
    
    About NearestDistance():

    Every time a word of the list of words is found we will make a list that starts with this word, if that word isn't repeat we will skip the iteraction to the
    other word (in case we find this word again in the document) and store it in the list. We will be storing words of the documet in every list till we find 
    the whole of the close words in a particular list.
    
    If the word is repeated n times and the word is found for a second time, we will start another list and repeat the procedure above described, that till we be
    with the times the word were repeated.

    The last lines we'll be trying to find the least lenght of snippet in the list <ToEvaluate>.
*/
public class BestDistance
{
    List<string> query;

    List<string>[] gWords;

    List<List<bool>> repeat = new List<List<bool>>();

    public List<string> recomendation;
    public BestDistance(string query, List<string>[] gWords, List<string> recomendation)
    {
        this.query = Operator.NoExistenceDetector(TF_IDF1.QueryTokenization(query).ToList()).Item1;//get the query tokenized

        this.recomendation = recomendation;

        this.gWords = gWords;

        this.RecomendSnippet = new Tuple<List<string>, List<string[]>>(new List<string>(), new List<string[]>());//we will initialize this tupe for Query at Moogle.cs at line 61

        DistanceDetector();
    }
    public bool Existence { get; set; }
    public Tuple<List<string>, List<string[]>> RecomendSnippet { get; set; }
    public string suggestion { get; set; }
    public bool NoDistance { get; set; }
    void DistanceDetector()
    {
        for (int i = 0; i < query.Count; i++)
        {
            if (query[i] == "~")
            {
                FindDistance();
                Existence = true;//if was found any "~" we have to notify to Moogle.Query() that the could be close words and show the resoult
                break;
            }
        }
    }
    void FindDistance()
    {
        //in this method we will verify if the words of the query are close enough, if there is no resoult we will try a suggestion
        bool watcher;

        List<string[]> best = new List<string[]>();

        List<string> findDistance = FindWords();//search for the words to close

        string[] newQuery = new string[0];

        List<string> Title = new List<string>();

        RecomendSnippet = new Tuple<List<string>, List<string[]>>(Title, best);

        for (int i = 0; i < gWords.Length; i++)
        {
            //we will fistly verify if the hole of the close words are in a recomended document
            watcher = true;

            for (int j = 0; j < findDistance.Count; j++)

                if (Contains.Contain(findDistance[j], gWords[i]) == false)
                {
                    watcher = false;
                    break;
                }

            if (watcher == true && recomendation.Count != 0)
            {
                if (findDistance.Count == 1)//distance to A to A is 0;
                {
                    NoDistance = true;
                    break;
                }
                //if exist the words in the document we will verify if they are close, if so, to carry the snippet that NearestDistance() returns and carry the document
                best.Add(NearestDistance(gWords[i], findDistance.Count, findDistance));

                if (best[i].Length != 0) Title.Add(recomendation[i]);
            }

        }
        if (best.Count != 0)//if there is are closing words lets order it by the lenght of the snippet
        {
            OrdenationByLenght(best, Title);
        }
        if (best.Count == 0 && recomendation.Count != 0)//if there is no resoult for the query lets verify if the query is mispelt
        {
            Tuple<List<string>, string[], int> T = MakeMatch(findDistance);

            suggestion = BuildSuggestion(T.Item2);//add format to the suggestion was found

            best?.Add(NearestDistance(T.Item1, T.Item2.Length, T.Item2.ToList()));//and try again with the suggestion

            if (best[0].Length != 0) Title.Add(recomendation[T.Item3]);//add the name of the document

        }
    }
    static string BuildSuggestion(string[] item2)//add format to the suggestions
    {
        string suggest = "";

        for (int i = 0; i < item2.Length; i++)
        {
            if (i == item2.Length - 1)
            {
                suggest += item2[i];
                break;
            }

            suggest += item2[i] + " ";
        }
        return suggest;
    }
    Tuple<List<string>, string[], int> MakeMatch(List<string> findDistance)
    {
        //we will look for the most similar words in a single documents and then verify if these words are close in that document
        int bestLevenshtein = int.MaxValue;

        string[] newQuery = new string[findDistance.Count];

        string[] bestQuery = new string[findDistance.Count];

        int distance;

        int bestSum = int.MaxValue - 100000;

        int auxSum = int.MaxValue;

        int index = 0;

        List<string> doc = new List<string>();

        for (int i = 0; i < gWords.Length; i++)//after this cicle we will know what is the best suggestion in the recomendation returned by TF_IDF1 class
        {
            for (int h = 0; h < findDistance.Count; h++)//after this cicle we will know what is the best suggestion in this document
            {
                bestLevenshtein = int.MaxValue;

                for (int j = 0; j < gWords[i].Count; j++)
                {
                    distance = Suggestion.LevenshteinDistance(gWords[i][j].ToLower(), findDistance[h]);

                    if (bestLevenshtein > distance)//if there is a better Levenshtein distance then it is our best
                    {
                        bestLevenshtein = distance;

                        newQuery[h] = gWords[i][j];
                    }
                }

                auxSum += bestLevenshtein;

                if (auxSum >= bestSum) break;//if the sum up to the h-word is bigger or equal than the bestSum we don't use
            }


            if (auxSum < bestSum)//if there is a better sum we should take it into account and update bestSum , newQuery and get the
            {
                bestSum = auxSum;

                newQuery.CopyTo(bestQuery, 0);

                if (i > 0)//restarting the counter of the iteraction
                {
                    doc = gWords[i - 1];
                    index = i - 1;
                }
                if (i == 0)
                {
                    doc = gWords[i];
                    index = i;
                }
            }
            auxSum = 0;
        }

        Tuple<List<string>, string[], int> T = new Tuple<List<string>, string[], int>(doc, bestQuery, index);

        return T;
    }
    void OrdenationByLenght(List<string[]> best, List<string> Title)
    {
        //we will order by the snippet by lenght
        string[] aux = new string[0];
        string aux1;

        for (int i = 0; i < best.Count; i++)

            for (int j = i + 1; j < best.Count; j++)
            {

                if (best[i].Length > best[j].Length)//if there were a better lenght for the snippet of closing words we put it up
                {
                    aux = best[i];
                    aux1 = Title[i];//this change will be reflected in the ranking

                    best[i] = best[j];
                    Title[i] = Title[j];

                    best[j] = aux;
                    Title[j] = aux1;
                }
            }
    }
    List<string> FindWords()
    {
        List<string> findDistance = new List<string>();

        for (int i = 0; i < query.Count; i++)
        {
            if ((i - 1) >= 0 && (i + 1) < query.Count && query[i] == "~")//we have to make sure that "~" is between words
            {
                if (findDistance.Contains(query[i - 1]) == false) findDistance.Add(query[i - 1]); ;//we add the word before "~"

                if (findDistance.Contains(query[i + 1]) == false) findDistance.Add(query[i + 1]);//we add the word after "~"
            }
        }
        return findDistance;
    }
    string[] NearestDistance(List<string> documet, int count, List<string> query)
    {
        //varibles
        ///////////////////////////////////////////////////
        List<List<string>> nearest = new List<List<string>>();

        List<string> nearest1 = new List<string>();

        List<int> nearestInt = new List<int>();

        List<bool> right = new List<bool>();

        List<List<string>> ToEvaluate = new List<List<string>>();

        List<int> zeros = new List<int>();

        string[] aux = new string[0];

        string[] snippet = new string[0];

        bool v = true;

        for (int i = 0; i < documet.Count; i++)
        {
            v = true;

            for (int j = 0; j < query.Count; j++)
            {
                if (documet[i].ToLower() == query[j].ToLower())//if we find in txt a word of the close words...
                {
                    nearest1 = new List<string>();//we'll restart this list every time

                    for (int y = 0; y < nearest.Count; y++)
                    {

                        if (nearest[y].Contains(query[j].ToLower())) continue;//if the list contains the word and this term is repeated enought times 
                                                                              // we continue
                        else
                        {
                            //  repeat[j][repeat[j].IndexOf(false)] = true;//we convert the first apparence of a false as true

                            nearest[y].Add(query[j].ToLower());//add the new word to the list in lower case

                            nearestInt[y]--;//decrese the the number of terms the list have to find

                            right[y] = false;//this is a key not to enter in a cicle that will add the terms two times

                            if (nearestInt[y] == 0)//if there are all the terms we should put the list into those as value list
                            {
                                aux = new string[nearest[y].Count];

                                //we have to pass the values by values because if we do it by reference when removing the list this list will be deleted the same were
                                //reference exist
                                for (int k = 0; k < aux.Length; k++)
                                    aux[k] = nearest[y][k];

                                ToEvaluate.Add(aux.ToList());//add to the value list

                                nearest.Remove(nearest[y]);//remove from the list that carries posible value lists

                                nearestInt.Remove(nearestInt[y]);//we remove the zero from list of values

                                right.Remove(right[y]);//and remove this key

                                //as we remove a element we have to step back
                                if (y == 0)

                                    y = 0;

                                else
                                    y -= 1;
                            }
                        }
                    }
                    //if the key is false in a particular list we don't have to add the new element
                    for (int q = 0; q < nearest.Count; q++)

                        if (right[q] == true)

                            nearest[q].Add(query[j]);

                        else right[q] = true;//if the key is mark as false put it true

                    //when we find a word of the list of words to find closing we shall add them to verify if the list that resoult with this new star is better 
                    //in lenght than the other, so we add some label

                    right.Add(true);

                    nearest.Add(nearest1);//because <nearest> is list of list

                    nearest[nearest.Count - 1].Add(query[j].ToLower());

                    nearestInt.Add(count - 1);

                    v = false;
                }
            }
            // we the word in the i position of <dcoument> is not a of the list of words we will add it to the hole of list
            if (v == true)
                for (int h = 0; h < nearest.Count; h++)
                    nearest[h].Add(documet[i].ToLower());

            for (int l = 0; l < nearest.Count; l++)//if the list comes bigger then the number of the words multiply by ten between them those words aren't close
                if (nearest[l].Count >= query.Count * 10)//so we remove this list and its label
                {
                    nearest.Remove(nearest[l]);

                    nearestInt.Remove(nearestInt[l]);

                    right.Remove(right[l]);
                }
        }
        //at this line we will look for the list that has got the less number of words
        int min = int.MaxValue;

        for (int i = 0; i < ToEvaluate.Count; i++)

            if (ToEvaluate[i].Count < min)

                min = ToEvaluate[i].Count;

        if (ToEvaluate.Count != 0)

            for (int i = 0; i < ToEvaluate.Count; i++)
            {
                if (ToEvaluate[i].Count == min)
                {
                    snippet = new string[ToEvaluate[i].Count];

                    for (int j = 0; j < ToEvaluate[i].Count; j++)

                        snippet[j] = ToEvaluate[i][j];

                    break;
                }
            }
        return snippet;
    }
}