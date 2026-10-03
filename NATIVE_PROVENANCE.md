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
