using System.Text.Json;

namespace MoogleEngine.Tools;

public static class Preprocessing
{
    /*
        Devuelve una lista con los contenidos de los documentos y un array con las direcciones relativas de cada documento
    */
    public static HashSet<string> LoadStopWords()
    {
        string json = File.ReadAllText(Path.Join("..", "stopwords.json"));
        StopWords deserializedJson = JsonSerializer.Deserialize<StopWords>(json);

        return [.. deserializedJson.Words];
    }

    public static Document[] LoadDocuments(HashSet<string> StopWords)
    {
        string[] directory = Directory.GetFiles(Path.Join("..", "Content"));
        List<Document> corpus = [];

        foreach (string document in directory)
        {
            char[] splitters = ['/', '\\'];
            StreamReader sr = new StreamReader(document);
            string documentText = sr.ReadToEnd();
            Console.WriteLine(sr.CurrentEncoding);

            if (documentText != null)
            {
                string _name = document.Split(splitters).Last(); //Get the file's name with extension.

                corpus.Add(new(
                    _name[.._name.LastIndexOf('.')], //Get the file's name without extension.
                    documentText,
                    Parse(documentText, StopWords)
                ));
            }
        }

        System.Console.WriteLine(directory.Length + " " + corpus.Count);

        return [.. corpus];
    }

    /*
        Devuelve un diccionario que relaciona cada palabra con el conjunto de todos sus posibles sinonimos
    */
    public static Dictionary<string, HashSet<string>> LoadAndCreateSynonymsDictionary(HashSet<string> StopWords)
    {
        Dictionary<string, HashSet<string>> synonymsDictionary = new Dictionary<string, HashSet<string>>();
        string json = File.ReadAllText(Path.Join("..", "sinonimos.json"));
        Sinonyms deserializedJson = JsonSerializer.Deserialize<Sinonyms>(json);

        for (int i = 0; i < deserializedJson.Words.Length; i++)
        {
            for (int j = 0; j < deserializedJson.Words[i].Length; j++)
            {
                if (!synonymsDictionary.ContainsKey(deserializedJson.Words[i][j]))
                {
                    synonymsDictionary.Add(deserializedJson.Words[i][j], []);
                }
                for (int k = 0; k < deserializedJson.Words[i].Length; k++)
                {
                    if (k != j)
                    {
                        Token[] parse = Parse(deserializedJson.Words[i][k], StopWords);
                        if(parse.Length > 0) synonymsDictionary[deserializedJson.Words[i][j]].Add(parse[0].Lexeme);
                    }
                }
            }
        }

        return synonymsDictionary;
    }

    public static Token[] Parse(string text, HashSet<string> StopWords)
    {
        List<Token> tokens = [];
        string _word = "";
        int startingPosition = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsLetterOrDigit(text[i]))
            {
                _word += char.ToLower(text[i]);
            }
            else
            {
                if (_word != "")
                {
                    if (!StopWords.Contains(_word))
                    {
                        Token _token = new(_word, startingPosition);
                        tokens.Add(_token);
                    }
                    _word = "";
                }

                startingPosition = i + 1;
            }
        }

        if (_word != "")
        {
            Token _token = new(_word, startingPosition);
            tokens.Add(_token);
        }

        return [..tokens];
    }
}