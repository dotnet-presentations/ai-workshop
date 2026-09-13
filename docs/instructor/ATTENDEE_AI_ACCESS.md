# Instructor Note: Attendee AI Access Fallback

> **Audience:** Workshop leaders only. This is **not** part of the attendee-facing
> content. Attendees are expected to bring their own [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) access (see
> [Part 1 - Setup](../../Part%2001%20-%20Setup/README.md)).

## The problem

The workshop's main path assumes each attendee can provision a [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry)
resource and deploy `gpt-5-mini` + `text-embedding-3-small`. In practice, a
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
  dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-PROXY-URL/api/v1"
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
2. Assign the same models the workshop uses: `gpt-5-mini` and
   `text-embedding-3-small`.
3. **Activate the event before distributing keys.** Attendee event keys are
   unauthorized until the event is active and the current time is inside its
   configured start/end window.
4. **Size the caps for RAG.** The manual-RAG part ingests the workshop manuals,
   which fires many embedding calls. Do one test ingestion through the proxy and
   set the daily-request / token caps above that, or proxy users will hit the wall
   mid-ingestion.
5. **Run an attendee-key smoke test.** Set the event start time early enough to run this before attendees arrive. Once the event is active, use a real attendee
   event key (not an administrator credential) with the attendee endpoint and
   verify both one chat completion against `gpt-5-mini` and one embedding request
   against `text-embedding-3-small`. Do not record the key, endpoint, or private
   join information in the repository.
6. Distribute join info out-of-band (e.g., a private gist / QR code shown in the
   room) — keep it out of the public workshop materials.

## Troubleshooting attendee access

| Symptom | Check |
| --- | --- |
| HTTP 401 for every attendee key | Confirm the event is active and the current time is inside its scheduled window. |
| HTTP 404 or another route error | Confirm the endpoint given to attendees is the Azure OpenAI-compatible proxy endpoint and includes the proxy API route, commonly `/api/v1`. |
| Chat works but RAG fails | Confirm `text-embedding-3-small` is assigned to the event and that the event limits allow the manual-RAG ingestion traffic. |
| Only one attendee fails | Reissue or recopy that attendee's event key, then repeat the smoke test. |

## Deciding not to set it up

That's fine — it's a safety net, not a requirement. Without it, attendees who
can't get Foundry access can pair up with someone who can, or follow along and run
the code later.
