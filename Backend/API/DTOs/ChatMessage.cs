namespace API.Entities;

public class ChatMessage{
    public string Sender { get; set; } = string.Empty; 
    public string Receiver { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset SentDate { get; set; }
}