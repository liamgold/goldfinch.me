#!/usr/bin/env bash
# Cloud environment setup script for Claude Code Routines / Claude Code on the web.
#
# Paste the contents of this file into the "Setup script" field of the cloud
# environment at https://claude.ai/code (Environments), or set the field to:
#     bash scripts/cloud-setup.sh
# The script is self-contained: it works whether or not the repository has
# been cloned yet, and is safe to re-run.
#
# It runs as root on Ubuntu 24.04, is cached per environment (~7 days), and
# should finish within ~5 minutes. Only installed files survive into the
# session; running processes do not.
#
# MVP scope: build-only. Installs the .NET SDK + Node so `dotnet build` and both
# `npm run build` steps work. No SQL Server, no licence key, no site boot.
# See docs/routines/ai-pipeline.md for the phase 2 additions.

set -euo pipefail

REPO_DIR="${CLAUDE_PROJECT_DIR:-$(pwd)}"
DOTNET_ROOT_DIR="/usr/share/dotnet"

echo "==> Repo dir: $REPO_DIR"

# ---------------------------------------------------------------------------
# .NET SDK (not preinstalled in the sandbox). Version comes from global.json.
# ---------------------------------------------------------------------------
if [[ -f "$REPO_DIR/global.json" ]]; then
  DOTNET_VERSION="$(sed -n 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$REPO_DIR/global.json" | head -n1)"
else
  DOTNET_VERSION=""
fi
DOTNET_CHANNEL="10.0"

if command -v dotnet >/dev/null 2>&1 && dotnet --list-sdks 2>/dev/null | grep -q "^${DOTNET_CHANNEL}"; then
  echo "==> .NET SDK ${DOTNET_CHANNEL}.x already present: $(dotnet --version)"
else
  echo "==> Installing .NET SDK (${DOTNET_VERSION:-channel $DOTNET_CHANNEL})"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
  if [[ -n "$DOTNET_VERSION" ]]; then
    /tmp/dotnet-install.sh --version "$DOTNET_VERSION" --install-dir "$DOTNET_ROOT_DIR" \
      || /tmp/dotnet-install.sh --channel "$DOTNET_CHANNEL" --install-dir "$DOTNET_ROOT_DIR"
  else
    /tmp/dotnet-install.sh --channel "$DOTNET_CHANNEL" --install-dir "$DOTNET_ROOT_DIR"
  fi
  ln -sf "$DOTNET_ROOT_DIR/dotnet" /usr/local/bin/dotnet
fi

# Make DOTNET_ROOT visible to every later shell (session included).
cat > /etc/profile.d/dotnet.sh <<PROFILE
export DOTNET_ROOT="$DOTNET_ROOT_DIR"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_NOLOGO=1
PROFILE
export DOTNET_ROOT="$DOTNET_ROOT_DIR"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_NOLOGO=1

echo "==> dotnet: $(dotnet --version)"

# ---------------------------------------------------------------------------
# Node. The sandbox ships 20/21/22; .nvmrc asks for 24. Use nvm when present,
# otherwise fall back to whatever `node` is on PATH (Vite + webpack run on 22).
# ---------------------------------------------------------------------------
NODE_WANTED="24"
if [[ -f "$REPO_DIR/.nvmrc" ]]; then
  NODE_WANTED="$(tr -d '[:space:]' < "$REPO_DIR/.nvmrc")"
fi

export NVM_DIR="${NVM_DIR:-$HOME/.nvm}"
if [[ -s "$NVM_DIR/nvm.sh" ]]; then
  # shellcheck disable=SC1091
  . "$NVM_DIR/nvm.sh"
  echo "==> nvm install $NODE_WANTED"
  nvm install "$NODE_WANTED" >/dev/null
  nvm alias default "$NODE_WANTED" >/dev/null
  nvm use default >/dev/null
else
  echo "==> nvm not found; using system node"
fi
echo "==> node: $(node --version)  npm: $(npm --version)"

# ---------------------------------------------------------------------------
# Warm caches so the session's first build is fast. Only when the repo is here.
# ---------------------------------------------------------------------------
if [[ -f "$REPO_DIR/Goldfinch.sln" ]]; then
  echo "==> dotnet restore"
  (cd "$REPO_DIR" && dotnet restore Goldfinch.sln)

  echo "==> npm ci (sitefiles)"
  (cd "$REPO_DIR/src/Goldfinch.Web/wwwroot/sitefiles" && npm ci --no-audit --no-fund)

  echo "==> npm ci (admin client)"
  (cd "$REPO_DIR/src/Goldfinch.Admin/Client" && npm ci --no-audit --no-fund)
else
  echo "==> Goldfinch.sln not found in $REPO_DIR; skipping restore/npm ci (will run in-session)"
fi

echo "==> Setup complete"
