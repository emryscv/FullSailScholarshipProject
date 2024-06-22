using System.Diagnostics;
using MoogleEngine.Tools;

namespace MoogleEngine;

public static class Moogle
{
    public static Dictionary<string, SearchResult> memory = new Dictionary<string, SearchResult>();

    public static SearchResult Query(string query, VectorModel model, Document[] corpus, Dictionary<string, HashSet<string>> synonymsDictionary)
    {
        Stopwatch cronos = new Stopwatch();
        cronos.Start();

        if (memory.TryGetValue(query, out SearchResult? value))
        {
            cronos.Stop();
            System.Console.WriteLine((double)cronos.ElapsedMilliseconds / 1000);
            System.Console.WriteLine(model.TF.Count);
            return value;
        }

        Search result = new Search(query, model, synonymsDictionary);
        SearchItem[] items;

        if (result.Result.Length == 0)
        {
            items = [new SearchItem("No hay coincidencias", "", 0f)];
        }
        else
        {
            items = new SearchItem[result.Result.Length];
            for (int i = 0; i < result.Result.Length; i++)
            {
                int _documentIndex = result.Result[i].Item2;

                items[i] = new SearchItem(corpus[_documentIndex].Name, QueryTools.FindSnippet(_documentIndex, model, corpus, result.NormalizedQuery), (float)result.Result[i].Item1);
            }
        }

        cronos.Stop();
        System.Console.WriteLine((double)cronos.ElapsedMilliseconds / 1000);
        System.Console.WriteLine(model.TF.Count);

        memory.Add(query, new SearchResult(items, result.Suggestion));

        return new SearchResult(items, result.Suggestion);
    }
}
