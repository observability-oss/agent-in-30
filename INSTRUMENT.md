# Wiring up observability — the live part of the lab

We do this together during the session. The observability wiring is added by
the **instrument-agent skill** running inside your coding agent — you paste
one prompt and review the diff it proposes. Fallen behind at any point?
`git checkout instrumented` is the finished version.

## What you need for this part

- A coding agent: **GitHub Copilot** (VS Code) or **Claude Code**, signed in
- No coding agent? The **manual path** at the bottom of this page is four
  copy-paste edits — same result

## 1 — Add your two settings

Your **Integration key** (`ac_p_…`) comes from
[observability.progress.com](https://observability.progress.com/) → API Keys.

```bash
dotnet user-secrets set "Progress:Observability:ApiKey" "ac_p_...your key..."
dotnet user-secrets set "Progress:Observability:AppName" "yourname-agent"
```

Use your own name in `yourname-agent` — the lab shares one workspace, and
the app name is how you'll spot *your* traces among everyone else's.

## 2 — One prompt

Open this folder in your coding agent and paste:

```text
Install https://github.com/telerik/observability-skills from GitHub, use instrument-agent to add tracing to this app.
```

The skill inspects the project, reports what it found and the change it
intends to make, then adds the instrumentation: the SDK package, the tracer
initialization, one line on the chat-client chain, and a flush on exit.
**Read the diff before accepting it** — it's small, and understanding it is
the point of the lab.

## 3 — Run it again

```bash
dotnet run
```

Ask a question, exit with an empty line, then open **Observations** at
[observability.progress.com](https://observability.progress.com/) and filter
by your app name. Give ingestion up to a minute. You're looking for this:

```
invoke: ailab-agent
├── llm_call            call 1 — the model asks for the tools
├── tool GetWeather     your C# method, visible as a span
├── tool GetCurrentTime
└── llm_call            call 2 — the model answers using the results
```

Two model calls for one question is normal — a model can only *ask* for a
tool; your app runs it and sends the result back. And notice the second
call costs more than the first: it carries the whole conversation plus the
tool results. That's the kind of thing you can only see with tracing on.

---

## Appendix — the manual path

No coding agent handy? The same result in one package and four edits.

```bash
dotnet add package Progress.Observability.Instrumentation
```

**a — one more `using`**, with the others at the top of `Program.cs`:

```csharp
using Progress.Observability.Extensions.AI;
```

**b — read the settings and start the tracer**, right after the
`var baseUrl = ...` line:

```csharp
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
```

**c — THE LINE.** After `chat` is created, just before the
`List<AITool> tools` block:

```csharp
if (!string.IsNullOrWhiteSpace(obsKey))
    chat = chat.AddObservability();
```

Position matters: **between `AsIChatClient()` and `AsAIAgent()`** this one
line captures the model calls *and* every tool invocation. Put it anywhere
else and the app still compiles, still emits model spans — and silently
loses the tool spans. Observability failures are quiet; that's the point of
this platform. (The guard matters too: without a key, a bare
`.AddObservability()` tries to configure itself from the environment and
throws.)

**d — flush on exit.** After the `while` loop, just before `return 0;`:

```csharp
ObservabilityTracer.Shutdown();
```

A short-lived console app that exits without this loses its last batch of
spans.
