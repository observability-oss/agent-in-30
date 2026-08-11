# Troubleshooting

Symptoms first, fastest fix first. Every entry here is something we've
actually seen happen. Trace-related entries apply after the live
instrumentation step ([INSTRUMENT.md](INSTRUMENT.md)) — before it, no
traces is the expected state, that's the lab.

| Symptom | Cause | Fix |
|---|---|---|
| Agent runs fine, **zero traces** in the platform | The key is an MCP key (`acm_…`) — it can't write traces | Set the **Integration key**: `dotnet user-secrets set "Progress:Observability:ApiKey" "ac_p_..."`. |
| Agent runs fine, zero traces, key is correct | The app exited without flushing | Exit with an **empty line** — that runs the clean shutdown. Closing the window with ✕ can lose the last batch. |
| No traces yet, ran 10 seconds ago | Ingestion takes a moment | Give it up to a minute and refresh before changing anything. |
| Can't find *my* traces | Looking at the whole shared workspace | Filter Observations by **your** app name (`yourname-agent`). |
| My traces show under `ailab-agent`, not my name | `Progress:Observability:AppName` secret not set — the code falls back to the default | `dotnet user-secrets set "Progress:Observability:AppName" "yourname-agent"` and run again. |
| `401` from OpenRouter | Key mistyped or not added yet | Re-run the `user-secrets set "OpenRouter:ApiKey" ...` command with the key exactly as shared. |
| `429` from OpenRouter | The shared key hit its rate/budget limit | Tell the host — there is a backup key ready to drop in chat. |
| `dotnet: command not found`, or version below 8 | .NET SDK missing | Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and reopen the terminal. |
| First run is slow | NuGet restoring packages | Normal on first run — and exactly why the setup happens before the event. |
| `NU1605: Detected package downgrade: OpenTelemetry` | You copied the code into your own project pinning an older OpenTelemetry | Raise your project's OpenTelemetry reference to 1.15.3 or later. Doesn't occur in this repo. |
| Traces never arrive on a corporate laptop | Proxy or VPN blocks `collector.observability.progress.com:443` | Try a hotspot — or follow along now and run at home; everything works after the event too. |
| Weather tool says it couldn't reach the service | Network or proxy blocks `api.open-meteo.com` / `geocoding-api.open-meteo.com` | Try a hotspot. The agent still answers and the tool span still appears — it records the failure, which is its own observability lesson. |
| The model answers but never uses tools | The model ignored its instructions (rare) | Ask it directly: "use your tools — weather in Sofia?" |

## For hosts

- **Rotate the shared OpenRouter key right after the event** — it will have
  been visible in many chat windows.
- The scripted first question exists so everyone's first trace has the same
  shape: `llm_call → tool ×2 → llm_call`. Debug against that shape.
- Someone with nothing working: invite them to watch now and run afterwards —
  everything works the same after the event.
- Ingestion lag talking point: "if your trace isn't there after a minute,
  wave at me — before that, it's just the pipeline doing pipeline things."
