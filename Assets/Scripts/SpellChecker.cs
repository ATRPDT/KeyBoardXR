using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpellChecker 
{
    HashSet<string> WORD_COUNTS = new HashSet<string>();
    private readonly static string alphabet = "abcdefghijklmnopqrstuvwxyz";
    // Start is called before the first frame update
    public SpellChecker() { LoadDictionary(); }
    public void LoadDictionary()
    {
        WORD_COUNTS = new HashSet<string>(File.ReadAllLines(@"E:\UnityPr\KeyBoardUnited\Assets\SwipeType\EnglishWords.txt"));

    }
    public List<string> EditDistance1(string word)
    {
        word = word.ToLower(); //toLowerCase().split('');
        // var results = [];
        List<string> results = new List<string>();

        //Adding any one character (from the alphabet) anywhere in the word.
        for (var i = 0; i <= word.Length; i++)
        {
            for (var j = 0; j < alphabet.Length; j++)
            {
                var newWord = word;
                newWord = newWord.Insert(i, alphabet[j].ToString()); //splice(i, 0, alphabet[j]);
                results.Add(newWord);
            }
        }

        //Removing any one character from the word.
        if (word.Length > 1)
        {
            for (var i = 0; i < word.Length; i++)
            {
                var newWord = word;
                newWord = newWord.Remove(i, 1);// splice(i, 1);
                results.Add(newWord);
            }
        }

        //Transposing (switching) the order of any two adjacent characters in a word.
        if (word.Length > 1)
        {
            for (var i = 0; i < word.Length - 1; i++)
            {
                var newWord = word;
                var r = newWord[i];
                var r2 = newWord[i + 1];//Remove(i, 1); // splice(i, 1);
                newWord = newWord.Remove(i, 2); //Replace(newWord[i], newWord[i + 1]); //splice(i + 1, 0, r[0]);
                newWord = newWord.Insert(i, r2.ToString());//(newWord[i + 1], r);
                newWord = newWord.Insert(i, r.ToString());
                results.Add(newWord);
            }
        }

        //Substituting any character in the word with another character.
        for (var i = 0; i < word.Length; i++)
        {
            for (var j = 0; j < alphabet.Length; j++)
            {
                var newWord = word;
                newWord = newWord.Remove(i, 1);
                newWord = newWord.Insert(i, alphabet[j].ToString());
                results.Add(newWord);
            }
        }


        return results;
    }
    public string Correct(string word)
    {

        if (WORD_COUNTS.Contains(word))
        {
            //return new List<string>() { word };
            return word;
            
        }

        var editDistance1Words = EditDistance1(word);
        var editDistance2Words = new List<string>();
        List<string> correctedWord = new List<string>();

        for (var i = 0; i < editDistance1Words.Count; i++)
        {
            editDistance2Words.AddRange(EditDistance1(editDistance1Words[i]));
        }
        
        for (var i = 0; i < editDistance1Words.Count; i++)
        {
            if (WORD_COUNTS.Contains(editDistance1Words[i]))
            {
                correctedWord.Add(editDistance1Words[i]);
            }
        }

        for (var i = 0; i < editDistance2Words.Count; i++)
        {
            if (WORD_COUNTS.Contains(editDistance2Words[i]))
            {
                correctedWord.Add(editDistance2Words[i]);
            }
        }

        if (correctedWord.Count == 0)
        {
            
            return word;
           
        }

        return correctedWord[0];

    }
}
  
