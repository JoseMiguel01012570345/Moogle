namespace MoogleEngine;
class Contains
{
    //this method is thought verify if a word is in a list of words by putting them all in lower case
    public static bool Contain(string findDistance, List<string> gWords)
    {
        for (int i = 0; i < gWords.Count; i++)
        {
            if (gWords[i].ToLower() == findDistance.ToLower()) return true;
        }

        return false;
    }
}