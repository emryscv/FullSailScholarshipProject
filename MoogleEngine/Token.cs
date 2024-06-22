namespace MoogleEngine;

public class Token{
    public string Lexeme {get; private set;}
    public int Position {get; private set;}

    public Token(string lexeme, int position){
        this.Lexeme = lexeme;
        this.Position = position;
    }
}