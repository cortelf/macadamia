using Microsoft.Extensions.AI;
using Macadamia.Core;
using Macadamia.Examples.ChatAssistant.Contracts;

namespace Macadamia.Server.Examples.Hosting.Handlers;

public class ChatAssistantQuestionHandler(IChatClient chatClient)
    : NatsMessageHandler<ChatAssistantQuestion, ChatAssistantQuestionResponse>
{
    protected override async Task<NatsReplyWrapper<ChatAssistantQuestionResponse>> HandleAsync(
        ChatAssistantQuestion message, CancellationToken cancellationToken)
    {
        List<ChatMessage> chatHistory =
        [
            new(ChatRole.System, "You are a helpful assistant"),
            new(ChatRole.User, message.Question),
        ];

        var clankerResponse = await chatClient.GetResponseAsync(chatHistory, cancellationToken: cancellationToken);
        return Ok(new ChatAssistantQuestionResponse
        {
            Answer = clankerResponse.Text
        });
    }
}