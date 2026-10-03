# Native provenance

`NATIVE_PROVENANCE.json` records SHA-256 and size for all 4 checked-in
native binaries, their configured NuGet paths, declared upstream components,
source revisions/archive hashes where available, and hashes of build inputs.
The baseline repository commit is the revision inspected before this manifest
was introduced. No native files were rebuilt or replaced during recording.

## Verify locally

From the tryAGI workspace:

```sh
python3 scripts/verify-native-provenance.py SpeexDspSharp
python3 scripts/verify-native-provenance.py --require-complete SpeexDspSharp
```

The first command checks integrity and inventory, and prints historical build
attestation gaps. The second fails on those gaps as well. A successful integrity
check does not prove that a binary was built from the declared upstream sources.

## Evidence and limitations

Both Linux binaries match the payload bytes from pinned Debian 1.2.1-1 packages; package and payload hashes are recorded. The macOS dylib matches the local Homebrew 1.2.1 installation; the formula source hash and installation receipt metadata are recorded. The Windows upstream package could not be fetched from either MSYS2 endpoint during review, so its package checksum is explicitly unknown. None of these observations recreates an original build attestation.

## Updating the baseline

Review upstream source/revision, license, download checksums, build environment,
patches and each output hash when rebuilding. Preserve the evidence alongside
the changed manifest. Do not regenerate hashes merely to silence a mismatch.
Unresolved source/build evidence must remain visible; never relabel it verified
without an actual attestation.

## Recording new operations

The repository's native build/refresh script now records each successful RID
operation under `natives/attestations/<rid>/<content-sha256>.json`. Python 3.11 or newer and
the system `file` tool are required. Receipts are included in future NuGet
packages under `native-attestations/`.

A build receipt includes source file/tree hashes and available Git revision,
compiler/tool versions and executable hashes, configuration file hashes, explicit
parameters, actual output hashes/sizes/formats, and timestamp. Opus Linux builds
record the immutable Docker image ID plus compiler/package-version reports from
inside the build container. A mismatched architecture, stale/missing output,
missing input or unavailable tool version prevents receipt creation.

SpeexDSP refresh operations produce **import** receipts: they record downloaded
package bytes or Homebrew formula/library inputs and local extraction tools.
They explicitly leave the upstream compiler provenance unknown and do not close
a source-build recording gap. macOS import is recorded for osx-arm64 only.

Receipts are unsigned local observations, not independently authenticated or
reproducible-build proofs. A successful build record can satisfy the local
recording check only while its output hashes and required RID coverage match.
A preserved receipt from an older generation is historical evidence. Old
binaries keep their historical gaps; no receipts were synthesized for them.

After a rebuild, review changes to the output baseline and update its hashes
and source inputs intentionally before publication. Recording does not silently
rewrite `NATIVE_PROVENANCE.json`. Alternate encoder/output layouts require a
reviewed baseline inventory matching the intended RID. Do not run builders
concurrently against the same source/output family.
