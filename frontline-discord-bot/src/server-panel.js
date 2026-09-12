const { EmbedBuilder } = require("discord.js");
const http = require("http");
const fs = require("fs");
const { query } = require("./db");

const COLOR = parseInt(String(process.env.EMBED_COLOR || "#1E90FF").replace("#", ""), 16);
const GUILD_ID = process.env.GUILD_ID || "";
const CHANNEL_LOGS = process.env.CHANNEL_LOGS || "";
const DOCKER_SOCK = process.env.DOCKER_SOCK || "/var/run/docker.sock";
const SERVER_CONTAINER = process.env.SERVER_DOCKER_CONTAINER || "servidor-server-1";
const SOCKET_CONTAINER = process.env.SOCKET_DOCKER_CONTAINER || "servidor-socket-1";
// Batch curto: Discord rate-limit; linhas críticas flush imediato.
const FLUSH_MS = Math.max(400, Number(process.env.LOGS_FLUSH_MS || 800));
const DB_POLL_MS = Math.max(1000, Number(process.env.LOGS_DB_POLL_MS || 2000));
const MAX_CHUNK = 1800;
const URGENT_LINE = /\b(Error|Exception|kick|ban|FL GUARD|FG-\d+|HardBan|FAILED|falhou)\b/i;
const ANNOUNCE = process.env.PANEL_ANNOUNCE === "1";
const DATABASE_URL = process.env.DATABASE_URL || "";

/** Docker: tudo útil (economia, shop, passe, combate, segurança) — ainda corta flood de ACK */
const INTERESTING = [
  /\bAccount User\b/i,
  /\bDisconnected\b|\bConnected\b/i,
  /\bLOGOUT\b|\bLOGIN\b/,
  /\bFL GUARD\b/i,
  /\bFG-\d+/i,
  /\bAntiAim\b|\baimbot\b/i,
  /\bHwIdBan\b/i,
  /\bACCOUNT_KICK\b/i,
  /\bkick(ed|ing)?\b/i,
  /\bban(ned|ning)?\b/i,
  /\bun-?mute\b|\bunban\b/i,
  /\bHardBan\b|\bApplyHardBan\b/i,
  /\bPermanently banned\b/i,
  /\bBan successful\b|\bMute successful\b/i,
  /\bGMCHAT_APPLY_PENALTY\b/i,
  /\bHeartbeat\b/i,
  /\bError\b|\bException\b|\bfalhou\b|\bFAILED\b/i,
  /\bDaily database\b/i,
  /\[DIAG\]/i,
  /\bRESULTADO\b/i,
  /\bGold\b|\bCash\b|\bgold=|\bcash=/i,
  /\bShop\b|\bBUY_|\bGoods\b|\bgift\b|\bcoupon\b/i,
  /\bSeason\b|\bBattlepass\b|\bBUY_SEASON\b|\bPASS\b|\bChallenge\b/i,
  /\bCREATECHARA\b|\bINSUFFICIENT\b/i,
  /\brefund\b|\btopup\b|\bWebcoin\b|\bVIP Balance\b/i,
  /\bMatch\b|\broom\b|\bsala\b/i,
  /\bhigh latency\b/i,
  /\bstarted\b|\bstopped\b|\blistening\b/i,
  /\bAuth\b.*\bready\b|\bGame\b.*\bready\b/i,
  /\bRCON\b|\bLogsPanel\b/i,
  /\bShutting down\b|\bshutdown complete\b|\bonline\b/i,
  /\bPlugin carregado\b|\bEndereço\b/i,
];

