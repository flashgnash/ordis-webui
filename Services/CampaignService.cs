using Microsoft.EntityFrameworkCore;

public class CampaignService(IDbContextFactory<OrdisContext> dbFactory, LiveUpdateService liveUpdates)
{

    private async Task AssertIsDmAsync(OrdisContext db, int campaignId, string callerDiscordId)
    {
        if (string.IsNullOrEmpty(callerDiscordId))
            throw new UnauthorizedAccessException("Missing caller identity.");

        var dmId = await db.Campaigns
            .Where(c => c.Id == campaignId)
            .Select(c => c.DungeonMasterId)
            .SingleOrDefaultAsync();

        if (dmId == null)
            throw new InvalidOperationException($"Campaign {campaignId} not found.");

        if (dmId != callerDiscordId)
            throw new UnauthorizedAccessException($"User {callerDiscordId} is not the DM of campaign {campaignId}.");
    }

    public async Task<PlayerCharacter> AddPlayerToCampaignAsync(int campaignId, string callerDiscordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await AssertIsDmAsync(db, campaignId, callerDiscordId);
        var campaign = await db.Campaigns.Include(c => c.Players).FirstOrDefaultAsync(c => c.Id == campaignId);
        if (campaign == null) throw new InvalidOperationException($"Campaign {campaignId} not found");
        var player = new PlayerCharacter
        {
            Name = "New Player",
            IsNpc = false,
            Gauges = new List<Gauge>(),
            Inventory = new List<Item>(),
            Spells = new List<Spell>(),
            Rolls = new List<RollResult>(),
            StatBlock = new StatBlock { Stats = new(), SpecialStats = new() },
        };
        campaign.Players.Add(player);
        await db.SaveChangesAsync();
        liveUpdates.NotifyCampaignChanged(campaignId);
        return player;
    }

    public async Task<PlayerCharacter> AddNpcAsync(int campaignId, string callerDiscordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await AssertIsDmAsync(db, campaignId, callerDiscordId);
        var campaign = await db.Campaigns.Include(c => c.Players).FirstOrDefaultAsync(c => c.Id == campaignId);
        if (campaign == null) throw new InvalidOperationException($"Campaign {campaignId} not found");

        var npc = new PlayerCharacter
        {
            Name = "New NPC",
            IsNpc = true,
            Gauges = new List<Gauge>(),
            Inventory = new List<Item>(),
            Spells = new List<Spell>(),
            Rolls = new List<RollResult>(),
            StatBlock = new StatBlock { Stats = new(), SpecialStats = new() },
        };

        campaign.Players.Add(npc);
        await db.SaveChangesAsync();
        liveUpdates.NotifyCampaignChanged(campaignId);

        return npc;
    }

    public async Task RemovePlayerAsync(Campaign campaign, PlayerCharacter player, string callerDiscordId) {

        await using var db = await dbFactory.CreateDbContextAsync();
        await AssertIsDmAsync(db, campaign.Id, callerDiscordId);
        var fetchedCampaign = await db.Campaigns.Include(c => c.Players).FirstOrDefaultAsync(c => c.Id == campaign.Id);
        if (fetchedCampaign == null) return;

        var pc = fetchedCampaign.Players?.FirstOrDefault(p => p.Id == player.Id);
        if (pc == null) return;
        fetchedCampaign.Players.Remove(pc);

        Console.WriteLine($"Removing player {player.Name} from {fetchedCampaign.Name}");

        db.Campaigns.Update(fetchedCampaign);

        await db.SaveChangesAsync();
        liveUpdates.NotifyCampaignChanged(campaign.Id);
        Console.WriteLine("Done");

    }

