namespace Nats.Reply.Examples.ChatAssistant.Contracts;

public class ChatAssistantQuestion
{
    public required string Question { get; init; }
}

public class ChatAssistantQuestionResponse
{
    public required string Answer { get; init; }
}