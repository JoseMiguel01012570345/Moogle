namespace MoogleEngine;

/* 
    This class contains a method and a property, all of them static in order to initilize before a query is made
*/
public class Synonimus
{
    public static List<string>[]? SynonimuS { get; set; }
    public static void MySynonimus()//tokenization of the synonimus txt
    {
        string[] s = File.ReadAllLines(Directory.GetCurrentDirectory() + "/synonimus.txt");

        List<string>[] synonimus = new List<string>[s.Length];

        for (int i = 0; i < s.Length; i++)//initializing the synonimus list
        {
            synonimus[i] = new List<string>();
        }

        string[] words = new string[0];

        for (int x = 0; x < s.Length; x++)
        {
            words = s[x].Split(" ");

            for (int i = 0; i < words.Length; i++)
            {
                string speech = "";//restart empty the variable

                for (int j = 0; j < words[i].Length; j++)
                {
                    if (char.IsLetterOrDigit(words[i][j]) || words[i][j] == '-')

                        speech += words[i][j];

                    else//if the char is not a value lets add what we in speech to synonimus and continue to the next iteraction
                    {
                        if (speech != "")
                            synonimus[x].Add(speech.ToLower());

                        speech = "";//and clean

                        continue;
                    }
                }
                if (speech != "")//lets add them in lower case
                    synonimus[x].Add(speech.ToLower());
            }
        }
        SynonimuS = synonimus;
    }
}