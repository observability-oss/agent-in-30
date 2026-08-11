# AI LAB — Build an AI Agent in 30 Minutes

Welcome! In this lab you'll run your own AI agent — built on **Microsoft
Agent Framework** — and then, together, wire it up to the
[Progress AI Observability Platform](https://www.telerik.com/ai-observability-platform)
and watch everything it does: every model call, every tool it uses, every
token and cent it spends.

The agent in this repo deliberately starts as a **black box** — no
observability at all. Making it a glass box is what we do live.

No agent-building experience needed. If you can run two commands in a
terminal, you're ready.

## What you need

- The [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A free **Progress Observability** account — sign up at
  [observability.progress.com](https://observability.progress.com/) — with an
  **Integration key** created (starts with `ac_p_…`; keep it handy for the
  session)
- A model key for OpenRouter — **we share one at the event**, nothing to do
  now
- Recommended: a coding agent — **GitHub Copilot** (VS Code) or
  **Claude Code**, signed in — we use it to add the observability wiring
  with a single prompt (a manual path exists too)

## Set up before the event

Two minutes:

```bash
git clone https://github.com/observability-oss/agent-in-30.git
cd agent-in-30
dotnet run
```

No model key yet — that's expected. The app checks your setup and tells
you **"you're ready for the event"**. That line is all you need to see.

Anything not working?
[TROUBLESHOOTING.md](TROUBLESHOOTING.md) has the fix — or bring the message
along and we'll sort it together in the first minutes.

## At the event

1. Add the shared model key when we post it, and meet your agent for real:

   ```bash
   dotnet user-secrets set "OpenRouter:ApiKey" "...shared at the event..."
   dotnet run
   ```

2. Notice what you *can't* answer: how many model calls did that reply
   take? What did it cost? Did it really call the tools?

3. Open [INSTRUMENT.md](INSTRUMENT.md) — we wire up observability
   together, live: your Integration key from prep, then **one prompt to
   your coding agent** does the instrumentation while you watch the diff.
   Fallen behind? `git checkout instrumented` is the finished version.

## Where to go next

- [dotnet-agent-starter](https://github.com/observability-oss/dotnet-agent-starter)
  — the full-featured starter: retrieval, task tools, and evaluations
- [progress-observability plugin](https://github.com/observability-oss/progress-observability-plugin)
  — instrument your own existing app from Claude Code or GitHub Copilot