    public async Task UpdateAsync(Campaign c, string callerDiscordId)
    {
        if (string.IsNullOrEmpty(callerDiscordId))
            throw new UnauthorizedAccessException("Missing caller identity.");

        bool isNew = c.Id == 0;
        // Strip presets from cascade — managed separately
        var savedPresets = c.Presets;
        c.Presets = null;

        await using var db = await dbFactory.CreateDbContextAsync();

        if (isNew)
        {
            c.DungeonMasterId = callerDiscordId;
        }
        else
        {
            var existingDmId = await db.Campaigns
                .Where(x => x.Id == c.Id)
                .Select(x => x.DungeonMasterId)
                .SingleOrDefaultAsync();

            if (existingDmId != callerDiscordId)
                throw new UnauthorizedAccessException($"User {callerDiscordId} is not the DM of campaign {c.Id}.");

            // Never trust client-supplied DM id on update
            c.DungeonMasterId = existingDmId;
        }

        db.Campaigns.Update(c);

        Console.WriteLine($"Updating campaign {c.Name}");

        await db.SaveChangesAsync();
        c.Presets = savedPresets;

        if (isNew)
            await EnsureDefaultPresetAsync(c.Id);

        liveUpdates.NotifyCampaignChanged(c.Id);

        Console.WriteLine("Done");
    }

    private async Task EnsureDefaultPresetAsync(int campaignId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (await db.CharacterPresets.AnyAsync(p => p.CampaignId == campaignId)) return;
        db.CharacterPresets.Add(CreateDefaultPreset(campaignId));
        await db.SaveChangesAsync();
    }

    public static CharacterPreset CreateDefaultPreset(int campaignId = 0) => new()
    {
        Name = "Character",
        CampaignId = campaignId,
        StatBlock = new StatBlock
        {
            Stats = new List<Stat>
            {
                new() { Name = "strength", Value = 10 },
                new() { Name = "agility", Value = 10 },
                new() { Name = "constitution", Value = 10 },
                new() { Name = "intelligence", Value = 10 },
                new() { Name = "wisdom", Value = 10 },
                new() { Name = "charisma", Value = 10 },
            },
            SpecialStats = new List<Stat>(),
        },
        Gauges = new List<PresetGauge>
        {
            new() { Name = "health", Max = 100, Colour = "red", GaugeType = GaugeType.IconBar },
        },
    };

    public async Task AddPresetAsync(int campaignId, string callerDiscordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await AssertIsDmAsync(db, campaignId, callerDiscordId);
        var campaign = await db.Campaigns.Include(c => c.Presets).FirstOrDefaultAsync(c => c.Id == campaignId);
        if (campaign == null) return;
        campaign.Presets ??= new List<CharacterPreset>();
        campaign.Presets.Add(new CharacterPreset
        {
            Name = "New Preset",
            StatBlock = new StatBlock { Stats = new(), SpecialStats = new() },
            Gauges = new(),
        });
        await db.SaveChangesAsync();
        liveUpdates.NotifyCampaignChanged(campaignId);
    }

    public async Task RemovePresetAsync(int campaignId, int presetId, string callerDiscordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await AssertIsDmAsync(db, campaignId, callerDiscordId);
        var preset = await db.CharacterPresets.FindAsync(presetId);
        if (preset == null) return;
        if (preset.CampaignId != campaignId)
            throw new UnauthorizedAccessException($"Preset {presetId} does not belong to campaign {campaignId}.");
        db.CharacterPresets.Remove(preset);
        await db.SaveChangesAsync();
        liveUpdates.NotifyCampaignChanged(campaignId);
    }

    public async Task UpdatePresetAsync(CharacterPreset preset, string callerDiscordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await AssertIsDmAsync(db, preset.CampaignId, callerDiscordId);
        db.CharacterPresets.Update(preset);
        await db.SaveChangesAsync();
        liveUpdates.NotifyCampaignChanged(preset.CampaignId);
    }

