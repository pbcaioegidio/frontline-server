const { Pool } = require("pg");

let pool = null;

function getPool() {
  if (pool) return pool;
  const connectionString = process.env.DATABASE_URL;
  if (!connectionString) {
    throw new Error("Falta DATABASE_URL no .env");
  }
  pool = new Pool({
    connectionString,
    max: 5,
    idleTimeoutMillis: 30_000,
  });
  pool.on("error", (err) => console.error("[db] pool error:", err.message));
  return pool;
}

async function query(text, params) {
  return getPool().query(text, params);
}

async function ping() {
  const r = await query("SELECT 1 AS ok");
  return r.rows[0]?.ok === 1;
}

/**
 * Conexão dedicada para LISTEN (não usar o pool — fica ocupada).
 * @param {string} channel
 * @param {(payload: string) => void} onNotify
 * @returns {Promise<() => Promise<void>>} stop
 */
/**
 * Conexão dedicada LISTEN. Resolve quando a conexão acaba (para o caller reconectar).
 * @param {string} channel nome SQL seguro (identificador simples)
 * @param {(payload: string) => void} onNotify
 * @returns {Promise<{ stop: () => Promise<void>, ended: Promise<void> }>}
 */
async function listen(channel, onNotify) {
  if (!/^[a-z_][a-z0-9_]*$/i.test(channel)) {
    throw new Error(`canal LISTEN inválido: ${channel}`);
  }
  const connectionString = process.env.DATABASE_URL;
  if (!connectionString) throw new Error("Falta DATABASE_URL no .env");

  const { Client } = require("pg");
  const client = new Client({ connectionString });
  await client.connect();
  await client.query(`LISTEN ${channel}`);

  client.on("notification", (msg) => {
    if (msg.channel !== channel) return;
    try {
      onNotify(msg.payload || "");
    } catch (e) {
      console.warn("[db] listen handler:", e.message);
    }
  });

  const ended = new Promise((resolve) => {
    client.on("error", (err) => {
      console.error("[db] listen error:", err.message);
      resolve();
    });
    client.on("end", () => {
      console.warn("[db] listen connection ended");
      resolve();
    });
  });

  return {
    ended,
    stop: async () => {
      try {
        await client.query(`UNLISTEN ${channel}`);
      } catch {
        /* ignore */
      }
      await client.end().catch(() => null);
    },
  };
}

module.exports = { query, ping, getPool, listen };
