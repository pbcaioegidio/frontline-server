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
          instalador Full: pack-player-setup.ps1 -Upload → Cloudflare R2
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
# Instalador Full oficial: Cloudflare R2 (downloads.frontlinebattle.com.br)
# /var/frontline/downloads/     # LEGADO — manter vazio (nao hospedar ZIP aqui)
```

Nginx: [`docs/nginx-frontlinebattle.conf`](nginx-frontlinebattle.conf) — site. Download do ZIP vem do R2.

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

## Instalador Full (Cloudflare R2)

O site (**BAIXAR**) aponta pro R2 — **não** use a VPS como host do ZIP.

```powershell
cd c:\Users\pbcai\Downloads\source
# secrets locais (nao commit): docs/r2-secrets.local.env
.\scripts\pack-player-setup.ps1 -Mode Full -Upload
# → gera dist\*.exe + bins, ZIP local, sobe FrontLine-Setup-latest.zip no R2
```

Ou só o upload se o pack já existir:

```powershell
.\scripts\upload-installer-r2.ps1 -Source .\dist\FrontLine-Setup-latest.zip
```

Atualize também `frontline-web/public/downloads-manifest.json` e faça deploy do site se a lista/tamanho mudar.

### Legado: GitHub Releases / VPS `/downloads`

**Depreciado.** Full não cabe no GitHub (~2 GB). VPS `/var/frontline/downloads` **não** é mais o caminho oficial (enche disco; site usa R2).

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

## Armadilhas que já quebraram o servidor

Três erros derrubaram o update de todos os jogadores em 07/09/2026. Se algo der errado no patch,
comece por aqui.

### `manifest.json` precisa ser UTF-8 **sem BOM**

O Socket usa Newtonsoft. Com BOM o launcher mostra:

```text
Erro durante a atualização.
Unexpected character encountered while parsing value: . Path '', line 0, position 0.
```

No Windows PowerShell 5.1, `Set-Content -Encoding UTF8` **grava BOM**. Use sempre:

```powershell
[IO.File]::WriteAllText($path, $json, (New-Object Text.UTF8Encoding $false))
```

Conferir na VPS: `od -c -N 4 .../Info/manifest.json` deve começar em `{`, não em `357 273 277`.

### O socket sobe com **dois** composes

O container `servidor-socket-1` roda em host network, criado com
`docker-compose.vps.yml` + `docker-compose.hostnet.yml`. Reiniciar só com o `vps.yml` pode tirá-lo
da rede host. Os três workflows usam o mesmo padrão:

```bash
C=(docker compose -f docker-compose.vps.yml)
[ -f docker-compose.hostnet.yml ] && C+=(-f docker-compose.hostnet.yml)
"${C[@]}" restart socket
```

### Nunca deixe `.bak` dentro de `client/`

O `FileListBuilder` varre a pasta inteira. Um `FrontLine.exe.bak-admin` esquecido entra na lista
assinada, e aí **todo jogador** recebe "arquivo ausente" do FL Guard. Backups vão para
`_client-backups/`, fora do client. O `IntegrityRules.ShouldSkip` também ignora qualquer `.bak`.

Pelo mesmo motivo, nunca faça hex-patch no `FrontLine.exe`: trocar `requireAdministrator` por
`asInvoker` tem tamanho diferente e corrompe o XML do manifesto embutido, gerando
"configuração lado a lado incorreta" (evento `SideBySide`, linha 14).

### Ordem correta ao republicar o client

```text
1. FileListBuilder no client (gera UserFileList.dat + .sig + ufl-md5.txt)
2. scp dos arquivos alterados → Socket/Data/Client
3. manifest.json com size/md5 de cada arquivo (sem BOM)
4. bump ClientVersion no Socket/Config/config.ini
5. restart do socket com os dois composes
```

Se a lista assinada da VPS estiver errada, o workflow `client-patch` propaga o erro — ele faz
*merge* na lista existente, não regera do client completo.

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
