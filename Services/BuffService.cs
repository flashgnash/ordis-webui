using Microsoft.EntityFrameworkCore;

public class BuffService(IDbContextFactory<OrdisContext> dbFactory)
{
    // -- Library queries --

    public async Task<List<BuffTemplate>> GetPlayerLibraryAsync(string ownerId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.BuffTemplates
            .Where(t => t.OwnerId == ownerId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<BuffTemplate>> GetCampaignLibraryAsync(int campaignId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.BuffTemplates
            .Where(t => t.CampaignId == campaignId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<BuffTemplate>> SearchTemplatesAsync(string query, string ownerId, int? campaignId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.BuffTemplates
            .Where(t => t.OwnerId == ownerId || (campaignId != null && t.CampaignId == campaignId));

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(t => EF.Functions.ILike(t.Name, $"%{query}%"));

        return await q.OrderBy(t => t.Name).Take(20).ToListAsync();
    }

    // -- Template CRUD --

    public async Task<BuffTemplate> CreateTemplateAsync(BuffTemplate template)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.BuffTemplates.Add(template);
        await db.SaveChangesAsync();
        return template;
    }

    public async Task UpdateTemplateAsync(BuffTemplate template)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var existing = await db.BuffTemplates.FindAsync(template.Id);
        if (existing == null) return;
        existing.Name = template.Name;
        existing.Icon = template.Icon;
        existing.Type = template.Type;
        existing.EffectsJson = template.EffectsJson;
        await db.SaveChangesAsync();
    }

    public async Task DeleteTemplateAsync(Guid templateId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var template = await db.BuffTemplates.FindAsync(templateId);
        if (template != null)
        {
            db.BuffTemplates.Remove(template);
            await db.SaveChangesAsync();
        }
    }

    // -- Export to campaign (new UUID), links the player template to the campaign one --

    public async Task<BuffTemplate> ExportToCampaignAsync(Guid templateId, int campaignId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var source = await db.BuffTemplates.FindAsync(templateId);
        if (source == null) throw new InvalidOperationException("Template not found");

        var campaignTemplate = new BuffTemplate
        {
            Id = Guid.NewGuid(),
            Name = source.Name,
            Icon = source.Icon,
            Type = source.Type,
            EffectsJson = source.EffectsJson,
            CampaignId = campaignId
        };
        db.BuffTemplates.Add(campaignTemplate);

        // Link the player template to the new campaign template
        source.SourceTemplateId = campaignTemplate.Id;

        await db.SaveChangesAsync();
        return campaignTemplate;
    }

    // -- Import from campaign: create player-level copy linked to campaign source --

    public async Task<BuffTemplate> ImportFromCampaignAsync(Guid campaignTemplateId, string ownerId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var source = await db.BuffTemplates.FindAsync(campaignTemplateId);
        if (source == null) throw new InvalidOperationException("Template not found");

        // Check if player already has a template linked to this campaign template
        var existing = await db.BuffTemplates
            .FirstOrDefaultAsync(t => t.OwnerId == ownerId && t.SourceTemplateId == campaignTemplateId);
        if (existing != null) return existing;

        var playerTemplate = new BuffTemplate
        {
            Id = Guid.NewGuid(),
            Name = source.Name,
            Icon = source.Icon,
            Type = source.Type,
            EffectsJson = source.EffectsJson,
            OwnerId = ownerId,
            SourceTemplateId = campaignTemplateId
        };
        db.BuffTemplates.Add(playerTemplate);
        await db.SaveChangesAsync();
        return playerTemplate;
    }

    // -- Pull: reset player template to match its campaign source --

    public async Task PullFromSourceAsync(Guid templateId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var template = await db.BuffTemplates
            .Include(t => t.SourceTemplate)
            .FirstOrDefaultAsync(t => t.Id == templateId);
        if (template?.SourceTemplate == null) return;

        template.Name = template.SourceTemplate.Name;
        template.Icon = template.SourceTemplate.Icon;
        template.Type = template.SourceTemplate.Type;
        template.EffectsJson = template.SourceTemplate.EffectsJson;
        await db.SaveChangesAsync();
    }

    // -- Get a template with its source loaded --

    public async Task<BuffTemplate?> GetTemplateWithSourceAsync(Guid templateId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.BuffTemplates
            .Include(t => t.SourceTemplate)
            .FirstOrDefaultAsync(t => t.Id == templateId);
    }

    // -- Apply buff to character --

    public async Task<CharacterBuff> ApplyBuffAsync(int characterId, Guid templateId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        // Prevent duplicate: same template on same character
        var existing = await db.CharacterBuffs
            .FirstOrDefaultAsync(cb => cb.PlayerCharacterId == characterId && cb.TemplateId == templateId);
        if (existing != null) return existing;

        var buff = new CharacterBuff
        {
            TemplateId = templateId,
            PlayerCharacterId = characterId
        };
        db.CharacterBuffs.Add(buff);
        await db.SaveChangesAsync();
        return buff;
    }

    public async Task RemoveBuffAsync(Guid characterBuffId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var buff = await db.CharacterBuffs.FindAsync(characterBuffId);
        if (buff != null)
        {
            db.CharacterBuffs.Remove(buff);
            await db.SaveChangesAsync();
        }
    }

    public async Task UpdateStacksAsync(Guid characterBuffId, int stacks)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var buff = await db.CharacterBuffs.FindAsync(characterBuffId);
        if (buff != null)
        {
            buff.Stacks = Math.Max(0, stacks);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<CharacterBuff>> GetCharacterBuffsAsync(int characterId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.CharacterBuffs
            .Include(cb => cb.Template)
                .ThenInclude(t => t.SourceTemplate)
            .Where(cb => cb.PlayerCharacterId == characterId)
            .ToListAsync();
    }
}
