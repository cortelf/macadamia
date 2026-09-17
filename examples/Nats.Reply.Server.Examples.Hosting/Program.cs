using System.ClientModel;
using System.Reflection;
using Microsoft.Extensions.AI;
using Nats.Reply.Examples.ChatAssistant.Contracts;
using Nats.Reply.Server.Examples.Hosting;
using Nats.Reply.Server.Hosting;
using OpenAI;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddNatsReplyServer(cfg =>
    cfg.ConfigureOptionsFromConfigurationSection(builder.Configuration.GetSection("Nats"))
        .Subscribe<ChatAssistantQuestion>(SubjectNames.ChatAssistantQuestion)
    );

builder.Services.RegisterNatsMessageHandlersFromAssembly(Assembly.GetCallingAssembly());

var options = builder.Configuration.GetSection("Assistant")
    .Get<AIAssistantOptions>()!;
var chatClient =
    new OpenAIClient(
            new ApiKeyCredential(options.Token),
            new OpenAIClientOptions { Endpoint = options.Endpoint })
        .GetChatClient(options.Model)
        .AsIChatClient();
builder.Services.AddSingleton(chatClient);

var host = builder.Build();
host.Run();