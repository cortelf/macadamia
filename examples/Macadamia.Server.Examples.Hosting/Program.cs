using System.ClientModel;
using System.Reflection;
using Microsoft.Extensions.AI;
using Macadamia.Examples.ChatAssistant.Contracts;
using Macadamia.Server.Examples.Hosting;
using Macadamia.Server.Hosting;
using OpenAI;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddMacadamiaServer(cfg =>
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