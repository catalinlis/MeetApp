using Amazon.DynamoDBv2.DataModel;

namespace API.Entities;

[DynamoDBTable("Messages")]
public class ChatMessageDynamo{
    [DynamoDBHashKey]
    public string Pk => $"CHAT#{GetSortedPair(SenderId, ReceiverId)}";
    public string Sk => $"{SentDate:yyyyMMddHHmmss}#{ShortGuid()}";
    public string SortKey { get; set; } = string.Empty;
    [DynamoDBProperty]
    public int SenderId { get; set; }
    [DynamoDBProperty]
    public int ReceiverId { get; set; }
    [DynamoDBProperty]
    public string Message { get; set; } = string.Empty;
    [DynamoDBProperty]
    public DateTimeOffset SentDate { get; set; } = DateTimeOffset.UtcNow;
    public bool messageRead { get; set; } = false;

    private static string GetSortedPair(int id1, int id2){
        return id1 < id2 ? $"{id1}#{id2}" : $"{id2}#{id1}";
    }

    private string ShortGuid(){
        return Guid.NewGuid().ToString().Substring(0,6);
    }
}