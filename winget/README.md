# winget Submission

Package identifier: **`HichamBouchikhi.OpenMyTunnel`**

The manifests under `manifests/` mirror the folder structure expected by
[microsoft/winget-pkgs](https://github.com/microsoft/winget-pkgs).

---

## Initial (first-time) submission

The `winget-submit` CI job only works for updates to an already-accepted package.
The very first version must be submitted manually:

1. **Validate locally** (requires Windows + winget):
   ```
   winget validate --manifest winget/manifests/h/HichamBouchikhi/OpenMyTunnel/1.0.3
   ```

2. **Fork** [microsoft/winget-pkgs](https://github.com/microsoft/winget-pkgs).

3. Copy the three YAML files into your fork at the same relative path:
   ```
   manifests/h/HichamBouchikhi/OpenMyTunnel/1.0.3/
   ```

4. Open a PR against `microsoft/winget-pkgs:master`.
   The Vedder bot will auto-validate within minutes.

5. Once merged, future releases are submitted automatically by the
   `winget-submit` CI job (see `.github/workflows/release.yml`).

---

## Automated updates (v1.0.4+)

Add the secret **`WINGET_SUBMIT_TOKEN`** to the repository:

- Go to **Settings → Secrets and variables → Actions → New repository secret**
- Name: `WINGET_SUBMIT_TOKEN`
- Value: a GitHub Personal Access Token with **`public_repo`** scope

Every time you push a new version tag (e.g. `v1.0.4`), the release workflow
will build the Windows zip and automatically open a PR in `microsoft/winget-pkgs`
via `wingetcreate`.

---

## Updating manifests manually

```
iwr https://aka.ms/wingetcreate/latest -OutFile wingetcreate.exe
.\wingetcreate.exe update HichamBouchikhi.OpenMyTunnel `
  --version 1.0.4 `
  --urls https://github.com/hicham-bouchikhi/OpenMyTunnel/releases/download/v1.0.4/OpenMyTunnel-v1.0.4-win-x64.zip `
  --submit `
  --token <YOUR_PAT>
```
