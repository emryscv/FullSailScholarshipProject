namespace MoogleEngine;

public static class QueryTools
{
    /*
        Recive como parametros dos palabras las cuales se va a calcular la distancia lexicografica(cuantas letras hay que cambiar, añadir o
      eliminar para que sean iguales)
        Devuelve la distancia lexicografica entre las dos palabras
    */
    public static int EditDistance(string wordA, string wordB)
    {
        int sizeA = wordA.Length;
        int sizeB = wordB.Length;

        int[,] dp = new int[sizeA + 1, sizeB + 1];

        for (int i = 0; i <= sizeA; i++)
        {
            dp[i, 0] = i;
        }
        for (int j = 0; j <= sizeB; j++)
        {
            dp[0, j] = j;
        }

        for (int i = 1; i <= sizeA; i++)
        {
            for (int j = 1; j <= sizeB; j++)
            {
                if (wordA[i - 1] == wordB[j - 1])
                {
                    dp[i, j] = dp[i - 1, j - 1];
                }
                else
                {
                    dp[i, j] = 1 + Math.Min(dp[i - 1, j - 1], Math.Min(dp[i - 1, j], dp[i, j - 1]));
                }
            }
        }

        return dp[sizeA, sizeB];
    }

    /*
        Recive como parametros la palabra de cual vamos a obtener la que mas se parce en nuestro diccionario y el diccionario 
    donde esta guardado todo nuestro universo de palabras
        Retorna la palabra que mas se parece a la palabra recivida como parametro
    */

    public static string ClosestWord(string queryWord, Dictionary<string, int> wordsIndex)
    {
        int minDistance = int.MaxValue;
        string result = "";

        foreach (string word in wordsIndex.Keys)
        {
            int distance = QueryTools.EditDistance(queryWord, word);
            if (distance < minDistance)
            {
                minDistance = distance;
                result = word;
            }
        }
        return result;
    }

    /*
        Recive como parametros las dos palabras para las cuales se va comprobar cual es la menor distancia entre ellas en cada texto, 
      la estructura de datos en la cual estan almacenadas las posiciones de cada palabra en cada texto y la cantidad de documentos 
      de nuestro universo de documentos.
        Devuelve un array de enteros con la distancia minima entre estas dos palabras en cada documento.
    */

    public static int[] MinDistance(int word1, int word2, List<List<int>[]> wordPositionsInText, int DOCUMENTS_AMOUNT)
    {
        int[] minDistancePerDocument = new int[DOCUMENTS_AMOUNT];
        int minDistance = int.MaxValue;
        int j, k, _wordPosition;

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            j = k = 0;
            while (j < wordPositionsInText[word1][i].Count && k < wordPositionsInText[word2][i].Count)
            {
                _wordPosition = wordPositionsInText[word2][i][k];
                while (j < wordPositionsInText[word1][i].Count && wordPositionsInText[word1][i][j] < _wordPosition)
                {
                    minDistance = Math.Min(_wordPosition - wordPositionsInText[word1][i][j], minDistance);
                    j++;
                }

                if (j < wordPositionsInText[word1][i].Count)
                {
                    _wordPosition = wordPositionsInText[word1][i][j];
                }
                else
                {
                    break;
                }

                while (k < wordPositionsInText[word2][i].Count && wordPositionsInText[word2][i][k] < _wordPosition)
                {
                    minDistance = Math.Min(_wordPosition - wordPositionsInText[word2][i][k], minDistance);
                    k++;
                }
            }
            minDistancePerDocument[i] = minDistance;
            minDistance = int.MaxValue;
        }