    public async Task<List<PlayerCharacter>> CreateCharactersFromPresetAsync(int campaignId, int presetId, bool isNpc, int count, string callerDiscordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await AssertIsDmAsync(db, campaignId, callerDiscordId);
        var campaign = await db.Campaigns
            .Include(c => c.Players)
            .Include(c => c.Presets)
            .FirstOrDefaultAsync(c => c.Id == campaignId);
        if (campaign == null) return new();

        var preset = campaign.Presets?.FirstOrDefault(p => p.Id == presetId);
        if (preset == null) return new();

        Console.WriteLine(preset);
        
        var baseName = preset.Name ?? "Character";

        var usedNumbers = campaign.Players
            .Select(p => p.Name ?? "")
            .Where(n => n.StartsWith(baseName + " "))
            .Select(n =>
            {
                var suffix = n.Substring(baseName.Length + 1).Trim();
                return int.TryParse(suffix, out var num) ? num : 0;
            })
            .Where(n => n > 0)
            .ToHashSet();

        var created = new List<PlayerCharacter>();
        int nextNum = 1;
        for (int i = 0; i < count; i++)
        {
            while (usedNumbers.Contains(nextNum)) nextNum++;
            usedNumbers.Add(nextNum);

            var character = new PlayerCharacter
            {
                Name = $"{baseName} {nextNum}",
                IsNpc = isNpc,
                StatBlockJson = preset.StatBlockJson,
                Gauges = preset.Gauges.Select(g => new Gauge
                {
                    Name = g.Name ?? "gauge",
                    Max = g.Max,
                    Value = g.Max,
                    Colour = g.Colour,
                    GaugeType = g.GaugeType,
                }).ToList(),
                Inventory = new List<Item>(),
                Spells = new List<Spell>(),
                Rolls = new List<RollResult>(),
            };
            campaign.Players ??= new List<PlayerCharacter>();
            campaign.Players.Add(character);
            created.Add(character);
            nextNum++;
        }

        await db.SaveChangesAsync();
        liveUpdates.NotifyCampaignChanged(campaignId);
        return created;
    }
    // -- Custom dice --

    public async Task<List<CustomDie>> GetCustomDiceAsync(int campaignId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.CustomDice
            .Where(d => d.CampaignId == campaignId)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<CustomDie> SaveCustomDieAsync(CustomDie die, string callerDiscordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await AssertIsDmAsync(db, die.CampaignId, callerDiscordId);

        var existing = die.Id != Guid.Empty ? await db.CustomDice.FindAsync(die.Id) : null;
        if (existing == null)
        {
            if (die.Id == Guid.Empty) die.Id = Guid.NewGuid();
            db.CustomDice.Add(die);
        }
        else
        {
            existing.Name = die.Name;
            existing.Sides = die.Sides;
            existing.FacesJson = die.FacesJson;
        }

        await db.SaveChangesAsync();
        liveUpdates.NotifyCampaignChanged(die.CampaignId);
        return existing ?? die;
    }

    public async Task DeleteCustomDieAsync(Guid dieId, string callerDiscordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var die = await db.CustomDice.FindAsync(dieId);
        if (die == null) return;
        await AssertIsDmAsync(db, die.CampaignId, callerDiscordId);
        db.CustomDice.Remove(die);
        await db.SaveChangesAsync();
        liveUpdates.NotifyCampaignChanged(die.CampaignId);
    }

    public async Task DeleteAsync(int campaignId, string callerDiscordId) {
        await using var db = await dbFactory.CreateDbContextAsync();
        await AssertIsDmAsync(db, campaignId, callerDiscordId);

        db.Campaigns.Remove(new Campaign(){Id = campaignId});

        await db.SaveChangesAsync();

    }

    public async Task<Campaign?> GetByIdAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Campaigns
            .Include(g => g.Players)
                .ThenInclude(p => p.Gauges)

            .Include(g => g.Players)
                .ThenInclude(p => p.Rolls)
                    .ThenInclude(r => r.Rolls)

            .Include(g => g.Players)
                .ThenInclude(p => p.Buffs)
                    .ThenInclude(b => b.Template)

            .Include(g => g.Presets)

            .Include(g => g.CustomDice)

            .SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Campaign>> GetByDiscordIdAsync(string discordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db
            .Campaigns
            .Include(g => g.Players)
                .ThenInclude(p => p.Rolls)
            .Where(c => (c.DungeonMaster != null && c.DungeonMaster.Id == discordId) || (c.Players != null && c.Players.Any(c => c.UserId == discordId)))
            .ToListAsync();
    }
}
