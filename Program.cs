// AI LAB — build an AI agent in 30 minutes.
//
// The FINISHED version — main's bare agent with every INSTRUMENT.md step
// applied. If you fell behind during the live part, you are looking at the
// answer key.

using System.ClientModel;
using AgentLab;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using Progress.Observability.Extensions.AI;

// Environment variables override user secrets; secrets live outside the
// repo (dotnet user-secrets), so there is never a key in this folder.
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

var orKey = config["OpenRouter:ApiKey"];
var model = config["OpenRouter:Model"] ?? "google/gemini-2.5-flash";
var baseUrl = config["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1";

var appName = config["Progress:Observability:AppName"] ?? "ailab-agent";
var obsKey = config["Progress:Observability:ApiKey"];

if (!string.IsNullOrWhiteSpace(obsKey))
{
    ObservabilityTracer.Initialize(new ObservabilityOptions
    {
        AppName = appName,
        ApiKey = obsKey,
    });
}

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

if (!string.IsNullOrWhiteSpace(obsKey))
    chat = chat.AddObservability(); // THE LINE — see INSTRUMENT.md step 3c

List<AITool> tools =
[
    AIFunctionFactory.Create(LabTools.GetWeather),
    AIFunctionFactory.Create(LabTools.GetCurrentTime),
];

var agent = chat.AsAIAgent(
    instructions: "You are the AI LAB assistant. For weather or time questions, " +
                  "always call your tools instead of guessing, then answer briefly.",
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

ObservabilityTracer.Shutdown();
return 0;