        return minDistancePerDocument;
    }

    /*
        Recive como parametros el indice asignado al documento en el cual se busca el snippet, la estructura de datos que contiene todos los datos
      extraidos del preprocesamiento y la query normalizada
        Devuelve una cadena de texto con el snippet que mas importante para la query que se hizo
    */

    public static string FindSnippet(int documentIndex, VectorModel Model, Document[] corpus, List<string> normalizedQuery)
    {
        int maxScoreI = 0;
        double _score = 0d;

        if (corpus[documentIndex].Tokens.Length < 100) return corpus[documentIndex].Text;

        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < normalizedQuery.Count; j++)
            {
                if (corpus[documentIndex].Tokens[i].Lexeme == normalizedQuery[j])
                {
                    _score += Model.TFIDF[Model.WordsIndex[normalizedQuery[j]], documentIndex];
                    break;
                }
            }
        }

        double maxScore = _score;

        for (int i = 100; i < corpus[documentIndex].Tokens.Length; i++)
        {
            for (int j = 0; j < normalizedQuery.Count; j++)
            {
                if (corpus[documentIndex].Tokens[i].Lexeme == normalizedQuery[j])
                {
                    _score += Model.TFIDF[Model.WordsIndex[normalizedQuery[j]], documentIndex];
                }
                if (corpus[documentIndex].Tokens[i - 100].Lexeme == normalizedQuery[j])
                {
                    _score -= Model.TFIDF[Model.WordsIndex[normalizedQuery[j]], documentIndex];
                }
            }
            if (_score > maxScore)
            {
                maxScore = _score;
                maxScoreI = i - 99;
            }
        }

        string snippet = corpus[documentIndex].Text.Substring(
                corpus[documentIndex].Tokens[maxScoreI].Position,
                corpus[documentIndex].Tokens[maxScoreI + 99].Position + corpus[documentIndex].Tokens[maxScoreI + 99].Lexeme.Length - corpus[documentIndex].Tokens[maxScoreI].Position - 1
                );

        return snippet;
    }

    /*
        Recive como parametro la query normalizada sin quitarle los operadores
        Devuelve en una array los operadores asociados a cada palabra
    */
    public static string[] FindOperators(List<string> normalizedQuery)
    {
        string[] operators = new string[normalizedQuery.Count];
        int _k, _m;
        bool flag = false;

        _m = _k = 0;

        for (int i = 0; i < normalizedQuery.Count; i++)
        {
            operators[i] = "";

            for (int j = 0; j < normalizedQuery[i].Length; j++)
            {
                switch (normalizedQuery[i][j])
                {
                    case '!':
                        _m++;
                        operators[_k] = "!";
                        break;
                    case '*':
                        _m++;
                        if (!operators[_k].Contains('!'))
                        {
                            operators[_k] += "*";
                        }
                        break;
                    case '^':
                        _m++;
                        if (!operators[_k].Contains('!'))
                        {
                            operators[_k] = "^" + operators[_k];
                        }
                        break;
                    case '~':
                        _m++;
                        flag = true;
                        break;
                }
            }

            if (_m == normalizedQuery[i].Length)
            {
                operators[_k] = "";
                _k--;
                if (operators[_k] != "!" && flag)
                {
                    operators[_k] += "~";
                    operators[_k + 1] = "~";
                    _m++;
                }
            }

            _k++;
            _m = 0;
            flag = false;
        }

        for (int i = 0; i < normalizedQuery.Count; i++) System.Console.WriteLine(i + " " + operators[i]);

        return operators[.._k];

        //!**el **perro ! e^s e~l ^!papa ~ !de lo!s ^**cachorros
    }
    public static List<string> Normalize(string text, bool isQuery = false)
    {
        char[] spliters = [' ', '\n', '\t', ',', '.', ':', ';'];
        string[] words = text.Split(spliters);
        string newWord;
        List<string> listWords = [];

        foreach (string word in words)
        {
            newWord = "";
            foreach (char c in word)
            {
                char _ac = char.ToLower(c);

                if (char.IsLetterOrDigit(_ac))
                {
                    newWord += _ac.ToString();
                    continue;
                }

                if (isQuery)
                {
                    if (_ac == '!') { newWord += _ac.ToString(); continue; }
                    if (_ac == '^') { newWord += _ac.ToString(); continue; }
                    if (_ac == '~') { newWord += _ac.ToString(); continue; }
                    if (_ac == '*') { newWord += _ac.ToString(); continue; }
                }
            }

            if (newWord != "") listWords.Add(newWord);
        }

        return listWords;
    }
}
