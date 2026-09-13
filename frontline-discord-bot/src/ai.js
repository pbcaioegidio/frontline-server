const { EmbedBuilder, SlashCommandBuilder } = require("discord.js");

const COLOR = parseInt(String(process.env.EMBED_COLOR || "#1E90FF").replace("#", ""), 16);
const GUILD_ID = process.env.GUILD_ID;
const COMANDOS_CH = process.env.CHANNEL_COMANDOS || null;
const CHANNEL_AI = process.env.CHANNEL_AI || null;

const GEMINI_API_KEY = process.env.GEMINI_API_KEY || "";
const GROQ_API_KEY = process.env.GROQ_API_KEY || "";
const OPENROUTER_API_KEY = process.env.OPENROUTER_API_KEY || "";

// Ordem: 1º melhor → fallback se falhar
const GEMINI_MODEL = process.env.AI_MODEL_GEMINI || "gemini-2.0-flash";
const GROQ_MODEL = process.env.AI_MODEL_GROQ || "openai/gpt-oss-20b";
const OPENROUTER_MODEL = process.env.AI_MODEL_OPENROUTER || "openrouter/free";

const COOLDOWN_MS = Math.max(0, Number(process.env.AI_COOLDOWN_MS || 8000));
const MAX_PROMPT = 1500;
const MAX_REPLY = 3500;

const SYSTEM_PROMPT =
  process.env.AI_SYSTEM_PROMPT ||
  [
    "Você é o assistente oficial da comunidade FrontLine | FPS.",
    "FrontLine é um SERVIDOR PRIVADO de Point Blank (client BR ~3.122), NÃO é um mod genérico nem pacote solto de arquivos.",
    "Site oficial: https://www.frontlinebattle.com.br — baixe o instalador em Downloads.",
    "Fluxo do jogador: instalar Full → abrir FLLauncher → Update → login → FL Guard → entrar no jogo.",
    "Conta: criar/vincular no Discord (canal de cadastro do servidor). Downloads e suporte também no Discord.",
    "NÃO invente: sites falsos (ex. frontline.com.br), mapas/modos inventados, instalação por extrair zip na pasta do PB oficial, Android/iOS, XP/skins inventados.",
    "Se não souber um detalhe (preço VIP, IP, regras específicas, status do servidor), diga que não sabe e oriente a olhar o Discord/site.",
    "Responda em português do Brasil, curto e completo (cabe num Discord embed). Evite tabelas longas e listas intermináveis.",
    "Nunca peça ou invente senha, token ou dados de ban.",
  ].join(" ");

/** @type {Map<string, number>} */
const lastUse = new Map();

const aiCommands = [
  new SlashCommandBuilder()
    .setName("ai")
    .setDescription("Pergunta à IA do FrontLine (Gemini → Groq → OpenRouter)")
    .addStringOption((o) =>
      o
        .setName("pergunta")
        .setDescription("O que você quer perguntar")
        .setRequired(true)
        .setMaxLength(MAX_PROMPT)
    ),
].map((c) => c.toJSON());

function emb(title, description, color = COLOR) {
  return new EmbedBuilder()
    .setColor(color)
    .setTitle(title)
    .setDescription(description)
    .setFooter({ text: "IA" })
    .setTimestamp();
}

function truncate(text, max) {
  const s = String(text || "").trim();
  if (s.length <= max) return s;
  return `${s.slice(0, max - 1)}…`;
}

function allowedChannel(channelId) {
  if (CHANNEL_AI) return channelId === CHANNEL_AI;
  if (COMANDOS_CH) return channelId === COMANDOS_CH;
  return true;
}

function checkCooldown(userId) {
  if (!COOLDOWN_MS) return null;
  const now = Date.now();
  const prev = lastUse.get(userId) || 0;
  const wait = COOLDOWN_MS - (now - prev);
  if (wait > 0) return Math.ceil(wait / 1000);
  lastUse.set(userId, now);
  return null;
}

