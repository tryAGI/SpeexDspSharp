#!/usr/bin/env bash
# Refresh prebuilt libspeexdsp binaries for SpeexDspSharp.
#
# Sources:
#   Linux  — Debian bookworm `libspeexdsp1` package (BSD-licensed prebuilt)
#   macOS  — Homebrew `speexdsp` formula (arm64; matches our dev machines)
#
# Re-run whenever bumping SpeexDSP versions or wiring up new targets. Commit
# the resulting *.so / *.dylib files alongside the script so CI has zero
# network dependencies during builds.

set -euo pipefail

# Pinned upstream version (Debian package revision). Bump and re-run to
# refresh. SpeexDSP itself is at 1.2.1 — no patches we want to land.
DEB_VERSION="1.2.1-1"
DEB_MIRROR="http://ftp.debian.org/debian/pool/main/s/speexdsp"

script_dir="$(cd "$(dirname "$0")" && pwd)"
cd "$script_dir"

mkdir -p linux macos

# --- Linux (Debian prebuilts) -------------------------------------------------

download_linux() {
    # $1 = Debian arch (arm64 | amd64)
    # $2 = output filename relative to natives/linux/
    # $3 = multi-arch subdirectory name inside the .deb
    local arch="$1" outfile="$2" libdir="$3"
    local tmp
    tmp="$(mktemp -d)"
    trap "rm -rf '$tmp'" RETURN

    echo "[refresh] linux/$arch — fetching libspeexdsp1_${DEB_VERSION}_${arch}.deb"
    curl -fsSL -o "$tmp/pkg.deb" \
        "${DEB_MIRROR}/libspeexdsp1_${DEB_VERSION}_${arch}.deb"

    (
        cd "$tmp"
        ar x pkg.deb
        tar xJf data.tar.xz
    )

    cp "$tmp/usr/lib/${libdir}/libspeexdsp.so.1.5.2" "linux/${outfile}"
    file "linux/${outfile}"
}

download_linux arm64 libspeexdsp-arm64.so aarch64-linux-gnu
download_linux amd64 libspeexdsp.so       x86_64-linux-gnu

# --- Windows x64 (MSYS2 clang64 prebuilt) -------------------------------------
# The clang64 build has no mingw runtime DLL deps — only KERNEL32 and the
# Universal CRT (shipped with Windows 10+). 166 KB.

download_windows() {
    local tmp
    tmp="$(mktemp -d)"
    trap "rm -rf '$tmp'" RETURN

    echo "[refresh] windows/x64 — fetching mingw-w64-clang-x86_64-speexdsp-${DEB_VERSION}-any.pkg.tar.zst"
    curl -fsSL -o "$tmp/pkg.tar.zst" \
        "https://mirror.msys2.org/mingw/clang64/mingw-w64-clang-x86_64-speexdsp-${DEB_VERSION}-any.pkg.tar.zst"

    if ! command -v zstd >/dev/null 2>&1 && ! tar --use-compress-program=unzstd --version >/dev/null 2>&1; then
        echo "[refresh] WARNING: zstd not available — install via 'brew install zstd' or 'apt install zstd'"
        return 1
    fi

    tar --use-compress-program=unzstd -xf "$tmp/pkg.tar.zst" -C "$tmp"

    cp "$tmp/clang64/bin/libspeexdsp-1.dll" windows/speexdsp.dll
    chmod u+w windows/speexdsp.dll
    file windows/speexdsp.dll
}

mkdir -p windows
download_windows

# --- macOS (Homebrew, arm64) --------------------------------------------------

if command -v brew >/dev/null 2>&1; then
    if ! brew list speexdsp >/dev/null 2>&1; then
        echo "[refresh] installing speexdsp via brew"
        brew install speexdsp
    fi
    src="$(brew --prefix speexdsp)/lib/libspeexdsp.1.dylib"
    if [[ -f "$src" ]]; then
        # Homebrew dylibs are r--r--r--; remove existing target so cp doesn't
        # fail when restaging the file.
        rm -f macos/libspeexdsp.dylib
        cp "$src" macos/libspeexdsp.dylib
        chmod u+w macos/libspeexdsp.dylib
        echo "[refresh] macos — copied $src"
        file macos/libspeexdsp.dylib
    else
        echo "[refresh] WARNING: expected dylib not found at $src"
    fi
else
    echo "[refresh] skipping macOS — brew not on PATH"
fi

echo "[refresh] done. Commit the updated binaries:"
echo "  git add natives/{linux,windows,macos}"
