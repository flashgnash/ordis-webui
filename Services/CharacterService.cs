using Microsoft.EntityFrameworkCore;

public class PlayerCharacterService(IDbContextFactory<OrdisContext> dbFactory, HttpClient httpClient)
{
    public async Task UpdateGaugeAsync(Gauge gauge)
    {
        Console.WriteLine($"Update gauge {gauge.Id}");
        using var db = await dbFactory.CreateDbContextAsync();

        var old = await db.Gauges.FirstOrDefaultAsync(x => x.Id == gauge.Id);

        if (old == null)
            db.Gauges.Add(gauge);
        else
        {
            old.Name = gauge.Name;
            old.Value = gauge.Value;
            old.Max = gauge.Max;
            old.Icon = gauge.Icon;
        }

        await db.SaveChangesAsync();
    }

    public async Task<RollResult> RollFor(PlayerCharacter character, String rollFormula)
    {
        var response = await httpClient.PostAsync(
            $"http://localhost:3000/roll/{character.Id}/{rollFormula}",
            null
        );

        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.InternalServerError:
            case System.Net.HttpStatusCode.BadRequest:
                throw new InvalidRollException();
        }
        var rollResult = await response.Content.ReadFromJsonAsync<RollResult>();
        
        await SaveRollAsync(character.Id, rollResult);

        return rollResult;
    }

    public async Task SaveRollAsync(int id, RollResult rollResult) {
        
        using var db = await dbFactory.CreateDbContextAsync();


        var character = await db.Characters.FirstOrDefaultAsync(c => c.Id == id) ?? throw new Exception("Not found by that ID");
        rollResult.Timestamp = rollResult.Timestamp ?? DateTime.UtcNow;

        character.Rolls = character.Rolls ?? new List<RollResult>();
        character.Rolls.Add(rollResult);

        await db.SaveChangesAsync();
    } 

    public async Task<RollResult?> GetLatestRollAsync(PlayerCharacter character) {

        using var db = await dbFactory.CreateDbContextAsync();

        var characterWithRolls = await db.Characters.Include(c => c.Rolls).FirstOrDefaultAsync(c => c.Id == character.Id);
    
        return characterWithRolls?.Rolls?.OrderBy(r => r.Timestamp).FirstOrDefault();
    }

    public async Task UpdateAsync(PlayerCharacter c)
    {
        var db = await dbFactory.CreateDbContextAsync();

        c.StatBlock = c.StatBlock;

        db.Characters.Update(c);

        await db.SaveChangesAsync();
    }


    public async Task CreateAsync(PlayerCharacter c) {
        
        var db = await dbFactory.CreateDbContextAsync();

        await db.Characters.AddAsync(c);

        await db.SaveChangesAsync();
    }

    // public async Task CreateOrUpdateAsync(Gauge gauge)
    // {
    //     await using var db = await dbFactory.CreateDbContextAsync();

    //     Console.WriteLine($"pre-find {gauge.Id}");

    //     var existing = await db.Gauges.FindAsync(gauge.Id);

    //     Console.WriteLine($"post-find {gauge.Id}");
    //     if (existing != null)
    //     {
    //         existing.Value = gauge.Value;
    //         existing.Max = gauge.Max;
    //         existing.Icon = gauge.Icon;
    //         existing.Name = gauge.Name;
    //     }
    //     else
    //     {
    //         gauge.Id = Guid.NewGuid(); // make new ID
    //         Console.WriteLine(gauge.Id);
    //         await db.Gauges.AddAsync(gauge);
    //     }

    //     await db.SaveChangesAsync();
    // }

    public async Task<PlayerCharacter?> GetByIdAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Characters
            .Include(g => g.Gauges)
            .Include(g => g.Campaign)
            .Include(g => g.Rolls)
            .SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<PlayerCharacter>> GetByDiscordIdAsync(string discordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db
            .Characters.Include(g => g.Gauges).Include(c => c.Rolls)
            .Where(c => c.UserId == discordId)
            .OrderBy(c => c.Rolls.Any())
            .ThenBy(c => c.Rolls.Max(r => r.Timestamp))
            .ToListAsync();
    }
}
