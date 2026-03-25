namespace Core.Models;

public class SuccessMessage
{
    public SuccessMessage(string message)
    {
        Message = message;
    }
    public SuccessMessage(string message, object? data)
    {
        Data = data;
        Message = message;
    }
    public string Message { get; set; }
    public object? Data { get; set; }
}
