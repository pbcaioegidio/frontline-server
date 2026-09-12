const { EmbedBuilder } = require("discord.js");
const http = require("http");
const fs = require("fs");
const WebSocket = require("ws");
const { query, ping, listen } = require("./db");

const COLOR = parseInt(String(process.env.EMBED_COLOR || "#1E90FF").replace("#", ""), 16);
const CHANNEL_STATUS = process.env.CHANNEL_STATUS || "";
const STATUS_MESSAGE_ID = process.env.STATUS_MESSAGE_ID || "";
const DOCKER_SOCK = process.env.DOCKER_SOCK || "/var/run/docker.sock";
const SERVER_CONTAINER = process.env.SERVER_DOCKER_CONTAINER || "servidor-server-1";
const SOCKET_CONTAINER = process.env.SOCKET_DOCKER_CONTAINER || "servidor-socket-1";
const DB_CONTAINER = process.env.DB_DOCKER_CONTAINER || "servidor-db-1";
// Poll só para Docker/saúde; jogadores online vêm via WS StatusFeed + LISTEN (fallback).
const INTERVAL_MS = Math.max(15_000, Number(process.env.STATUS_INTERVAL_MS || 30_000));
const DATABASE_URL = process.env.DATABASE_URL || "";
const LISTEN_CHANNEL = "frontline_online";
const NOTIFY_COALESCE_MS = Math.max(500, Number(process.env.STATUS_NOTIFY_COALESCE_MS || 1500));
const STATUS_FEED_URL = (process.env.STATUS_FEED_URL || "").replace(/\/$/, "");
const STATUS_FEED_TOKEN = process.env.STATUS_FEED_TOKEN || "";

function snapshotKey(data) {
  return [
    data.server.ok,
    data.server.status,
    data.socket.ok,
    data.socket.status,
    data.db.ok,
    data.dbPing,
    data.onlinePlayers,
    data.totalAccounts,
  ].join("|");
}

function dockerInspectState(name) {
  return new Promise((resolve) => {
    if (!fs.existsSync(DOCKER_SOCK)) {
      resolve({ ok: false, status: "sem docker.sock" });
      return;
    }
    const req = http.request(
      {
        socketPath: DOCKER_SOCK,
        path: `/containers/${encodeURIComponent(name)}/json`,
        method: "GET",
        headers: { Host: "localhost" },
      },
      (res) => {
        let d = "";
        res.on("data", (c) => (d += c));
        res.on("end", () => {
          if (res.statusCode >= 400) {
            resolve({ ok: false, status: `erro ${res.statusCode}` });
            return;
          }
          try {
            const j = JSON.parse(d);
            const running = Boolean(j?.State?.Running);
            const health = j?.State?.Health?.Status || "";
            const status = running
              ? health
                ? `online (${health})`
                : "online"
              : j?.State?.Status || "offline";
            resolve({ ok: running, status });
          } catch {
            resolve({ ok: false, status: "parse" });
          }
        });
      }
    );
    req.on("error", () => resolve({ ok: false, status: "erro" }));
    req.setTimeout(4000, () => {
      req.destroy();
      resolve({ ok: false, status: "timeout" });
    });
    req.end();
  });
}

function lamp(ok) {
  return ok ? "🟢" : "🔴";
}

function buildEmbed({ server, socket, db, dbPing, onlinePlayers, totalAccounts }) {
  const allOk = server.ok && socket.ok && db.ok && dbPing;
  return new EmbedBuilder()
    .setColor(allOk ? COLOR : 0xe74c3c)
    .setTitle("📊 Status do servidor")
    .setDescription(
      allOk
        ? "Servidor **FrontLine** operacional."
        : "Servidor com **instabilidade** — equipe já pode estar verificando."
    )
    .addFields(
      {
        name: "Serviços",
        value: [
          `${lamp(server.ok)} Game / Auth — \`${server.status}\``,
          `${lamp(socket.ok)} Launcher (Socket) — \`${socket.status}\``,
          `${lamp(db.ok && dbPing)} Banco — \`${db.ok ? (dbPing ? "online" : "container ok, ping falhou") : db.status}\``,
        ].join("\n"),
        inline: false,
      },
      {
        name: "Jogadores",
        value: [
          `👥 Online agora: **${onlinePlayers}**`,
          totalAccounts != null ? `📇 Contas: **${totalAccounts}**` : null,
        ]
          .filter(Boolean)
          .join("\n"),
        inline: false,
      }
    )
    .setFooter({ text: "FrontLine | FPS · atualiza em tempo real" })
    .setTimestamp();
}

/**
 * @param {import('discord.js').Client} client
 */