/** Traduções comuns dos logs C# / Docker → PT-BR (ordem: frases longas primeiro). */
const PT_REPLACEMENTS = [
  [/Shutting down Match Manager\.\.\./gi, "Encerrando gerenciador de Match..."],
  [/Match Manager shutdown complete/gi, "Gerenciador de Match encerrado"],
  [/Shutting down Auth Manager\.\.\./gi, "Encerrando gerenciador de Auth..."],
  [/Auth Manager shutdown complete/gi, "Gerenciador de Auth encerrado"],
  [/Shutting down Game Manager\.\.\./gi, "Encerrando gerenciador de Game..."],
  [/Game Manager shutdown complete/gi, "Gerenciador de Game encerrado"],
  [/Accept Callback Date:\s*/gi, "Callback de aceite em: "],
  [/Exception:\s*Operation canceled/gi, "Exceção: operação cancelada"],
  [/Operation canceled/gi, "operação cancelada"],
  [/FrontLine server online/gi, "Servidor FrontLine online"],
  [/Plugin carregado:/gi, "Plugin carregado:"],
  [/\bAccount User:\s*/gi, "Conta: "],
  [/\bPlayer UID:\s*/gi, "UID: "],
  [/\bIs Connected\b/gi, "conectado"],
  [/\bIs Disconnected\b/gi, "desconectado"],
  [/\bDisconnected\b/gi, "desconectado"],
  [/\bConnected\b/gi, "conectado"],
  [/\bPermanently banned\b/gi, "banido permanente"],
  [/\bBan successful\b/gi, "ban aplicado"],
  [/\bMute successful\b/gi, "mute aplicado"],
  [/\bhigh latency\b/gi, "latência alta"],
  [/\blistening\b/gi, "escutando"],
  [/\bstarted\b/gi, "iniciado"],
  [/\bstopped\b/gi, "parado"],
  [/\bFAILED\b/g, "FALHOU"],
  [/\bError\b/g, "Erro"],
  [/\bException\b/g, "Exceção"],
  [/\bsystem\b/g, "sistema"],
  [/\bserver\b(?=\])/gi, "servidor"],
  [/\bsocket\b(?=\])/gi, "socket"],
];

function interesting(line) {
  const s = String(line || "").trim();
  if (!s) return false;
  // flood de pacotes de protocolo
  if (
    /\b(GET_|CONNECT_ACK|OPTION_SAVE|ITEMGROUP|POINT_CASH|INVEN_INFO|CHARA_INFO|USER_INFO|CHALLENGE_INFO|CHALLENGE_SEASON|SEASON_ACK|MESSAGE_USER|DAILY_RECORD|KEEP_ALIVE)\b/.test(
      s
    ) &&
    !/Error|fail|DIAG|kick|ban|Gold|Cash|RESULTADO/i.test(s)
  ) {
    return false;
  }
  return INTERESTING.some((re) => re.test(s));
}

function toPt(line) {
  let s = String(line || "");
  for (const [re, pt] of PT_REPLACEMENTS) {
    s = s.replace(re, pt);
  }
  return s;
}

function emb(title, description, color = COLOR) {
  return new EmbedBuilder()
    .setColor(color)
    .setTitle(title)
    .setDescription(description)
    .setFooter({ text: "Painel FrontLine" })
    .setTimestamp();
}

function dockerRequest(path, { follow = false } = {}) {
  return new Promise((resolve, reject) => {
    const req = http.request(
      {
        socketPath: DOCKER_SOCK,
        path,
        method: "GET",
        headers: { Host: "localhost" },
      },
      (res) => {
        if (res.statusCode && res.statusCode >= 400) {
          let d = "";
          res.on("data", (c) => (d += c));
          res.on("end", () => reject(new Error(`docker ${res.statusCode}: ${d.slice(0, 200)}`)));
          return;
        }
        if (follow) resolve(res);
        else {
          let d = "";
          res.on("data", (c) => (d += c));
          res.on("end", () => resolve(d));
        }
      }
    );
    req.on("error", reject);
    req.end();
  });
}

