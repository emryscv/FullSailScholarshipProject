/*
    Implementacion del modelo de espacio vectorial:
       -Cada palabra es representada como un vector donde la componente i-esima es su valor de
        TfIdf para el documento i-esimo
       -Metodos implementados:
            -Constructor:
            -CalcTf: Recorro todos los documentos del directorio y les aplico el metodo Normalize a cada uno y obtengo
                     una lista con todas las palabras del documento y luego cuento cuantas veces aparece cada una en cada
                     documento y esta informacion la almaceno en una lista de arrays(utilizado como matriz)
                     que se llama tf(actua como matriz)
            -AddWord: Este Metodo es llamado cuando encuentro una palabra nueva que no aparece en mi diccrionario de
                      palabras wordIndex y este le asocia a la palbra su fila de la matriz

*/
using System.Diagnostics;

namespace MoogleEngine;

public class VectorModel
{
    public Dictionary<string, int> WordsIndex { get; private set; }
    public List<double[]> TF { get; private set; }
    public double[,] TFIDF { get; private set; }
    public List<List<int>[]> WordPositionsInText { get; private set; }
    public int DOCUMENTS_AMOUNT { get; private set; }

    public VectorModel(Document[] Corpus)
    {
        this.DOCUMENTS_AMOUNT = Corpus.Length;
        this.WordsIndex = [];
        this.WordPositionsInText = [];

        this.TF = [];
        CalcTF(Corpus);

        this.TFIDF = new double[this.TF.Count, this.DOCUMENTS_AMOUNT];
        CalcTFIDF();
    }

    void AddWord(string word, int wordIndex, int documentIndex, int wordPosition)
    {
        this.WordsIndex.Add(word, wordIndex);
        this.TF.Add(new double[this.DOCUMENTS_AMOUNT]);
        this.TF[wordIndex][documentIndex] = 1;
        this.WordPositionsInText.Add(new List<int>[this.DOCUMENTS_AMOUNT]);

        for (int i = 0; i < DOCUMENTS_AMOUNT; i++) this.WordPositionsInText[wordIndex][i] = [];

        this.WordPositionsInText[wordIndex][documentIndex].Add(wordPosition);
    }

    public void CalcTF(Document[] corpus)
    {
        int wordIndex = 0;
        int documentIndex = 0;
        int wordPosition = 1;

        foreach (Document document in corpus)
        {
            foreach (var word in document.Tokens)
            {
                if (WordsIndex.TryGetValue(word.Lexeme, out int value))
                {
                    this.TF[value][documentIndex]++; //Aumentar la frecuencia de la palabra
                    this.WordPositionsInText[value][documentIndex].Add(wordPosition); //Llenar la tabla con las posiciones de las palabras en los textos    
                }
                else
                {
                    AddWord(word.Lexeme, wordIndex, documentIndex, wordPosition);
                    wordIndex++;
                }
                wordPosition++;
            }

            double max = double.MinValue;

            for (int i = 0; i < wordIndex; i++) if (this.TF[i][documentIndex] > max) max = this.TF[i][documentIndex];
            for (int i = 0; i < wordIndex; i++) this.TF[i][documentIndex] /= max;

            documentIndex++;
            wordPosition = 1;
        }
    }

    double CalcIDF(double[] wordTf)
    {
        int df = Array.FindAll(wordTf, (word) => word != 0).Length;
        double logArgument = (double)this.DOCUMENTS_AMOUNT / (1 + df);
        double wordIdf = Math.Log10(logArgument);

        return wordIdf;
    }

    public void CalcTFIDF()
    {
        double _wordIdf;

        for (int i = 0; i < this.TF.Count; i++)
        {
            _wordIdf = CalcIDF(this.TF[i]);
            for (int j = 0; j < this.DOCUMENTS_AMOUNT; j++)
            {
                this.TFIDF[i, j] = this.TF[i][j] * _wordIdf;
            }
        }
    }
}


