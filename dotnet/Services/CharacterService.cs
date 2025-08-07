using Microsoft.Data.Sqlite;

public class PlayerCharacterService : IDbService<PlayerCharacter, int>
{
    private readonly string _connStr;
    public PlayerCharacterService(string connStr) => _connStr = connStr;

    public void Insert(PlayerCharacter c)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = new SqliteCommand(@"
            INSERT INTO characters (
                user_id, name, stat_block_hash, stat_block, stat_block_message_id, stat_block_channel_id,
                spell_block_channel_id, spell_block_message_id, spell_block, spell_block_hash, mana,
                mana_readout_channel_id, mana_readout_message_id
            ) VALUES (
                @user_id, @name, @stat_block_hash, @stat_block, @stat_block_message_id, @stat_block_channel_id,
                @spell_block_channel_id, @spell_block_message_id, @spell_block, @spell_block_hash, @mana,
                @mana_readout_channel_id, @mana_readout_message_id
            )", conn);
        AddParams(cmd, c);
        cmd.ExecuteNonQuery();
    }

    public void Update(PlayerCharacter c)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = new SqliteCommand(@"
            UPDATE characters SET
                user_id=@user_id, name=@name, stat_block_hash=@stat_block_hash, stat_block=@stat_block,
                stat_block_message_id=@stat_block_message_id, stat_block_channel_id=@stat_block_channel_id,
                spell_block_channel_id=@spell_block_channel_id, spell_block_message_id=@spell_block_message_id,
                spell_block=@spell_block, spell_block_hash=@spell_block_hash, mana=@mana,
                mana_readout_channel_id=@mana_readout_channel_id, mana_readout_message_id=@mana_readout_message_id
            WHERE id=@id", conn);
        AddParams(cmd, c);
        cmd.Parameters.AddWithValue("@id", c.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = new SqliteCommand("DELETE FROM characters WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public PlayerCharacter? GetById(int id)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = new SqliteCommand("SELECT * FROM characters WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return null;
        return ReadPlayerCharacter(reader);
    }

    public IEnumerable<PlayerCharacter> GetByDiscordId(long discordId)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        using var cmd = new SqliteCommand("SELECT * FROM characters WHERE user_id=@id", conn);
        cmd.Parameters.AddWithValue("@id", discordId.ToString());
        using var reader = cmd.ExecuteReader();

        var result = new List<PlayerCharacter>();
        while (reader.Read())
            result.Add(ReadPlayerCharacter(reader));
        return result;
    }

    private void AddParams(SqliteCommand cmd, PlayerCharacter c)
    {
        cmd.Parameters.AddWithValue("@user_id", (object?)c.UserId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@name", (object?)c.Name ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stat_block_hash", (object?)c.StatBlockHash ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stat_block", (object?)c.StatBlock ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stat_block_message_id", (object?)c.StatBlockMessageId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stat_block_channel_id", (object?)c.StatBlockChannelId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spell_block_channel_id", (object?)c.SpellBlockChannelId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spell_block_message_id", (object?)c.SpellBlockMessageId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spell_block", (object?)c.SpellBlock ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spell_block_hash", (object?)c.SpellBlockHash ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@mana", (object?)c.Mana ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@mana_readout_channel_id", (object?)c.ManaReadoutChannelId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@mana_readout_message_id", (object?)c.ManaReadoutMessageId ?? DBNull.Value);
    }

    private PlayerCharacter ReadPlayerCharacter(SqliteDataReader r) => new PlayerCharacter
    {
        Id = r["id"] as long? is long l ? (int)l : null,
        UserId = r["user_id"] as string,
        Name = r["name"] as string,
        StatBlockHash = r["stat_block_hash"] as string,
        StatBlock = r["stat_block"] as string,
        StatBlockMessageId = r["stat_block_message_id"] as string,
        StatBlockChannelId = r["stat_block_channel_id"] as string,
        SpellBlockChannelId = r["spell_block_channel_id"] as string,
        SpellBlockMessageId = r["spell_block_message_id"] as string,
        SpellBlock = r["spell_block"] as string,
        SpellBlockHash = r["spell_block_hash"] as string,
        Mana = r["mana"] as long? is long ml ? (int)ml : null,
        ManaReadoutChannelId = r["mana_readout_channel_id"] as string,
        ManaReadoutMessageId = r["mana_readout_message_id"] as string,
    };
}
