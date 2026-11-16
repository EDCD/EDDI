# Optional deferred speech with SpeakAfter

This proposal moves deferred reporting out of the core event pipeline. `Body mapped`
continues to reach monitors, the Speech Responder, VoiceAttack, and other responders
in its normal order. No shipped personality opts in automatically.

```cottle
{SpeakAfter("Combined mapping report", 200, "Surface signals detected")}
```

`SpeakAfter(scriptToSpeak, maximumDelayMs, scriptToWaitFor?, args?)` schedules a
**new evaluation** of a named script. It returns no text and does not wait in the
calling event handler. This is different from adding a pause to already-generated
speech. The scheduled output goes through the usual subtitle and speech queue path.

- `maximumDelayMs` is an integer from 0 to 30000. It is configurable per call;
  200 is an example to test, not a measured guarantee about journal timing.
- If `scriptToWaitFor` is provided, completion of that top-level Speech Responder
  script's handling can release the report early. An empty string or omitted third
  argument gives a timer-only report. Nested `InvokeScript()` calls are not completion
  notifications. A silent or disabled event script still completes its handling.
- Specifically, a request originating in `Body mapped` and waiting for
  `Surface signals detected` requires live **SAA** signals with the original system
  address and body ID, and a timestamp no earlier than the mapping. FSS, replay,
  missing identity, a manual call without the signal event, and unrelated signals
  cannot satisfy that wait. They do not prevent the timer fallback.
- Pass a fourth argument map to retain settings or values the helper needs. Pass
  `args` explicitly to forward another helper's arguments. Without it, `args` is
  empty. Local variables from the scheduling script are not implicitly captured.
- `event` is the original scheduling event, not the event that released the wait.
  Other runtime variables, including `state`, are compiled when the report runs.
  A helper should identify its body using `event.bodyname` / `event.bodyId` and
  `event.systemAddress`, rather than assuming the global current-body context is
  still the same. An explicitly supplied VoiceAttack voice/priority is retained;
  otherwise the deferred script's normal priority is used.
- The helper receives `speakafter.matched` and `speakafter.timedout`, indicating how
  it was released. Neither flag promises that biological signals exist.
- Each request runs at most once. Timer callbacks and ordinary speech generation
  are serialized; no Cottle document or runtime is retained for background reuse.
  The timeout makes a report eligible to run, not a wall-clock playback deadline.
- Pending reports are canceled on location/session reset, stop, reload, or personality
  change. A changed current system also prevents a stale report from rendering.
  They are not persisted across restarts. At most 32 requests may be pending.
- Deferred reports cannot call `SpeakAfter()` again, including through nested helpers.
  Non-Speech-Responder resolver calls cannot schedule speech. Disabled target scripts
  do not render. Quiet/subtitle settings remain effective for deferred output.

## Adapting a personality

Keep immediate context updates in `Body mapped`, and move the spoken mapping report
into a separate helper. Do not defer the whole event script if it sets context used
by other responders. Restore the helper's body-specific context from its original
`event` before calling older helpers that read global context states.

The signal event must still process and save its data even when its overlapping
speech is suppressed. A deferred function cannot automatically decide which portions
of two custom reports overlap. This remains a personality decision.

The following small example demonstrates the lifecycle. It deliberately stores only
one body's report data per keyed entry, and is not a replacement for an existing
personality's signal cache or reporting logic. It supports signals without a preceding
mapping event (including previously mapped bodies), and signals arriving after the
fallback. Production personalities should also clear their transient pending keys
on location/session reset or personality reload because canceled requests do not run
the helper's cleanup code.

### Body mapped

```cottle
{set key to cat("mapping_wait_", event.systemAddress, "_", event.bodyId)}
{SetState(key, true)}
{SetState(cat(key, "_genera"), "")}
{SpeakAfter("Combined mapping report", 200, "Surface signals detected", ["key": key])}
```

Keep any existing immediate context setup before this scheduling call. In a full
personality, gate this call on the user's reporting preferences, and pass those
preferences in the argument map if the helper needs them.

### Surface signals detected

```cottle
{if event.detectionType = "SAA":
    {set key to cat("mapping_wait_", event.systemAddress, "_", event.bodyId)}
    {set genera to []}
    {for genus in event.genera:
        {set genera to cat(genera, [genus.localizedName])}
    }
    {SetState(cat(key, "_genera"), join(genera, ", "))}
    {if !GetState(key) && len(genera) > 0:
        {event.bodyname} has {join(genera, ", ")}.
    }
}
```

Always perform existing signal caching before suppressing overlapping output.
Retain unrelated commentary as appropriate. The example's pending flag suppresses
only the biological sentence while the combined report is outstanding.

### Combined mapping report

```cottle
Mapping complete for {event.bodyname}.
{set genera to GetState(cat(args.key, "_genera"))}
{if genera: Biological genera include {genera}.}
{SetState(args.key, void)}
{SetState(cat(args.key, "_genera"), void)}
```

If matching signals arrive in time, their script saves the genera, suppresses its
standalone sentence, then releases the helper. If they do not, the helper reports
mapping completion and clears the pending flag; later signals can speak separately.
If no mapping request exists, the signal script reports normally. Repeated identical
signal events may still require the personality's existing duplicate-suppression
policy; this mechanism deduplicates a scheduled report, not journal events.

## Validation and limits

`Tests/DeferredSpeechTests.cs` covers deferred Cottle evaluation, explicit arguments
through `InvokeScript()`, original event identity, early release, timer fallback with
no further journal events, late/unrelated/replayed/FSS signals, multiple bodies,
concurrent timer/signal release, cancellation, serialization, and invalid calls.

The focused tests can be run with the EDDI test project on Windows. A Linux isolation
harness also compiles the production scheduler, Cottle function, ScriptResolver,
ScriptInvoker, and SpeechResponder against Cottle 2.1 with substitutes for platform
services. This does not replace a full Windows build, the complete EDDI test suite,
or testing with live journals. In particular, 200 ms still needs real-world timing
validation. The original journal sample has only whole-second timestamps.
