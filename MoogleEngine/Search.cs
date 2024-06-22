using MoogleEngine.Tools;

namespace MoogleEngine;

class Search
{
    public int DOCUMENTS_AMOUNT { get; private set; }
    public string Suggestion { get; private set; }
    public int QueryWordsAmount { get; private set; }
    public (double, int)[] Result { get; private set; }
    public List<string> NormalizedQuery { get; private set; }

    public Search(string query, VectorModel Model, Dictionary<string, HashSet<string>> synonymsDictionary)
    {
        this.NormalizedQuery = QueryTools.Normalize(query, true);
        string[] operators = QueryTools.FindOperators(this.NormalizedQuery);
        this.NormalizedQuery = QueryTools.Normalize(query);

        string closestWord;

        this.DOCUMENTS_AMOUNT = Model.DOCUMENTS_AMOUNT;
        this.Suggestion = "";
        this.QueryWordsAmount = this.NormalizedQuery.Count;
        this.Result = new (double, int)[DOCUMENTS_AMOUNT];


        for (int i = 0; i < DOCUMENTS_AMOUNT; i++)
        {
            this.Result[i] = (0d, i);
        }

        for (int i = 0; i < QueryWordsAmount; i++)
        {
            if (Model.WordsIndex.ContainsKey(this.NormalizedQuery[i]))
            {
                ProcessQueryWord(Model, operators, this.NormalizedQuery[i], i);
                Suggestion += this.NormalizedQuery[i] + " ";
            }
            else
            {
                closestWord = QueryTools.ClosestWord(this.NormalizedQuery[i], Model.WordsIndex);
                Suggestion += closestWord + " ";
            }
            if (synonymsDictionary.ContainsKey(this.NormalizedQuery[i]))
            {
                foreach (string synonym in synonymsDictionary[this.NormalizedQuery[i]])
                {
                    Console.WriteLine(synonym);
                    if (Model.WordsIndex.ContainsKey(synonym))
                    {
                        ProcessQueryWord(Model, operators, synonym, i, true);
                    }
                }
            }
        }

        ApplyOperators(Model, operators);

        this.Result = Array.FindAll(this.Result, (item) => item.Item1 != 0);
        Array.Sort(this.Result, (item1, item2) => item2.Item1.CompareTo(item1.Item1));

        for (int i = 0; i < this.Result.Length; i++)
        {
            System.Console.Write(this.Result[i]);
        }
        System.Console.WriteLine();
    }

    void ProcessQueryWord(VectorModel Model, string[] operators, string word, int i, bool isSynonym = false)
    {
        int _wordIndex = Model.WordsIndex[word];
        for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
        {
            if (operators[i].Contains('*'))
            {
                int _power = operators[i].LastIndexOf('*') - operators[i].IndexOf('*') + 1;
                this.Result[j].Item1 += (isSynonym ? Model.TFIDF[_wordIndex, j] / 2 : Model.TFIDF[_wordIndex, j]) * Math.Pow(2.0, _power);
            }
            else
            {
                this.Result[j].Item1 += (isSynonym ? Model.TFIDF[_wordIndex, j] / 2 : Model.TFIDF[_wordIndex, j]);
            }
        }
    }

    void ApplyOperators(VectorModel Model, string[] operators)
    {
        List<int[]> distancesList = [];
        int _wordIndex;

        for (int i = 0; i < this.QueryWordsAmount; i++)
        {
            if (Model.WordsIndex.ContainsKey(this.NormalizedQuery[i]))
            {
                _wordIndex = Model.WordsIndex[this.NormalizedQuery[i]];
                if (operators[i] == "!" || operators[i].Contains('^'))
                {
                    for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
                    {
                        if ((operators[i] == "!" && Model.TF[_wordIndex][j] != 0) || (operators[i].Contains('^') && Model.TF[_wordIndex][j] == 0))
                        {
                            this.Result[j].Item1 *= 0;
                        }
                    }
                }
                if (operators.Length > i + 1 && operators[i].Contains('~') && operators[i + 1].Contains('~') && Model.WordsIndex.ContainsKey(this.NormalizedQuery[i + 1]))
                {
                    distancesList.Add(QueryTools.MinDistance(Model.WordsIndex[this.NormalizedQuery[i]], Model.WordsIndex[this.NormalizedQuery[i + 1]], Model.WordPositionsInText, DOCUMENTS_AMOUNT));
                }
            }
        }

        double _maxScore = int.MinValue;
        for (int i = 0; i < DOCUMENTS_AMOUNT; i++) if (this.Result[i].Item1 > _maxScore) _maxScore = this.Result[i].Item1;

        for (int i = 0; i < distancesList.Count; i++)
        {
            for (int j = 0; j < DOCUMENTS_AMOUNT; j++)
            {
                if (this.Result[j].Item1 != 0)
                {
                    this.Result[j].Item1 += _maxScore / (double)distancesList[i][j];
                }
            }
        }
    }
}