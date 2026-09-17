using Nats.Reply.Client;
using Nats.Reply.Client.Hosting;
using Nats.Reply.Examples.ChatAssistant.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddNatsReplyClient(cfg=>
{
    cfg.ConfigureOptionsFromConfigurationSection(builder.Configuration.GetSection("Nats"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("/question", async (ChatAssistantQuestion question, INatsReplyClient client, CancellationToken cancellationToken) =>
    {
        var response = 
            await client.RequestAsync<ChatAssistantQuestion, ChatAssistantQuestionResponse>(
                SubjectNames.ChatAssistantQuestion, question, cancellationToken: cancellationToken);
        return response.Result;
    })
    .WithName("ChatAssistantQuestion");

app.Run();
