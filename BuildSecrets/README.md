# BuildSecrets

BuildSecrets supplies build-time credentials. Contributors can build and run the
normal MSTest suite without private credentials. This small assembly intentionally
has no debug symbols; other EDDI projects retain their normal debugging support.

## EDDN signing: original key, one local file

In the partial class in your ignored `BuildInjectedSecrets.local.cs`, add this
exact three-line block with the original key supplied by EDDN:

```csharp
#if false
private const string EddnSigningKeyInput = @"original-key-from-EDDN";
#endif
```

Paste the original key without Base64 conversion. Keep the declaration on one
line. This is a C# verbatim string: represent any quote within the key as two
quotes (`""`). Whitespace inside the string is significant and is not trimmed.
The literal `#if false` is required: the original key must never become compiled
code. The generator accepts only this narrowly defined block, not arbitrary C#.

Build normally, then restart EDDI. There are no extra flags, input files, or manual
conversion steps. The build generates randomized XOR-masked bytes and private
reconstruction code under `obj`; BuildSecrets calculates HMAC and clears each
reconstructed buffer. Users receive the normal binaries and need no key settings.

Omit the entire declaration for unsigned builds. Empty, duplicate, malformed, or
non-excluded declarations fail the build without echoing the key. Removing the
block removes stale generated signing source on the next build. A successful build
does not prove EDDN accepts the key; see [release checks](../docs/EDDN-signing.md).

## Migrate older configurations

Replace the old EDDN_SIGNING_KEY comment with the excluded declaration. Old key
comments are rejected with migration guidance. Remove any hand-written
SetEddnSigningKey implementation and helpers used only by it. Preserve your other
credentials. Do not paste an encoded representation as the original key.

## Companion and telemetry credentials

Copy [BuildInjectedSecrets.local.cs.example](BuildInjectedSecrets.local.cs.example)
to `BuildInjectedSecrets.local.cs`, which is already gitignored. If it exists,
merge only the methods you need; do not overwrite existing credentials.
Configure `SetCompanionAppClientId` and `SetTelemetryApiKey` with authorized values,
then build normally. Leaving them unconfigured returns empty strings, preserving
consumers' existing fallback behavior. The `.cs.example` file contains no
credentials and is not compiled; copied unchanged, it leaves EDDN unsigned.

## Private source and release artifacts

The local file is private plaintext build input. Do not commit it, publish generated
source, or distribute intermediate build directories. BuildSecrets enforces
`DebugType=none` and `DebugSymbols=false` in every configuration; attempts to enable
symbols fail. This prevents private source from being embedded in debug artifacts.
Obfuscation discourages casual extraction but does not prevent reverse engineering.

Clean and rebuild after updating from an affected version. Previously built DLLs
and PDBs are not sanitized by changing source. If affected binaries were shared,
rotate the exposed credentials before release.

Normal MSTests cover HMAC, buffer clearing, transport, and disposable Git builds
with ignored input. Artifact checks assert that BuildSecrets has no embedded PDB,
CodeView reference, or separate PDB. No real key or PowerShell test script is needed.
