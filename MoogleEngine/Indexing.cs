namespace MoogleEngine;

/*
Brief description of the class:

First of all we need to extract the words that database has got. To goal this it is necesary verify if what we have in a folder named as Content is a txt editor.
After this achived it is necesary read these all documents and tokenize each word under a patron to try to make the search easier.

Detailled description of WordCorpus():

1.At WordCorpus() it is readed each line of a txt document and splited into subarray so to make tokenization easier iteraction through each splited chunck of the text.

2.Iteraction by chunks is taken part in a <for> iteraction. In here there is another iteraction to determine letters or digits. If so
it is add to a string that will stop addition when the lenght of the chunk is reach or if the current char is no letter or nor digit.Afterward it is called to the 
ToLower() method to put each word in lower case.

3.When a word is made by the last patron is now time to log it in database. There are two possibilities, a word is already in database or not so we ask for that.
If is not in; we put it in. Each word has a label that characterise it in database in order to calculate its weight in database.So we add a labels like:

-Frecuency: frecuency of the word in each document

-FreacuencyInEachDoc: measure how many document has got the word

-Words: it is a list of the words that has got the database

-dic: it is a special data structure(dictionary)that access to each word in less time. If we were to access throught Words it wiil be more time the access

There is a bool list to mark if a word is taken into account in <FrecuencyInEachDoc> in order not to add more than one time the same document in case 
there were more the one ocurrence of the same word in the same doc 

This structure will be indexed in cache for the porpose not to charge for every query and make the process of a search very expensive */

public class Reading
{
    public static Tuple<Dictionary<string, int>, List<string>, List<int>, Dictionary<string, int>[]>? Index { get; set; }//in this structure we will store the words its frecuency and so on...to use it at TF_IDF class
    static bool VerificatorTXT(string name)//returns if a file is a txt or not
    {
        if (name.Contains(".txt")) return true;
        return false;
    }
    //reads the whole of the files returning a array of strings that conatains every name of each file
    public static string[] ReadDrirectory(string path)//get the txt documents that exit in the corresponding path
    {
        int trueTXT = 0;

        for (int i = 0; i < Directory.GetFiles(path).Length; i++)

            if (VerificatorTXT(Directory.GetFiles(path)[i])) trueTXT++;

        string[] files = new string[trueTXT];

        for (int i = 0; i < files.Length; i++)
        {
            if (VerificatorTXT(Directory.GetFiles(path)[i]))
            {
                files[i] = Directory.GetFiles(path)[i];
            }
        }
        return files;
    }
    public static void WordsCorpus()//Words the database
    {
        string[] words = new string[0];

        char[] abcd = new char[0];

        string[] database = TF_IDF1.Documents.ToArray();

        string[] databaseLines;

        string little = "";

        List<string> Words = new List<string>();

        List<int> freqInEachDoc = new List<int>();

        Dictionary<string, int> dic = new Dictionary<string, int>();

        Dictionary<string, int>[] frecuency = new Dictionary<string, int>[database.Length];


        for (int i = 0; i < frecuency.Length; i++)//incializing dicctionary<frecuency>

            frecuency[i] = new Dictionary<string, int>();


        //this structure is the way it is the database organized
        Tuple<Dictionary<string, int>, List<string>, List<int>, Dictionary<string, int>[]> Indexing = new Tuple<Dictionary<string, int>, List<string>, List<int>, Dictionary<string, int>[]>(dic, Words, freqInEachDoc, frecuency);

        List<bool>[] watcher = new List<bool>[database.Length];

        for (int i = 0; i < database.Length; i++)//inizializing list
            watcher[i] = new List<bool>();

        int superIndex = 0;

        for (int j = 0; j < database.Length; j++)
        {
            if (database[j] == null) continue;

            databaseLines = File.ReadAllLines(database[j]);//read the lines of the document so to do split and get each possible word

            for (int t = 0; t < databaseLines.Length; t++)//to tokanize word
            {
                words = databaseLines[t].Split(" ");

                for (int i = 0; i < words.Length; i++)
                {
                    string s = "";

                    abcd = words[i].ToCharArray();

                    for (int z = 0; z < abcd.Length; z++)
                    {
                        if ((char.IsLetterOrDigit(abcd[z]) == false) && abcd[z] != '-') continue;

                        s += abcd[z];
                    }

                    little = s.ToLower();//lets get it all lower so to avoid any possible dismatch in case dismatiching ocurrece by Capital letters

                    if (dic.ContainsKey(little))
                    {
                        if (frecuency[j].ContainsKey(little))
                            frecuency[j][little]++;//lets add another frecuence for the word in the document

                        else frecuency[j].Add(little, 1);//otherwise let add it as one ocurrence

                        superIndex = dic[little];

                        if (watcher[j][superIndex] == false)//if the word is not mark; it means that is not taken into account this document
                        {
                            freqInEachDoc[superIndex]++;//lets add another frecuency for the word by document

                            watcher[j][superIndex] = true;//and mark it so not to add another frecuency in case there were another word like this one in the current document
                        }
                    }
                    else //if the word is not in database we need to add its labels
                    {
                        frecuency[j].Add(little, 1);

                        dic.Add(little, Words.Count);

                        Words.Add(little);

                        freqInEachDoc.Add(1);

                        superIndex = dic[little];

                        for (int a = 0; a < database.Length; a++)//it is need to add this label to each template
                            watcher[a].Add(false);

                        watcher[j][superIndex] = true; //and make it as true
                    }
                }
            }
        }
        Index = Indexing;
    }
    public static string FindName(string fullname)//find the name of the given document
    {
        if (fullname == null) fullname = "";

        string name = "";

        char[] s = fullname.ToCharArray();

        List<char> auxiliar = new List<char>();

        for (int i = fullname.Length - 1; i >= 0; i--)
        {
            if (s[i] != '/' && s.Length != 0)//adding to auxiliar till find '/' from end to the beginning
                auxiliar.Add(s[i]);

            else break;
        }

        for (int j = auxiliar.Count() - 1; j >= 4; j--)//and reverse the auxiliar list
        {
            name += auxiliar.ElementAt(j).ToString();
        }

        return name;
    }
}