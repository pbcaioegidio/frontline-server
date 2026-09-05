using Npgsql;
var cs = "Host=127.0.0.1;Port=5433;Database=pb;Username=postgres;Password=postgres";
await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();
const string token = "AAECAwQFBgcICQoLDA0OMg==";
await using (var cmd = new NpgsqlCommand(@"
INSERT INTO accounts (username, password, token)
VALUES ('player1', 'player1', @t)
ON CONFLICT (username) DO UPDATE
SET password = EXCLUDED.password,
    token = EXCLUDED.token;", conn))
{
  cmd.Parameters.AddWithValue("t", token);
  try {
    var n = await cmd.ExecuteNonQueryAsync();
    Console.WriteLine($"UPSERT ok rows={n}");
  } catch (Exception ex) {
    Console.WriteLine("UPSERT fail: " + ex.Message);
    // fallback sem ON CONFLICT
    await using var sel = new NpgsqlCommand("SELECT COUNT(*) FROM accounts WHERE username='player1'", conn);
    var c = (long)(await sel.ExecuteScalarAsync() ?? 0L);
    if (c == 0) {
      await using var ins = new NpgsqlCommand("INSERT INTO accounts (username, password, token) VALUES ('player1','player1',@t)", conn);
      ins.Parameters.AddWithValue("t", token);
      await ins.ExecuteNonQueryAsync();
      Console.WriteLine("INSERT ok");
    } else {
      await using var upd = new NpgsqlCommand("UPDATE accounts SET password='player1', token=@t WHERE username='player1'", conn);
      upd.Parameters.AddWithValue("t", token);
      await upd.ExecuteNonQueryAsync();
      Console.WriteLine("UPDATE ok");
    }
  }
}
await using (var check = new NpgsqlCommand("SELECT username, length(password) plen, length(coalesce(token,'')) tlen FROM accounts WHERE username='player1'", conn))
await using (var r = await check.ExecuteReaderAsync()) {
  if (await r.ReadAsync())
    Console.WriteLine($"OK user={r.GetString(0)} passLen={r.GetInt32(1)} tokenLen={r.GetInt32(2)}");
  else
    Console.WriteLine("MISSING player1");
}
