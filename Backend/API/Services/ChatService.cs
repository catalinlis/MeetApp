using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using API.Data;
using API.DTOs;
using API.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Document = Amazon.DynamoDBv2.DocumentModel.Document;

namespace API.Services;

public class ChatService(IAmazonDynamoDB dynamoDbContext, 
                         EnvironmentVariables variables,
                         DataContext context,
                         IMapper mapper) : IChatService{
    public async Task<List<ChatMessageDynamo>> GetMessages(int user1, int user2, int pageNumber, string sortKey)
    {
        string sortedPair = GetSortedPair(user1, user2);
        string partitionKey = $"CHAT#{sortedPair}";
        int limit = pageNumber;
        var request = new QueryRequest{};

        if(sortKey == ""){
            request = new QueryRequest{
                TableName = variables["DYNAMO_DATABASE_TABLE"],
                KeyConditionExpression = "Pk = :pk",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>{
                    { ":pk", new AttributeValue { S = partitionKey }}
                },
                ScanIndexForward = false,
                Limit = limit
            };
        }
        else{
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>{
                { "Pk", new AttributeValue() { S = partitionKey } },
                { "Sk", new AttributeValue() { S = sortKey } }
            };
            
            request = new QueryRequest{
                TableName = variables["DYNAMO_DATABASE_TABLE"],
                KeyConditionExpression = "Pk = :pk",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>{
                    { ":pk", new AttributeValue { S = partitionKey }}
                },
                ScanIndexForward = false,
                Limit = limit,
                ExclusiveStartKey = lastEvaluatedKey
            };
        }

        var response = await dynamoDbContext.QueryAsync(request);

        return response.Items.Select(item => new ChatMessageDynamo{
            SortKey = item["Sk"].S,
            SenderId = int.Parse(item["SenderId"].N),
            ReceiverId = int.Parse(item["ReceiverId"].N),
            Message = item["Message"].S,
            SentDate = DateTimeOffset.Parse(item["SentDate"].S)
        }).ToList();

    }

    public async Task<bool> SaveMessage(ChatMessageDynamo message){
        
        var chatMessageJson = JsonSerializer.Serialize(message);
        var chatMessageDocument = Document.FromJson(chatMessageJson);
        var chatMessageAttribute = chatMessageDocument.ToAttributeMap();

        var putRequest = new PutItemRequest(){
            TableName = variables["DYNAMO_DATABASE_TABLE"],
            Item = chatMessageAttribute
        };

        var response = await dynamoDbContext.PutItemAsync(putRequest);
        return response.HttpStatusCode == HttpStatusCode.OK;

    }

    public async Task<List<UserMember>> GetChats(AppUser user){
        
        var userChats = await context.Chats.Where(x => x.ChatFirstUserId == user.Id)
                                .OrderBy(x => x.LastMessageSentAt)
                                .Select(x => x.ChatSecondUser).ToListAsync();

        var usersChats = mapper.Map<List<UserMember>>(userChats);

        return usersChats;
    }

    public async Task<int> GetUnreadMessages(int receiver, int sender){
        string sortedPair = GetSortedPair(receiver, sender);
        string partitionKey = $"CHAT#{sortedPair}";

        var request = new QueryRequest{
            TableName = variables["DYNAMO_DATABASE_TABLE"],
            KeyConditionExpression = "Pk = :pk",
            FilterExpression = "ReceiverId = :receiverId AND messageRead = :false",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>{
                { ":pk", new AttributeValue { S = partitionKey }},
                { ":receiverId", new AttributeValue { N = receiver.ToString() }},
                { ":false", new AttributeValue { BOOL = false } }
            }
            
        };

        var response = await dynamoDbContext.QueryAsync(request);

        return response.Items.Count;
    }

    public async Task SetReadMessages(int receiver, int sender){
        string sortedPair = GetSortedPair(receiver, sender);
        string partitionKey = $"CHAT#{sortedPair}";

        var querySortKeys = new QueryRequest{
            TableName = variables["DYNAMO_DATABASE_TABLE"],
            KeyConditionExpression = "Pk = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>{
                {":pk", new AttributeValue { S = partitionKey }}
            }
        };

        var queryResponse = await dynamoDbContext.QueryAsync(querySortKeys);

        var messagesToUpdate = queryResponse.Items
                                .Where(item => item["ReceiverId"].N == receiver.ToString())
                                .ToList();

        foreach(var item in messagesToUpdate){
            var request = new UpdateItemRequest{
                TableName = variables["DYNAMO_DATABASE_TABLE"],
                Key = new Dictionary<string, AttributeValue>{
                    { "Pk", new AttributeValue {S = partitionKey} },
                    { "Sk", new AttributeValue {S = item["Sk"].S } }
                },
                UpdateExpression = "SET messageRead = :true",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>{
                    { ":true", new AttributeValue { BOOL = true } }
                }
            };

            Console.WriteLine(request);

            await dynamoDbContext.UpdateItemAsync(request);
        }
    }

    public async Task AddChat(AppUser sender, AppUser receiver){
        
        var existsChats = await context.Chats.Where(x => (x.ChatFirstUserId == sender.Id && x.ChatSecondUserId == receiver.Id)
                                                      || (x.ChatFirstUserId == receiver.Id && x.ChatSecondUserId == sender.Id))
                                             .ToListAsync();
        
        if( existsChats.Count == 0 ){
            var senderChatUser = new Chat{
                ChatFirstUserId = sender.Id,
                ChatSecondUserId = receiver.Id,
                LastMessageSentAt = DateTimeOffset.UtcNow
            };

            var receiverChatUser = new Chat{
                ChatFirstUserId = receiver.Id,
                ChatSecondUserId = sender.Id,
                LastMessageSentAt = DateTimeOffset.UtcNow
            };

            context.Chats.Add(senderChatUser);
            context.Chats.Add(receiverChatUser);
        
            await context.SaveChangesAsync();

        }
        else{
            foreach(var chat in existsChats){
                chat.LastMessageSentAt = DateTimeOffset.UtcNow;
            }
            await context.SaveChangesAsync();
        }
    }

    private static string GetSortedPair(int id1, int id2){
        return id1 < id2 ? $"{id1}#{id2}" : $"{id2}#{id1}";
    }
}