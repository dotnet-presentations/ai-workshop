# Instructor Note: Attendee AI Access Fallback

> **Audience:** Workshop leaders only. This is **not** part of the attendee-facing
> content. Attendees are expected to bring their own Azure AI Foundry access (see
> [Part 1 - Setup](../../Part%201%20-%20Setup/README.md)).

## The problem

The workshop's main path assumes each attendee can provision an Azure AI Foundry
resource and deploy `gpt-4.1-mini` + `text-embedding-3-small`. In practice, a
handful of attendees won't be able to — no subscription, corporate policy blocks,
quota/region issues, or they simply run out of time during setup.

## Recommended fallback: azure-ai-proxy-lite

[microsoft/azure-ai-proxy-lite](https://github.com/microsoft/azure-ai-proxy-lite)
is a managed, multi-tenant proxy that sits in front of **your** Azure AI resources
and hands out time-bound, capacity-capped access to attendees.

Why it fits this workshop with no content changes:

- **Drop-in compatible with the Azure OpenAI .NET SDK.** Attendees who use the
  proxy set the exact same two user-secrets as everyone else — just pointed at the
  proxy URL with an issued event key:

  ```bash
  dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-PROXY-URL/"
  dotnet user-secrets set "AzureOpenAI:Key" "ATTENDEE-EVENT-KEY"
  ```

  The `AzureOpenAIClient(endpoint, key)` code path in every part is unchanged.
- **Covers the whole AI half.** Chat completions, embeddings, and streaming, so
  both the hands-on chat app and the manual-RAG part work. It also offers MCP
  pass-through for the agentic half.
- **Built for events.** Time-bound windows (bind it to the workshop day),
  self-service join via GitHub OAuth **or** shared codes (good for in-person), and
  per-attendee daily request + per-request token caps.
- **One-command deploy** with `azd up` (Container Apps + Static Web App + Table
  Storage).

## If you set it up

1. Deploy the proxy ahead of time (`azd up`) and create an event scoped to the
   workshop date.
2. Assign the same models the workshop uses: `gpt-4.1-mini` and
   `text-embedding-3-small`.
3. **Size the caps for RAG.** The manual-RAG part ingests the workshop manuals,
   which fires many embedding calls. Do one test ingestion through the proxy and
   set the daily-request / token caps above that, or proxy users will hit the wall
   mid-ingestion.
4. Distribute join info out-of-band (e.g., a private gist / QR code shown in the
   room) — keep it out of the public workshop materials.

## Deciding not to set it up

That's fine — it's a safety net, not a requirement. Without it, attendees who
can't get Foundry access can pair up with someone who can, or follow along and run
the code later.
