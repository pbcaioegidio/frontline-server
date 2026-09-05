# Deploy completo via GitHub Actions

O jogador **instala uma vez** (Inno) e depois só abre o `FLLauncher` → **Update** quando houver versão nova. Nada de copiar `UserFileList`, `Shop.dat` ou DLLs na mão.

## Tags

| Tag | Workflow | O que sobe |
|-----|----------|------------|
| `server-v20260905` | `server-deploy.yml` | Código `servidor/` → VPS + `docker compose up -d --build` |
| `launcher-v20260905` | `launcher-release.yml` | `FLLauncher` self-contained → `Socket/Data/Launcher` **e** `Data/Client` + bump `LauncherVersion` |
| `client-v20260905` | `client-patch.yml` | Delta em `client-patch/` + FileList assinada → `Socket/Data/Client` + `Info/manifest.json` + bump `ClientVersion` |
| `installer-v20260905` | `installer-release.yml` | Instalador Slim → `/var/frontline/downloads` (opcional; precisa client no runner) |

Também dá para rodar cada um por **Actions → workflow_dispatch** (sem tag).

Versões no Socket (`runtime/Socket/Config/config.ini`) são **números** comparáveis (`long`). Prefira `YYYYMMDD` ou `YYYYMMDDNN` (ex.: `20260905`).

## Secrets (GitHub → Settings → Secrets)

| Secret | Obrigatório | Uso |
|--------|-------------|-----|
| `VPS_HOST` | sim | IP/host da VPS (ex.: `132.226.74.48`) |
| `VPS_SSH_KEY` | sim | Chave privada SSH (ed25519), usuário `ubuntu` |
| `FILELIST_PRIVATE_PEM` | para `client-v*` | Conteúdo completo do `filelist-private.pem` (BEGIN…END) |
| `VPS_PATH` | não | Default `/opt/frontline/servidor` |
| `VPS_SSH_USER` | não | Default `ubuntu` |

A chave **pública** do FileList fica embutida no launcher (`ManifestTrust.PublicPem`). A privada **nunca** vai no git.

## Pastas na VPS

```text
/opt/frontline/servidor/
  docker-compose.vps.yml
  docker-compose.hostnet.yml
  .env                          # só na VPS
  runtime/Socket/
    Config/config.ini           # LauncherVersion / ClientVersion
    Info/manifest.json          # lista de arquivos do patch
    Data/Client/                # bytes baixados pelo Update (FILE_REQ)
    Data/Launcher/              # cópia canônica do FLLauncher
  runtime/Config|Data|Logs/     # Auth/Game/Match
```

## Fluxo do jogador

1. **1ª vez:** baixa o instalador Slim/Full → instala → abre `FLLauncher.exe`
2. Launcher fala com Socket `:9000` → compara versões
3. Se `ClientVersion` ou `LauncherVersion` local \< server → botão **Update**
4. Update baixa só o que mudou (`manifest.json` + `Data/Client`)
5. FL Guard valida `UserFileList.dat` + `.sig` (Shop.dat / EventPortal.dat **fora** da lista)
6. **Start** → jogo

## Como publicar um patch de client

1. Coloque os arquivos alterados em `client-patch/` espelhando o client  
   (ex.: `client-patch/UserFileList.dat` não — o workflow gera;  
   ex.: `client-patch/Config/foo.ini`, `client-patch/FLLauncher.exe` se precisar)
2. Commit + tag:

```bash
git tag client-v20260905
git push origin client-v20260905
```

3. O Actions:
   - assina FileList (merge com lista atual da VPS ou rebuild se `client/` existir no runner)
   - sobe arquivos para `Data/Client`
   - atualiza `Info/manifest.json`
   - bump `ClientVersion=20260905` + restart socket

## Como publicar servidor

```bash
git tag server-v20260905
git push origin server-v20260905
```

Actions envia `servidor/` (sem `bin`/`obj`) e roda:

```bash
docker compose -f docker-compose.vps.yml -f docker-compose.hostnet.yml --env-file .env up -d --build server
```

## Como publicar launcher

```bash
git tag launcher-v20260905
git push origin launcher-v20260905
```

## Checklist pós-deploy

- [ ] `docker compose … ps` — server/socket/db healthy  
- [ ] `config.ini` com versões novas  
- [ ] Launcher no PC mostra Update (versão local menor)  
- [ ] Update conclui sem erro do FL Guard  
- [ ] Login + lobby OK  

## Smoke local (sem tag)

```powershell
.\scripts\validate-deploy-workflows.ps1
.\scripts\e2e-deploy-smoke.ps1
```

Ferramenta de assinatura usada no merge: [`launcher/tools/SignFileList`](../launcher/tools/SignFileList).
