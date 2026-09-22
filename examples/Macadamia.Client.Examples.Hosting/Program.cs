using Macadamia.Client;
using Macadamia.Client.Hosting;
using Macadamia.Examples.ChatAssistant.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMacadamiaClient(cfg=>
{
    cfg.ConfigureOptionsFromConfigurationSection(builder.Configuration.GetSection("Nats"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("/question", async (ChatAssistantQuestion question, IMacadamiaClient client, CancellationToken cancellationToken) =>
    {
        var response = 
            await client.RequestAsync<ChatAssistantQuestion, ChatAssistantQuestionResponse>(
                SubjectNames.ChatAssistantQuestion, question, cancellationToken: cancellationToken);
        return response.Result;
    })
    .WithName("ChatAssistantQuestion");

app.Run();
