namespace Core.Models;

public class ErrorMessage(string mensagem, object? data = null)
{
    public string Message { get; set; } = mensagem;
    public object? Data { get; set; } = data;
}