function setupPublicStatus(client) {
  if (!CHANNEL_STATUS) {
    console.warn("[status] CHANNEL_STATUS ausente — desligado");
    return;
  }

  let messageId = STATUS_MESSAGE_ID || "";
  let ticking = false;
  let lastKey = "";
  /** @type {null | Awaited<ReturnType<typeof collect>>} */
  let lastData = null;
  let notifyTimer = null;
  let feedConnected = false;

  const collect = async () => {
    const [server, socket, db] = await Promise.all([
      dockerInspectState(SERVER_CONTAINER),
      dockerInspectState(SOCKET_CONTAINER),
      dockerInspectState(DB_CONTAINER),
    ]);

    let dbPing = false;
    let onlinePlayers = 0;
    let totalAccounts = null;
    if (DATABASE_URL) {
      try {
        await ping();
        dbPing = true;
        const r = await query(
          `SELECT
             COUNT(*) FILTER (WHERE online = true) AS online,
             COUNT(*) AS total
           FROM accounts`
        );
        onlinePlayers = Number(r.rows[0].online) || 0;
        totalAccounts = Number(r.rows[0].total) || 0;
      } catch (e) {
        dbPing = false;
        console.warn("[status] db:", e.message);
      }
    }

    return { server, socket, db, dbPing, onlinePlayers, totalAccounts };
  };

  const publish = async (data) => {
    const ch = await client.channels.fetch(CHANNEL_STATUS);
    if (!ch || !ch.isTextBased()) return;

    const key = snapshotKey(data);
    if (messageId && key === lastKey) return;

    const payload = { embeds: [buildEmbed(data)] };

    if (messageId) {
      try {
        const msg = await ch.messages.fetch(messageId);
        await msg.edit(payload);
        lastKey = key;
        lastData = data;
        return;
      } catch {
        messageId = "";
      }
    }

    const sent = await ch.send(payload);
    messageId = sent.id;
    lastKey = key;
    lastData = data;
    console.log(`[status] mensagem publicada id=${messageId} — salve STATUS_MESSAGE_ID=${messageId} no .env`);
  };

  const tick = async () => {
    if (ticking) return;
    ticking = true;
    try {
      const data = await collect();
      await publish(data);
    } catch (e) {
      console.warn("[status] tick:", e.message);
    } finally {
      ticking = false;
    }
  };

  /** Atualiza só a contagem online (Docker em cache do último poll). */
  const applyOnlineCount = async (count) => {
    if (typeof count !== "number" || Number.isNaN(count)) {
      await tick();
      return;
    }
    const base = lastData || (await collect());
    const data = { ...base, onlinePlayers: count, dbPing: true };
    await publish(data);
  };

  const scheduleOnline = (count, source) => {
    if (notifyTimer) clearTimeout(notifyTimer);
    notifyTimer = setTimeout(() => {
      console.log(`[status] ${source} online=${count}`);
      applyOnlineCount(count).catch((e) => console.warn(`[status] ${source}:`, e.message));
    }, NOTIFY_COALESCE_MS);
  };

  const onNotify = (payload) => {
    // Com StatusFeed UP, LISTEN fica como rede de segurança (Auth split / restart).
    let count = null;
    try {
      const j = JSON.parse(payload);
      if (typeof j.count === "number") count = j.count;
    } catch {
      /* payload opcional */
    }
    scheduleOnline(count, feedConnected ? "NOTIFY(fallback)" : "NOTIFY");
  };

  const startListen = () => {
    if (!DATABASE_URL) return;
    (async () => {
      for (;;) {
        try {
          console.log(`[status] LISTEN ${LISTEN_CHANNEL}`);
          const { ended, stop } = await listen(LISTEN_CHANNEL, onNotify);
          await ended;
          await stop().catch(() => null);
        } catch (e) {
          console.warn("[status] listen falhou:", e.message);
        }
        await new Promise((r) => setTimeout(r, 5000));
      }
    })().catch(() => {});
  };

  const startStatusFeed = () => {
    if (!STATUS_FEED_URL || !STATUS_FEED_TOKEN) {
      console.log("[status] StatusFeed WS desligado (STATUS_FEED_URL/TOKEN)");
      return;
    }

    let delay = 2000;
    const connect = () => {
      const sep = STATUS_FEED_URL.includes("?") ? "&" : "?";
      const url = `${STATUS_FEED_URL}${sep}token=${encodeURIComponent(STATUS_FEED_TOKEN)}`;
      let ws;
      try {
        ws = new WebSocket(url);
      } catch (e) {
        console.warn("[status] StatusFeed create:", e.message);
        setTimeout(connect, delay);
        delay = Math.min(60_000, delay * 2);
        return;
      }

      ws.on("open", () => {
        feedConnected = true;
        delay = 2000;
        console.log(`[status] StatusFeed conectado ${STATUS_FEED_URL}`);
      });

      ws.on("message", (raw) => {
        try {
          const j = JSON.parse(String(raw));
          if (typeof j.online === "number") {
            scheduleOnline(j.online, j.type || "feed");
          }
        } catch {
          /* ignore */
        }
      });

      ws.on("close", () => {
        feedConnected = false;
        console.warn("[status] StatusFeed fechou — reconecta");
        setTimeout(connect, delay);
        delay = Math.min(60_000, delay * 2);
      });

      ws.on("error", (e) => {
        console.warn("[status] StatusFeed erro:", e.message);
        try {
          ws.close();
        } catch {
          /* ignore */
        }
      });
    };

    connect();
  };

  const start = () => {
    tick().catch(() => {});
    setInterval(() => {
      tick().catch(() => {});
    }, INTERVAL_MS);
    startListen();
    startStatusFeed();
    console.log(
      `[status] público em ${CHANNEL_STATUS} · poll ${Math.round(INTERVAL_MS / 1000)}s · NOTIFY ${LISTEN_CHANNEL}` +
        (STATUS_FEED_URL ? ` · WS ${STATUS_FEED_URL}` : "")
    );
  };

  if (client.isReady()) start();
  else client.once("ready", start);

  client.on("messageCreate", async (message) => {
    try {
      if (message.channelId !== CHANNEL_STATUS) return;
      if (message.author.bot) return;
      await message.delete().catch(() => null);
    } catch {
      /* ignore */
    }
  });
}

module.exports = { setupPublicStatus };
