// AI LAB — build an AI agent in 30 minutes.
//
// A Microsoft Agent Framework agent with two tools — and, so far, no
// observability at all. It works, but it's a black box: how many model calls
// did that answer take? What did it cost? Did it really use its tools?
// In the lab we wire it up to Progress Observability, live, and find out —
// the steps are in INSTRUMENT.md.

using System.ClientModel;
using AgentLab;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

// Environment variables override user secrets; secrets live outside the
// repo (dotnet user-secrets), so there is never a key in this folder.
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var orKey = config["OpenRouter:ApiKey"];
var model = config["OpenRouter:Model"] ?? "google/gemini-2.5-flash";
var baseUrl = config["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1";

if (string.IsNullOrWhiteSpace(orKey))
{
    Console.WriteLine("Setup looks good — you're ready for the event!");
    Console.WriteLine("The model key is shared live on Wednesday. Once you add it, the agent wakes up:");
    Console.WriteLine("  dotnet user-secrets set \"OpenRouter:ApiKey\" \"...\"");
    return 0;
}

IChatClient chat = new OpenAIClient(
        new ApiKeyCredential(orKey),
        new OpenAIClientOptions { Endpoint = new Uri(baseUrl) })
    .GetChatClient(model)
    .AsIChatClient();
Console.WriteLine($"model: {model} via OpenRouter");

List<AITool> tools =
[
    AIFunctionFactory.Create(LabTools.GetWeather),
    AIFunctionFactory.Create(LabTools.GetCurrentTime),
    AIFunctionFactory.Create(LabTools.GetLocalTimeByCity),
];

var agent = chat.AsAIAgent(
    instructions: "You are the AI LAB assistant. For weather or time questions, " +
                  "always call your tools instead of guessing. If the user asks for time in a city, " +
                  "use GetLocalTimeByCity with the city from their prompt, then answer briefly.",
    name: "ailab-agent",
    tools: tools);

var session = await agent.CreateSessionAsync();

var scripted = "What's the weather in Sofia right now, and what time is it?";
Console.WriteLine($"\nYou:   {scripted}");
var response = await agent.RunAsync(scripted, session);
Console.WriteLine($"Agent: {response.Text}");

Console.WriteLine("\nYour turn — ask anything. Empty line exits.");
while (true)
{
    Console.Write("You:   ");
    var question = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(question)) break;
    var reply = await agent.RunAsync(question, session);
    Console.WriteLine($"Agent: {reply.Text}\n");
}

return 0;
