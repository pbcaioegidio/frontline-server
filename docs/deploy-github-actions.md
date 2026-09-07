# Deploy completo via GitHub Actions

Guia do que sobe onde, como o jogador atualiza, e como publicar instalador / launcher / client / servidor / site.

---

## Ideia em uma frase

O jogador **instala uma vez** (Full na VPS / site). Depois só abre o `FLLauncher` → **Update** quando o Socket tiver versão nova.

```text
[1ª vez]  frontlinebattle.com.br/downloads → Instalador Full (.exe + .bin) → Program Files\FrontLine
[sempre]  FLLauncher ↔ Socket :9000 → Update (Data/Client) → FL Guard → Start → Auth/Game
[você]    git tag server-v* / launcher-v* / client-v* → Actions (este repo)
          site: repo frontline-web + tag site-v* → Actions
          instalador Full: pack-player-setup.ps1 -Upload → /var/frontline/downloads/
```

---

## O que cada tag / workflow faz

| Tag / repo | Workflow | Resultado |
|------------|----------|-----------|
| `server-v202609.1.0` (este repo) | Server deploy | Código `servidor/` na VPS + `docker compose up -d --build` |
| `launcher-v202609.1.0` | Launcher release | `FLLauncher.exe` → Socket Data + bump `LauncherVersion` |
| `client-v202609.1.0` | Client patch | Delta `client-patch/` + FileList → bump `ClientVersion` |
| `site-v202609.1.0` ([frontline-web](https://github.com/pbcaioegidio/frontline-web)) | Site deploy | Build Vite → `/var/www/frontlinebattle` |

Instalador **não** roda no Actions (pasta `client/` fora do git). Full sobe com `-Upload` na VPS.

Também dá para rodar cada workflow em **Actions → Run workflow** (sem tag).

### Versionamento das tags (ano-mês + feature)

Formato: **`vYYYYMM.FEATURE.FIX`** (ex.: `launcher-v202609.1.0`).

| Situação | O que fazer | Exemplo |
|----------|-------------|---------|
| Nova feature / lançamento do mês | sobe o **FEATURE**, FIX = `0` | `v202609.1.0` |
| Deu problema → correção | sobe só o **FIX** | `v202609.1.1` |
| Outra feature no mesmo mês | sobe o **FEATURE**, FIX volta a `0` | `v202609.2.0` |
| Mês seguinte | novo `YYYYMM`, feature `1.0` | `v202610.1.0` |

Mesma regra para `launcher-v*`, `client-v*`, `server-v*` e `site-v*`.

No Socket (`config.ini`) a versão vira **só dígitos** (`long`):

| Tag | Número no Socket |
|-----|------------------|
| `…-v202609.1.0` | `2026090100` |
| `…-v202609.1.1` | `2026090101` |
| `…-v202609.2.0` | `2026090200` |

**Regra crítica:** a tag nova tem que gerar número **maior** que a versão já na VPS.

---

## Secrets (GitHub → Settings → Secrets and variables → Actions)

### Este repo (`frontline-server`)

| Secret | Obrigatório | Uso |
|--------|-------------|-----|
| `VPS_HOST` | sim | IP/host da VPS |
| `VPS_SSH_KEY` | sim | Chave privada SSH (ed25519) |
| `FILELIST_PRIVATE_PEM` | para `client-v*` | Conteúdo do `filelist-private.pem` |
| `VPS_PATH` | não | Default `/opt/frontline/servidor` |
| `VPS_SSH_USER` | não | Default `ubuntu` |

### Repo `frontline-web`

| Secret | Uso |
|--------|-----|
| `VPS_HOST` / `VPS_SSH_KEY` / `VPS_SSH_USER` | Deploy do site (mesmos valores) |

A chave **pública** do FileList fica no launcher. A **privada nunca** vai no git.

---

## Pastas na VPS (runtime)

```text
/opt/frontline/servidor/          # game server + socket
/var/www/frontlinebattle/         # site (Actions frontline-web)
/var/frontline/downloads/         # instalador Full (.exe + .bin)
  FrontLine-Setup-latest.exe      # symlink → versão atual
  FrontLine-Setup-latest-N.bin    # symlinks das fatias (mesmo basename do .exe)
```

Nginx: [`docs/nginx-frontlinebattle.conf`](nginx-frontlinebattle.conf) — site + `location /downloads/`.

Domínio DNS (KingHost): `A` `@` e `www` → IP da VPS. **Apague o AAAA** do `@` se ainda apontar IPv6 da King (senão Let’s Encrypt / visitantes IPv6 vão para o lugar errado).  
Firewall Oracle Cloud: liberar **TCP 80 e 443** no Security List / NSG (iptables na VM já aceita; 443 no cloud pode estar fechado).

Site público: `http://www.frontlinebattle.com.br` (HTTPS `www` com cert Let’s Encrypt quando a porta 443 estiver aberta).

---

## Fluxo do jogador

1. Baixa o instalador em [frontlinebattle.com.br](http://www.frontlinebattle.com.br) (`.exe` + todos os `.bin` na mesma pasta)  
2. Instala (UAC administrador **só nesta instalação**) → `Program Files\FrontLine`  
3. Abre `FLLauncher.exe`  
4. Socket `:9000` compara versões → **Update** se precisar  
5. FL Guard → **Start** → jogo  

---

## Instalador Full (VPS)

```powershell
cd c:\Users\pbcai\Downloads\source
$env:FL_VPS_SSH = "ubuntu@SEU_IP"   # nunca commitar IP
$env:FL_DOWNLOAD_BASE = "https://www.frontlinebattle.com.br"

.\scripts\pack-player-setup.ps1 -Mode Full -Upload
```

Atualize também `frontline-web/public/downloads-manifest.json` e faça deploy do site se a lista de arquivos mudar.

### Legado: GitHub Releases (`frontline-downloads`)

**Depreciado.** Full não cabe no limite ~2 GB do GitHub. O script [`publish-installer-release.ps1`](../scripts/publish-installer-release.ps1) e `-GitHubRelease` existem só por compatibilidade Slim/teste — o caminho oficial é `-Upload` na VPS. Pode apagar o repo `frontline-downloads`.

---

## Discord bot

Código em [`discord-bot/`](../discord-bot/) **neste** monorepo (não no `frontline-web`). Deploy típico: Docker na VPS / processo Node com `.env` (token, canais, `DATABASE_URL`).

Canal de downloads: botão aponta para o instalador no site (`FrontLine-Setup-latest.exe`). Atualizar embed:

```powershell
cd discord-bot
node scripts/update-download.js
```

(requer `DISCORD_TOKEN` e message id no script / env).

---

## Como publicar patch de client

1. Arquivos em `client-patch/`  
2. Tag: `git tag client-v202609.1.0 && git push origin client-v202609.1.0`

---

## Como publicar servidor

```bash
git tag server-v202609.1.0
git push origin server-v202609.1.0
```

---

## Como publicar launcher

```bash
git tag launcher-v202609.1.0
git push origin launcher-v202609.1.0
```

---

## Como publicar o site

No repo [frontline-web](https://github.com/pbcaioegidio/frontline-web):

```bash
git tag site-v202609.1.0
git push origin site-v202609.1.0
```

Ou **Actions → Site deploy → Run workflow**.

---

## Checklist pós-deploy

- [ ] Containers healthy  
- [ ] `config.ini` com versões novas  
- [ ] Launcher Update + FL Guard OK  
- [ ] Site `www.frontlinebattle.com.br` carrega  
- [ ] `/downloads/FrontLine-Setup-latest.exe` + `.bin` baixam  
- [ ] (HTTPS) Security List com TCP 443 + AAAA King removido  

---

## Smoke local (sem tag)

```powershell
.\scripts\validate-deploy-workflows.ps1
.\scripts\e2e-deploy-smoke.ps1
```

Assinatura FileList: [`launcher/tools/SignFileList`](../launcher/tools/SignFileList).
