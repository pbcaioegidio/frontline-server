# Deploy completo via GitHub Actions

Guia do que sobe onde, como o jogador atualiza, e como publicar instalador / launcher / client / servidor.

---

## Ideia em uma frase

O jogador **instala uma vez** (Inno no GitHub Releases). Depois só abre o `FLLauncher` → **Update** quando o Socket tiver versão nova. Nada de copiar `UserFileList`, `Shop.dat` ou DLL na mão.

```text
[1ª vez]  GitHub Releases → Instalador-FrontLine-*.exe → Program Files\FrontLine
[sempre]  FLLauncher ↔ Socket :9000 → Update (Data/Client) → FL Guard → Start → Auth/Game
[você]    git tag server-v* / launcher-v* / client-v* / installer-v* → Actions
```

---

## O que cada tag / workflow faz

| Tag | Workflow | Resultado |
|-----|----------|-----------|
| `server-v20260905` | Server deploy | Código `servidor/` na VPS + `docker compose up -d --build` |
| `launcher-v20260905` | Launcher release | `FLLauncher.exe` self-contained → `Socket/Data/Launcher` **e** `Data/Client` + bump `LauncherVersion` |
| `client-v20260905` | Client patch | Delta em `client-patch/` + FileList assinada → `Data/Client` + `manifest.json` + bump `ClientVersion` |
| `installer-v20260905` | Installer release | Gera Inno Slim/Full e publica **GitHub Release** (download fácil). Espelho opcional na VPS. |

Também dá para rodar cada um em **Actions → workflow → Run workflow** (sem tag).

Versões no Socket (`runtime/Socket/Config/config.ini`) são **números** (`long`). Prefira `YYYYMMDD` ou `YYYYMMDDNN` (ex.: `20260905`, `202609053`).

---

## Secrets (GitHub → Settings → Secrets and variables → Actions)

| Secret | Obrigatório | Uso |
|--------|-------------|-----|
| `VPS_HOST` | sim (server/launcher/client) | IP/host da VPS |
| `VPS_SSH_KEY` | sim | Chave privada SSH (ed25519) |
| `FILELIST_PRIVATE_PEM` | para `client-v*` | Conteúdo do `filelist-private.pem` (BEGIN…END) |
| `VPS_PATH` | não | Default `/opt/frontline/servidor` |
| `VPS_SSH_USER` | não | Default `ubuntu` |

A chave **pública** do FileList fica no launcher (`ManifestTrust.PublicPem`). A **privada nunca** vai no git.

O Installer release usa `GITHUB_TOKEN` (já incluso) para criar a Release — não precisa secret extra só pelo download no GitHub.

---

## Pastas na VPS (runtime)

```text
/opt/frontline/servidor/
  docker-compose.vps.yml
  docker-compose.hostnet.yml
  .env                          # só na VPS
  runtime/Socket/
    Config/config.ini           # LauncherVersion / ClientVersion
    Info/manifest.json          # lista do patch (Update)
    Data/Client/                # bytes do FILE_REQ
    Data/Launcher/              # cópia canônica do FLLauncher
  runtime/Config|Data|Logs/     # Auth / Game / Match
```

Espelho opcional do instalador: `/var/frontline/downloads/` (não é o caminho principal se usar GitHub Releases).

---

## Fluxo do jogador

1. Baixa o instalador na página de Releases do GitHub  
2. Instala (UAC administrador **só nesta instalação**) → `Program Files\FrontLine`  
3. Abre `FLLauncher.exe` (atalho)  
4. Socket `:9000` compara `LauncherVersion` / `ClientVersion` locais com o server  
5. Se local \< server → botão **Update** (baixa só o que mudou)  
6. FL Guard confere `UserFileList.dat` + `.sig`  
7. **Start** → jogo  

Quem **já instalou** não precisa baixar o instalador de novo só por causa de patch de launcher/client/server.

---

## Instalador (GitHub Releases — download fácil)

### Serve pra quê?

Só para a **primeira instalação** (ou reinstalação limpa). Não substitui Update do launcher.

### Posso apagar o `.exe` e gerar outro?

**Sim.** Em `dist\`, apague `Instalador-FrontLine-*.exe` antigo e gere de novo. O jogo já instalado no PC **não** some. Quem já joga continua no Update; o instalador novo é para novos jogadores (ou setup limpo).

### Como publicar (recomendado — na sua máquina)

A pasta `client/` é grande e **não** fica no git. Por isso o Actions `windows-latest` costuma falhar sem runner self-hosted. O caminho estável:

```powershell
# 1) (opcional) apagar instalador velho
Remove-Item .\dist\Instalador-FrontLine-*.exe -ErrorAction SilentlyContinue

# 2) gerar + publicar Release no GitHub
.\scripts\pack-player-setup.ps1 -Mode Slim -GitHubRelease
```

Requisitos: Inno Setup 6, `gh` logado (`gh auth login`), pasta `client\` com `FLLauncher.exe` + `FrontLine.exe`.

Slim = sem pasta `Pack` (menor). Full = client completo no setup (arquivo bem maior).

Link típico:

- Release da tag: `https://github.com/pbcaioegidio/frontline-server/releases/tag/installer-vYYYYMMDD`
- Última release: `https://github.com/pbcaioegidio/frontline-server/releases/latest`

### Via Actions (quando tiver `client/` no runner)

1. Tag: `git tag installer-v20260905` + `git push origin installer-v20260905`  
   **ou** Actions → **Installer release** → **Run workflow** (Slim/Full)  
2. O job gera o `.exe` e cria/atualiza a **GitHub Release** com o asset  
3. Espelho VPS (`/var/frontline/downloads`) só se os secrets VPS existirem  

Sem `client/` no runner: use o script local com `-GitHubRelease` (acima).

---

## Como publicar patch de client

1. Arquivos alterados em `client-patch/` (espelho do client).  
   Não coloque `UserFileList.dat` na mão — o workflow assina.  
2. Tag:

```bash
git tag client-v20260905
git push origin client-v20260905
```

3. Actions: merge/assinatura FileList → `Data/Client` → `manifest.json` → bump `ClientVersion` → restart socket.

---

## Como publicar servidor

```bash
git tag server-v20260905
git push origin server-v20260905
```

Sobe `servidor/` e roda compose com `docker-compose.vps.yml` + `docker-compose.hostnet.yml`.

---

## Como publicar launcher

```bash
git tag launcher-v20260905
git push origin launcher-v20260905
```

Sobe `FLLauncher.exe` para `Data/Launcher` e `Data/Client`, bump `LauncherVersion`.  
Se o hash do EXE mudar, regenere/assine `UserFileList` (ou rode um `client-v*` que inclua a lista) para o FL Guard não marcar o launcher como alterado.

---

## Checklist pós-deploy

- [ ] Containers healthy (`docker compose … ps`)  
- [ ] `config.ini` com versões novas  
- [ ] Launcher mostra Update se a versão local for menor  
- [ ] Update sem erro do FL Guard  
- [ ] Login + lobby OK  
- [ ] (se instalador) Release no GitHub com o `.exe` baixável  

---

## Smoke local (sem tag)

```powershell
.\scripts\validate-deploy-workflows.ps1
.\scripts\e2e-deploy-smoke.ps1
```

Assinatura FileList (merge): [`launcher/tools/SignFileList`](../launcher/tools/SignFileList).
