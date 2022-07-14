namespace MoogleEngine;
/*
    In order to improve search there are operator that will make of the user more expresive, they are:

    -> Exclution operator: represented as "!" this operator won't show document that contains a particular group of word

    -> Inclution operator: represented as "^" this one will show only thense documets that contains those group of words

    Performing operators we have to detect them, so will have to call OperatorDetector(),but we will do that in the TF_IDF1 class. Why?
    
    We first have to delete non wanted words and then calculated the weight of the query for each document, after that we will should detect
    priority operator and existence operator
*/

public class Operator
{
    double[,] QueryDatabase;
    public List<string> query;
    public Operator(List<string> query, double[,] QueryDatabase)
    {
        this.QueryDatabase = QueryDatabase;

        this.query = query;
    }

    public static Tuple<List<string>, List<string>> NoExistenceDetector(List<string> query)//if exist a non wanted word, just take it out of the query
    {
        List<string> query1 = new List<string>();

        List<string> watch = new List<string>();

        for (int i = 0; i < query.Count; i++)
        {
            if (query[i] == "!")//if no existence
            {
                for (int j = i; j < query.Count; j++)
                {
                    if (query[j] != "!" && query[j] != "^" && query[j] != "*")//if no operator
                    {
                        watch.Add(query[j]);//this varible will be taken into account for the snippet

                        query.RemoveAt(j);//remove the word and break the cicle to look for another no existence operator

                        break;
                    }
                    else
                    {
                        query.RemoveAt(j);//if operator, take it out of the query and return to the last iteraction
                        j--;
                    }
                }
                i = -1;//if a no existemce operator is found lets start again
            }
        }
        query1 = query;

        Tuple<List<string>, List<string>> returning = new Tuple<List<string>, List<string>>(query1, watch);

        return returning;
    }
    public void OperatorDetector()//detectes "!" and "^" operator
    {
        bool allow = false;

        List<int> aux1 = new List<int>();

        for (int i = 0; i < query.Count; i++)
        {
            if (query[i] == "^")//if "^" is found
                allow = true;

            if (allow == true && query[i] != "!" && query[i] != "^" && query[i] != "*" && query[i] != "~")
            {
                aux1.Add(i);//add the word index at the query array
                allow = false;
            }
        }
        allow = false;
        if (aux1.Count != 0)//and call Existence() with the index of the words that need to exist passed as parameter
            Existence(aux1);

        //we will now verify if exist "*"
        List<int> aux2 = new List<int>();

        List<int> aux2Int = new List<int>();

        int index0 = 0;

        int count = 0;

        for (int i = 0; i < query.Count; i++)
        {
            count = 0;

            if (query[i] == "*")
            {
                index0 = i;

                count = 1;

                while (query[index0] == "*")//counting the amount of "*"
                {
                    count++;
                    index0++;
                }
                i = index0;

                if (query[i] != "!" && query[i] != "^" && query[i] != "~")//if we reach to a query word to power it is taken its index and the amount of "*"
                {
                    aux2.Add(index0);
                    aux2Int.Add(count);
                }
            }
        }
        if (aux2.Count != 0)
            Priority(aux2Int, aux2);//then pass as parameter the list of index and the list of power number
    }
    void Priority(List<int> amount, List<int> indexes)//priority operator
    {
        Tuple<Dictionary<string, int>, List<string>, List<int>, Dictionary<string, int>[]> Index = Reading.Index;

        for (int i = 0; i < indexes.Count; i++)
        {
            if (Index.Item2.Contains(query[indexes[i]]))//we make sure the word is contained
            {
                for (int j = 0; j < Index.Item4.Length; j++)
                {
                    if (amount[i] == 1)//if the number of "*" is one we multiply by two
                        QueryDatabase[j, indexes[i]] *= 2;

                    if (amount[i] > 1)
                        QueryDatabase[j, indexes[i]] *= amount[i] + 1;
                }
            }
            else continue;
        }
    }
    void Existence(List<int> indexes)//existence operator
    {
        Tuple<Dictionary<string, int>, List<string>, List<int>, Dictionary<string, int>[]> Index = Reading.Index;

        for (int i = 0; i < indexes.Count; i++)
        {
            for (int j = 0; j < Index.Item4.Length; j++)
            {
                if (QueryDatabase[j, indexes[i]] == 0)//if the word does not exist or its weight is zero,the rest of the document is unsefull

                    for (int col = 0; col < QueryDatabase.GetLength(1); col++)
                        QueryDatabase[j, col] = 0;
            }
        }
    }
}