function createDockerLogDemux(onLine) {
  let buf = Buffer.alloc(0);
  let carry = "";
  return (chunk) => {
    buf = Buffer.concat([buf, chunk]);
    while (buf.length >= 8) {
      const size = buf.readUInt32BE(4);
      if (buf.length < 8 + size) break;
      const payload = buf.subarray(8, 8 + size).toString("utf8");
      buf = buf.subarray(8 + size);
      const mixed = carry + payload;
      const parts = mixed.split(/\r?\n/);
      carry = parts.pop() || "";
      for (const line of parts) {
        const t = line.replace(/\x1b\[[0-9;]*m/g, "").trim();
        if (t) onLine(t);
      }
    }
  };
}

/**
 * @param {import('discord.js').Client} client
 */
function setupServerPanel(client) {
  if (!CHANNEL_LOGS) {
    console.warn("[painel] CHANNEL_LOGS ausente — desligado");
    return;
  }

  /** @type {string[]} */
  let queue = [];
  /** @type {Set<string>} */
  const seen = new Set();
  let flushing = false;
  let started = false;
  /** @type {import('discord.js').TextChannel | null} */
  let channel = null;

  let lastShopId = 0;
  let lastSecId = 0;
  let lastLoginId = 0;

  let flushSoonTimer = null;

  const flush = async () => {
    if (flushing || !queue.length || !channel) return;
    flushing = true;
    try {
      const batch = queue.splice(0, 20);
      let body = batch.join("\n");
      if (body.length > MAX_CHUNK) body = body.slice(-MAX_CHUNK);
      await channel.send({
        embeds: [emb("📋 Logs do servidor", "```\n" + body + "\n```")],
      });
    } catch (e) {
      console.warn("[painel] send:", e.message);
    } finally {
      flushing = false;
      if (queue.length) scheduleFlush(false);
    }
  };

  const scheduleFlush = (urgent) => {
    if (urgent) {
      if (flushSoonTimer) {
        clearTimeout(flushSoonTimer);
        flushSoonTimer = null;
      }
      flush().catch(() => {});
      return;
    }
    if (flushSoonTimer) return;
    flushSoonTimer = setTimeout(() => {
      flushSoonTimer = null;
      flush().catch(() => {});
    }, FLUSH_MS);
  };

  const push = (text, urgent = false) => {
    const line = toPt(String(text || "").trim()).slice(0, 420);
    if (!line) return;
    if (seen.has(line)) return;
    seen.add(line);
    if (seen.size > 400) {
      const first = seen.values().next().value;
      seen.delete(first);
    }
    queue.push(line);
    if (queue.length > 100) queue = queue.slice(-100);
    scheduleFlush(urgent || URGENT_LINE.test(line));
  };

  const pushDocker = (source, line) => {
    if (!interesting(line)) return;
    const label = source === "server" ? "servidor" : source;
    push(`[${label}] ${line}`, URGENT_LINE.test(line));
  };

  setInterval(() => {
    flush().catch(() => {});
  }, Math.max(FLUSH_MS * 2, 1500));

  const followContainer = async (name, label) => {
    if (!fs.existsSync(DOCKER_SOCK)) return;
    const since = Math.floor(Date.now() / 1000);
    const path =
      `/containers/${encodeURIComponent(name)}/logs` +
      `?stdout=1&stderr=1&follow=1&tail=0&since=${since}`;
    const stream = await dockerRequest(path, { follow: true });
    const demux = createDockerLogDemux((line) => pushDocker(label, line));
    stream.on("data", demux);
    stream.on("error", (e) => console.warn(`[painel] stream ${name}:`, e.message));
    stream.on("end", () => {
      console.warn(`[painel] stream ${name} acabou — reconecta em 5s`);
      setTimeout(() => followContainer(name, label).catch((err) => console.warn(err.message)), 5000);
    });
    console.log(`[painel] seguindo logs de ${name} (since=${since})`);
  };

  const initCursors = async () => {
    if (!DATABASE_URL) return;
    try {
      const r = await query(`
        SELECT
          (SELECT COALESCE(MAX(id),0) FROM shop_audit) AS shop,
          (SELECT COALESCE(MAX(id),0) FROM security_events) AS sec,
          (SELECT COALESCE(MAX(id),0) FROM login_audit) AS login
      `);
      lastShopId = Number(r.rows[0].shop) || 0;
      lastSecId = Number(r.rows[0].sec) || 0;
      lastLoginId = Number(r.rows[0].login) || 0;
      console.log(`[painel] db cursors shop=${lastShopId} sec=${lastSecId} login=${lastLoginId}`);
    } catch (e) {
      console.warn("[painel] db cursor:", e.message);
    }
  };

  const pollDb = async () => {
    if (!DATABASE_URL || !channel) return;
    try {
      const shop = await query(
        `SELECT id, ts, player_id, op, good_id, item_id, currency, price, balance_before, balance_after, target_player
         FROM shop_audit WHERE id > $1 ORDER BY id ASC LIMIT 30`,
        [lastShopId]
      );
      for (const row of shop.rows) {
        lastShopId = Math.max(lastShopId, Number(row.id));
        const delta = Number(row.balance_after) - Number(row.balance_before);
        const sign = delta >= 0 ? `+${delta}` : `${delta}`;
        push(
          `[loja] #${row.player_id} ${row.op} ${row.currency} preço=${row.price} ` +
            `${row.balance_before}→${row.balance_after} (${sign}) ` +
            `item=${row.item_id} good=${row.good_id}` +
            (row.target_player > 0 ? ` →#${row.target_player}` : "")
        );
      }

      const sec = await query(
        `SELECT id, ts, player_id, username, nickname, action, source, category, reason, severity, client_code, auto, gm_id, ban_id
         FROM security_events WHERE id > $1 ORDER BY id ASC LIMIT 40`,
        [lastSecId]
      );
      for (const row of sec.rows) {
        lastSecId = Math.max(lastSecId, Number(row.id));
        const who = row.nickname || row.username || `#${row.player_id}`;
        push(
          `[segurança] ${row.action}/${row.category} ${who} ` +
            `origem=${row.source} sev=${row.severity}` +
            (row.client_code ? ` ${row.client_code}` : "") +
            (row.auto ? " auto" : "") +
            (row.ban_id > 0 ? ` ban#${row.ban_id}` : "") +
            (row.gm_id > 0 ? ` gm#${row.gm_id}` : "") +
            (row.reason ? ` — ${String(row.reason).slice(0, 120)}` : "")
        );
      }

      const login = await query(
        `SELECT id, ts, source, username, player_id, result, ip, reason
         FROM login_audit WHERE id > $1 ORDER BY id ASC LIMIT 40`,
        [lastLoginId]
      );
      for (const row of login.rows) {
        lastLoginId = Math.max(lastLoginId, Number(row.id));
        push(
          `[login] ${row.result} ${row.username || `#${row.player_id}`} ` +
            `via=${row.source} ip=${row.ip}` +
            (row.reason ? ` — ${String(row.reason).slice(0, 100)}` : "")
        );
      }
    } catch (e) {
      console.warn("[painel] db poll:", e.message);
    }
  };

  const start = async () => {
    if (started) return;
    started = true;
    try {
      const ch = await client.channels.fetch(CHANNEL_LOGS);
      if (!ch || !ch.isTextBased()) {
        console.warn("[painel] CHANNEL_LOGS inválido");
        return;
      }
      channel = ch;
      await initCursors();

      if (ANNOUNCE) {
        await channel.send({
          embeds: [
            emb(
              "🖥️ Painel FrontLine",
              [
                "Controle do servidor — logs + auditoria.",
                "Fontes: Docker (jogo/socket) + Postgres (`shop_audit`, `security_events`, `login_audit`).",
                "Economia, loja, passe, kick/ban, aimbot, login…",
              ].join("\n")
            ),
          ],
        });
      }

      if (fs.existsSync(DOCKER_SOCK)) {
        await followContainer(SERVER_CONTAINER, "server");
        await followContainer(SOCKET_CONTAINER, "socket").catch((e) =>
          console.warn("[painel] socket:", e.message)
        );
      } else {
        console.warn("[painel] sem docker.sock — só Postgres");
      }

      if (DATABASE_URL) {
        setInterval(() => {
          pollDb().catch(() => {});
        }, DB_POLL_MS);
        console.log("[painel] poll Postgres ativo");
      } else {
        console.warn("[painel] sem DATABASE_URL — sem shop/security/login audit");
      }
    } catch (e) {
      console.warn("[painel] start:", e.message);
      started = false;
    }
  };

  if (client.isReady()) start();
  else client.once("ready", () => {
    start();
  });

  client.on("messageCreate", async (message) => {
    try {
      if (message.channelId !== CHANNEL_LOGS) return;
      if (message.author.bot) return;
      if (GUILD_ID && message.guildId !== GUILD_ID) return;
      await message.delete().catch(() => null);
    } catch {
      /* ignore */
    }
  });
}

module.exports = { setupServerPanel };
