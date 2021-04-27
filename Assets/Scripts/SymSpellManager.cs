using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymSpellManager
{
    private SymSpell symSpell;

    public SymSpellManager()
    {
        LoadDictionary();
    }

    public void LoadDictionary()
    {
        //create object
        int initialCapacity = 82765;
        int maxEditDistanceDictionary = 2; //maximum edit distance per dictionary precalculation

        symSpell = new SymSpell(initialCapacity, maxEditDistanceDictionary);

        //load dictionary
        string dictionaryPath = Application.dataPath + @"\SymSpell\frequency_dictionary_en_82_765.txt";

        int termIndex = 0; //column of the term in the dictionary text file
        int countIndex = 1; //column of the term frequency in the dictionary text file

        if (!symSpell.LoadDictionary(dictionaryPath, termIndex, countIndex))
        {
            Debug.Log("File not found!");
            return;
        }
    }

    public string GetSuggestion(string word)
    {
        int maxEditDistanceLookup = 1; //max edit distance per lookup (maxEditDistanceLookup<=maxEditDistanceDictionary)
        var suggestionVerbosity = SymSpell.Verbosity.Closest; //Top, Closest, All
        var suggestions = symSpell.Lookup(word, suggestionVerbosity, maxEditDistanceLookup);

        if (suggestions.Count == 0)
            return word;

        return suggestions[0].term;
    }
    public string[] GetSuggestions(string word, int count)
    {
        if(word == "")
            return new string[] { word };

        int maxEditDistanceLookup = 1; //max edit distance per lookup (maxEditDistanceLookup<=maxEditDistanceDictionary)
        var suggestionVerbosity = SymSpell.Verbosity.Closest; //Top, Closest, All
        var suggestions = symSpell.Lookup(word, suggestionVerbosity, maxEditDistanceLookup);

        if (suggestions.Count == 0)
            return new string[] { word };

        int resultCount = suggestions.Count < count - 1 ? suggestions.Count : count - 1;

        string[] results = new string[resultCount+1];

        results[0] = word;

        for (int i = 0; i < resultCount; i++)
            results[i+1] = suggestions[i].term;

        return results;
    }
}