async function chatCompletions({ baseUrl, apiKey, model, messages, extraHeaders = {} }) {
  const res = await fetch(`${baseUrl}/chat/completions`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${apiKey}`,
      "Content-Type": "application/json",
      ...extraHeaders,
    },
    body: JSON.stringify({
      model,
      messages,
      temperature: 0.35,
      max_tokens: 900,
    }),
  });

  const raw = await res.text();
  let data;
  try {
    data = JSON.parse(raw);
  } catch {
    data = null;
  }

  if (!res.ok) {
    const detail =
      data?.error?.message || data?.error || raw.slice(0, 240) || res.statusText;
    const err = new Error(`${res.status} ${detail}`);
    err.status = res.status;
    throw err;
  }

  const text = data?.choices?.[0]?.message?.content;
  if (!text) throw new Error("Resposta vazia do modelo");
  return { text: String(text).trim(), model: data.model || model };
}

/** Cadeia: 1 Gemini → 2 Groq → 3 OpenRouter */
async function askAi(prompt) {
  const messages = [
    { role: "system", content: SYSTEM_PROMPT },
    { role: "user", content: prompt },
  ];

  const errors = [];

  /** @type {{ name: string, run: () => Promise<{ text: string, model: string, provider: string }> }[]} */
  const chain = [];

  if (GEMINI_API_KEY) {
    chain.push({
      name: "gemini",
      run: async () => {
        const out = await chatCompletions({
          baseUrl: "https://generativelanguage.googleapis.com/v1beta/openai",
          apiKey: GEMINI_API_KEY,
          model: GEMINI_MODEL,
          messages,
        });
        return { ...out, provider: "gemini" };
      },
    });
  } else {
    errors.push("gemini: GEMINI_API_KEY ausente");
  }

  if (GROQ_API_KEY) {
    chain.push({
      name: "groq",
      run: async () => {
        const out = await chatCompletions({
          baseUrl: "https://api.groq.com/openai/v1",
          apiKey: GROQ_API_KEY,
          model: GROQ_MODEL,
          messages,
        });
        return { ...out, provider: "groq" };
      },
    });
  } else {
    errors.push("groq: GROQ_API_KEY ausente");
  }

  if (OPENROUTER_API_KEY) {
    chain.push({
      name: "openrouter",
      run: async () => {
        const out = await chatCompletions({
          baseUrl: "https://openrouter.ai/api/v1",
          apiKey: OPENROUTER_API_KEY,
          model: OPENROUTER_MODEL,
          messages,
          extraHeaders: {
            "HTTP-Referer": "https://frontlinebattle.com.br",
            "X-Title": "FrontLine Discord Bot",
          },
        });
        return { ...out, provider: "openrouter" };
      },
    });
  } else {
    errors.push("openrouter: OPENROUTER_API_KEY ausente");
  }

  for (const step of chain) {
    try {
      return await step.run();
    } catch (e) {
      errors.push(`${step.name}: ${e.message}`);
      console.warn(`[ai] ${step.name} falhou:`, e.message);
    }
  }

  throw new Error(errors.join(" · "));
}

/**
 * @param {import('discord.js').Client} client
 */
function setupAi(client) {
  const enabled = Boolean(GEMINI_API_KEY || GROQ_API_KEY || OPENROUTER_API_KEY);
  if (!enabled) {
    console.warn("[ai] desligado — defina GEMINI_API_KEY / GROQ_API_KEY / OPENROUTER_API_KEY");
  } else {
    console.log(
      `[ai] ativo · ordem: ` +
        `1)gemini=${GEMINI_API_KEY ? GEMINI_MODEL : "off"} → ` +
        `2)groq=${GROQ_API_KEY ? GROQ_MODEL : "off"} → ` +
        `3)openrouter=${OPENROUTER_API_KEY ? OPENROUTER_MODEL : "off"}`
    );
  }

  client.on("interactionCreate", async (interaction) => {
    if (!interaction.isChatInputCommand()) return;
    if (interaction.commandName !== "ai") return;
    if (GUILD_ID && interaction.guildId !== GUILD_ID) return;

    if (!allowedChannel(interaction.channelId)) {
      const hint = CHANNEL_AI || COMANDOS_CH;
      await interaction.reply({
        embeds: [
          emb(
            "Canal errado",
            hint ? `Use \`/ai\` em <#${hint}>.` : "Canal não permitido.",
            0xe67e22
          ),
        ],
        ephemeral: true,
      });
      return;
    }

    if (!enabled) {
      await interaction.reply({
        embeds: [
          emb(
            "IA offline",
            "Nenhuma chave configurada (`GEMINI_API_KEY` / `GROQ_API_KEY` / `OPENROUTER_API_KEY`).",
            0xe74c3c
          ),
        ],
        ephemeral: true,
      });
      return;
    }

    const waitSec = checkCooldown(interaction.user.id);
    if (waitSec != null) {
      await interaction.reply({
        embeds: [emb("Aguarde", `Cooldown: tente de novo em **${waitSec}s**.`, 0xe67e22)],
        ephemeral: true,
      });
      return;
    }

    const pergunta = interaction.options.getString("pergunta", true).trim();
    if (!pergunta) {
      await interaction.reply({
        embeds: [emb("Vazio", "Escreva uma pergunta.", 0xe67e22)],
        ephemeral: true,
      });
      return;
    }

    await interaction.deferReply();

    try {
      const { text, provider, model } = await askAi(pergunta);
      const body = [
        `**Pergunta**\n${truncate(pergunta, 400)}`,
        "",
        `**Resposta**\n${truncate(text, MAX_REPLY)}`,
      ].join("\n");

      console.log(`[ai] ok provider=${provider} model=${model}`);
      await interaction.editReply({
        embeds: [emb("Inteligência Artificial", body).setFooter({ text: `IA · ${provider}` })],
      });
    } catch (e) {
      console.warn("[ai] cmd:", e.message);
      await interaction.editReply({
        embeds: [
          emb(
            "Falha na IA",
            truncate(e.message || "erro desconhecido", 500),
            0xe74c3c
          ),
        ],
      });
    }
  });
}

module.exports = { setupAi, aiCommands };
