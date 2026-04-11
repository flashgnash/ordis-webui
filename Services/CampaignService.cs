using Microsoft.EntityFrameworkCore;

public class CampaignService(IDbContextFactory<OrdisContext> dbFactory)
{

    public async Task RemovePlayerAsync(Campaign campaign, PlayerCharacter player) {
        
        var db = await dbFactory.CreateDbContextAsync();

        var fetchedCampaign = await db.Campaigns.Include(c => c.Players).FirstOrDefaultAsync(c => c.Id == campaign.Id);


        var pc = fetchedCampaign.Players.First(p => p.Id == player.Id);
        fetchedCampaign.Players.Remove(pc);

        Console.WriteLine($"Removing player {player.Name} from {fetchedCampaign.Name}");

        db.Campaigns.Update(fetchedCampaign);

        await db.SaveChangesAsync();
        Console.WriteLine("Done");
        
    }

    public async Task UpdateAsync(Campaign c)
    {
        var db = await dbFactory.CreateDbContextAsync();

        db.Campaigns.Update(c);

        Console.WriteLine($"Updating campaign {c.Name}");

        await db.SaveChangesAsync();

        Console.WriteLine("Done");

    }
    public async Task DeleteAsync(int campaignId) {
        var db = await dbFactory.CreateDbContextAsync();

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

            .SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Campaign>> GetByDiscordIdAsync(string discordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db
            .Campaigns.Include(g => g.Players)
            .Where(c => (c.DungeonMaster != null && c.DungeonMaster.Id == discordId) || (c.Players != null && c.Players.Any(c => c.UserId == discordId)))
            .ToListAsync();
    }
}
