# AI pipeline: the labels are the pipeline

GitHub labels carry an issue from rough idea to merged PR. Each label action fires one
autonomous Claude Code Routine run on Anthropic's cloud. Nothing advances by itself:
you add every gating label, the loops automate the mechanics.

**Current scope: build-only.** Routines verify with `dotnet build` and both
`npm run build` steps. They do not boot the site, so no E2E tests and no screenshots.
See [Phase 2](#phase-2-runtime--screenshots).

## The labels

| Label | Who adds it | What happens |
|---|---|---|
| `ai-discuss` | You | **Discuss** routine: questions, critiques and writes up the issue properly. One comment per fire. Your reply fires the next round. No code. |
| `ready-for-ai` | You | **Build** routine: implements the agreed plan on a `claude/` branch, runs the `pr-critic` agent, gets the build green, opens a PR that closes the issue. Removes `ready-for-ai` when done. |
| `ai-blocked` | Build routine | A guardrail stopped the run. The comment says why. Remove it and re-add `ready-for-ai` to retry. |
| `auto-rework` | You, after leaving PR review comments | **Rework** routine: addresses every review comment, re-greens the build, replies, removes the label. Never merges. |
| `auto-merge` | You | Plain GitHub Action. Enables squash auto-merge: merges once `PR Build` is green and there are no conflicts. No agent. |

```
issue -- ai-discuss --> Discuss loop <-- your replies
              |
   remove ai-discuss, add ready-for-ai
              v
         Build loop --> PR (Closes #n) -- auto-rework --> Rework loop
              |                              |
          ai-blocked                     auto-merge --> merged --> deploy.yml
```

## Plumbing

| File | Role |
|---|---|
| `.github/workflows/ai-pipeline.yml` | Issue-side dispatcher. Routines cannot subscribe to issue events, so this forwards `ai-discuss` / `ready-for-ai` labels and discussion replies to the right Routine's API trigger. Comments the session URL back. |
| `.github/workflows/auto-merge.yml` | `auto-merge` label -> `gh pr merge --auto --squash`. |
| `.github/workflows/pr-build.yml` | Build-only CI on PRs. This is the "CI green" gate. |
| `scripts/cloud-setup.sh` | Cloud environment setup script: .NET SDK, Node, restore, npm ci. Cached ~7 days. |
| `.claude/agents/pr-critic.md` | Adversarial reviewer subagent the Build and Rework routines must run before opening/updating a PR. Committed here so every Routine gets it from the clone. |

The Rework routine needs no Action: Routines trigger natively on `pull_request.labeled`.

Every comment the routines or the dispatcher post starts with `<!-- ai-discuss -->`.
Routines act under your GitHub identity, so this marker is how the dispatcher tells
your replies from the bot's and avoids firing itself in a loop.

## One-time setup

### 1. Merge to main

Routines clone `main`, and GitHub only runs issue-event workflows from the default
branch. Everything in the plumbing table must be on `main` first.

### 2. Repo settings and labels

- Settings -> General -> Pull Requests -> tick **Allow auto-merge**.
- Settings -> Branches -> protect `main` and add **build** (from `PR Build`) as a
  required status check. Without this, `auto-merge` merges immediately.
- Create the labels:

```
gh label create ai-discuss   --color 8B5CF6 --description "Shape it: Discuss routine critiques and plans, one comment per fire"
gh label create ready-for-ai --color 22C55E --description "Build it: Build routine implements the plan and opens a PR"
gh label create ai-blocked   --color DC2626 --description "Backstop tripped: see comment, remove and re-add ready-for-ai to retry"
gh label create auto-rework  --color EAB308 --description "Rework routine addresses PR review comments"
gh label create auto-merge   --color 16A34A --description "Approval: squash-merge once PR Build is green"
```

### 3. Cloud environment (once)

At https://claude.ai/code -> Environments -> New:

| Setting | Value |
|---|---|
| Name | `goldfinch-build` |
| Setup script | Contents of `scripts/cloud-setup.sh` |
| Network access | **Custom**, include defaults, plus: `dot.net`, `builds.dotnet.microsoft.com`, `download.visualstudio.microsoft.com`, `ci.dot.net` |
| Environment variables | None for the MVP |

Test it before creating any Routine: start a normal cloud session on this repo with
that environment and ask it to run `dotnet --version`, `dotnet build Goldfinch.sln`,
and `npm run build` in both front-end folders. Fix allowlist gaps interactively.

### 4. Three Routines

At https://claude.ai/code/routines -> New routine. All three: repository
`liamgold/Goldfinch.me`, environment `goldfinch-build`, no connectors.

| Routine | Trigger | Prompt |
|---|---|---|
| `Pipeline: discuss` | **API** | [Discuss prompt](#discuss-prompt) |
| `Pipeline: build` | **API** | [Build prompt](#build-prompt) |
| `Pipeline: rework` | **GitHub**: pull request **labeled**, label filter `auto-rework` | [Rework prompt](#rework-prompt) |

For the two API-triggered routines, the API trigger modal shows the fire URL
(`https://api.anthropic.com/v1/claude_code/routines/trig_.../fire`) and a **Generate
token** button. The token is shown once and cannot be retrieved later; if you lose it,
reopen the routine (pencil icon -> trigger section) and **Regenerate**. The dispatcher
sends the `anthropic-beta` and `anthropic-version` headers the endpoint requires.

### 5. Repository secrets

| Secret | Value |
|---|---|
| `ROUTINE_DISCUSS_URL` / `ROUTINE_DISCUSS_TOKEN` | From `Pipeline: discuss` |
| `ROUTINE_BUILD_URL` / `ROUTINE_BUILD_TOKEN` | From `Pipeline: build` |

### 6. First run

Open a small issue with a one-line description. Add `ai-discuss`. You should get a
dispatcher comment with a session link within a minute and a structured plan comment
when the run finishes. Reply once, get a revision, then swap to `ready-for-ai`.

Things to confirm on the first runs, since the docs do not state them outright:
`gh` inside the Routine can add/remove labels and post comments under your identity,
and pushing to a `claude/` branch plus `gh pr create` works from the sandbox.

## Shared prompt preamble

Each prompt below starts with this block. Paste it in verbatim above the stage text.

```
You are one stage of an automated pipeline for the Goldfinch.me repository (Xperience
by Kentico site, .NET 10, Vite front-end, Node 24). The trigger details are in the fire
payload for this run. If there is no payload, stop and do nothing.

Ground rules for every stage:
- Read CLAUDE.md first and follow it exactly.
- You act under the repository owner's GitHub identity. Every comment you post MUST
  begin with the literal marker <!-- ai-discuss --> on its own first line, otherwise
  the dispatcher will treat it as a human reply and fire again.
- NEVER add the labels ai-discuss, ready-for-ai, auto-rework or auto-merge. Those are
  human decisions. You may add ai-blocked and remove ready-for-ai / auto-rework.
- This environment has no database and no licence key. Do not run the site, do not
  run E2E tests, do not run `dotnet run`. Verification is build-only. The repo is a
  fresh clone with no node_modules, so install before building:
    dotnet build Goldfinch.sln --configuration Release
    (cd src/Goldfinch.Web/wwwroot/sitefiles && npm ci && npm run build)
    (cd src/Goldfinch.Admin/Client && npm ci && npm run build)
  Never set NODE_ENV=production on `npm ci`; it skips the dev deps that hold vite/tsc.
  If `dotnet` is missing, run `bash scripts/cloud-setup.sh` once.
- Use `gh` for all GitHub reads and writes (issue view --comments, issue comment,
  issue edit --add-label/--remove-label, pr create, pr view, api).
```

## Discuss prompt

```
Stage: DISCUSS. Purpose: turn a rough issue into a plan a build agent can execute
without guessing. You write no code and change no files.

1. Read the whole thread: gh issue view <n> --comments. Find the most recent comment
   that starts with <!-- ai-plan --> if one exists; that is your previous plan.
2. Explore the repo enough to ground the plan: which files, which existing patterns,
   what the design handoff in docs/design-handoff/ says. Use the Explore agent for
   broad searches.
3. Decide which ONE of these applies and post exactly one comment:
   a) The issue is too vague to plan. Ask at most three pointed questions, each with
      your best-guess default so the human can just say "yes". Also state anything in
      the request that looks wrong, risky or conflicts with CLAUDE.md.
   b) There is enough to plan. Post the plan comment. If a previous plan exists,
      revise it according to the latest human reply and note what changed.
4. Plan comment format (first line <!-- ai-discuss -->, second line <!-- ai-plan -->):
     ## Plan for #<n>: <one-line title>
     **Problem** one or two sentences in your own words.
     **Approach** numbered steps, concrete: which files, which pattern reused, which
       CSS tokens, which service method.
     **Files** bullet list of files to create/modify.
     **Out of scope** what you deliberately will not touch.
     **Acceptance** how a reviewer confirms it worked (build-only checks + what must be
       eyeballed in the browser/admin since this pipeline cannot run the site).
     **Open questions** or "None".
   Finish with: "Reply to revise. When happy, remove `ai-discuss` and add
   `ready-for-ai`."
5. Post nothing else. Do not create branches, commits or PRs. One comment per run.
```

## Build prompt

```
Stage: BUILD. Purpose: implement the agreed plan and open a reviewable PR, or stop
cleanly with ai-blocked.

1. Read the thread: gh issue view <n> --comments. The plan is the most recent comment
   starting with <!-- ai-plan -->. If there is no plan comment, derive one from the
   issue only if it is unambiguous; otherwise BLOCK (step 7) saying a discuss round is
   needed.
2. Create branch claude/issue-<n>-<short-slug> from main.
3. Implement the plan. Smallest correct change, no unrelated refactors, no committed
   build output (dist/ folders) unless the issue is about built assets.
4. Run all three build commands. Nullable warnings are errors; fix properly, never
   suppress.
5. Run the pr-critic subagent (.claude/agents/pr-critic.md) on your diff with the
   issue and plan. Fix every blocker and should-fix it reports, re-run the builds, and
   run the critic once more. Nits are your call. Two critic rounds maximum.
6. Commit with a conventional message (fix:/feat:/chore:), push, and open the PR
   against main with `gh pr create`. PR body, in this order:
     Closes #<n>
     **Problem** / **Cause** / **Fix** as short paragraphs.
     **Critic** what pr-critic flagged and what you changed in response.
     **Verification** the three build commands and their result. State plainly that
       the site was not run.
     **Reviewer should check** the Acceptance items from the plan that need a browser
       or admin.
   Then comment on the issue (with the marker) linking the PR, and remove the
   ready-for-ai label: gh issue edit <n> --remove-label ready-for-ai
7. BLOCK instead of guessing when: the plan needs runtime access you do not have
   (database content, admin UI, production logs), the build cannot be made green, the
   critic still says DO NOT SHIP after two rounds, or the change would touch
   App_Data/CIRepository, secrets or deployment config without the plan saying so.
   To block: do not open a PR. Comment on the issue (with the marker) with what you
   tried, the exact error or reason, and what a human needs to do. Then:
     gh issue edit <n> --add-label ai-blocked --remove-label ready-for-ai
```

## Rework prompt

```
Stage: REWORK. Purpose: address human review feedback on a PR opened by this
pipeline. Never merge.

1. The payload is a pull_request labeled event. Read the PR, its review comments and
   conversation: gh pr view <pr> --comments, and
   gh api repos/{owner}/{repo}/pulls/<pr>/comments for inline comments. Also read the
   linked issue and its plan comment for intent.
2. Check out the PR branch. For each unresolved human comment decide: do it, or
   explain concisely why not. Prefer doing it.
3. Make the changes, run all three builds, run the pr-critic subagent on the full
   branch diff, fix blockers and should-fixes, re-run builds.
4. Push to the same branch. Reply to each review comment you addressed (with the
   marker) saying what changed, or why you disagree. Post one summary comment on the
   PR (with the marker) listing what changed and the build results.
5. Remove the label: gh pr edit <pr> --remove-label auto-rework
6. If a request cannot be met without runtime access or would break the build, say so
   in the reply, leave that item undone, and still remove auto-rework so the human can
   decide. Do not add ai-blocked to a PR.
```

## Phase 2: runtime + screenshots

Adds a real SQL Server so routines can boot the site, run the Playwright E2E suite and
attach before/after screenshots. Additions only; nothing above changes except the
"build-only" lines in the preamble.

Setup script additions (cached):
- `docker pull mcr.microsoft.com/mssql/server:2022-latest`
- `dotnet tool install -g Kentico.Xperience.DbManager`
- Build the E2E project and install Chromium (`playwright.ps1 install chromium`)

Per-run bootstrap (SessionStart hook in `.claude/settings.json`, guarded by
`CLAUDE_CODE_REMOTE=true`, because running processes are not cached):
1. `docker run -d --name sql -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=$SA_PASSWORD -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest`
2. `kentico-xperience-dbmanager -- -s localhost -d Goldfinch -u sa -p $SA_PASSWORD -a $ADMIN_PASSWORD --hash-string-salt hash-string-salt --license-key $XPERIENCE_LICENSE_KEY`
3. `dotnet run --project src/Goldfinch.Web -- --kxp-ci-restore`
4. Start the site in the background on plain HTTP (`ASPNETCORE_URLS=http://localhost:52623`)

Environment additions: variables `SA_PASSWORD`, `ADMIN_PASSWORD`, `XPERIENCE_LICENSE_KEY`
(localhost licence from the Kentico client portal), `ConnectionStrings__CMSConnectionString`;
network `mcr.microsoft.com`, `*.data.mcr.microsoft.com`. Environment variables are
visible to anyone who can use the environment.

Screenshots: GitHub has no API for attaching images to PR comments. Commit before/after
PNGs to `docs/routines/screenshots/issue-<n>/` on the PR branch and reference them with
relative image links in the PR body.

Known runtime gaps: content item assets live in Azure Blob in production, so images
404 locally; the Lucene search index builds on first request.
