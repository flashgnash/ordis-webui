using System.Collections.Concurrent;

public class LiveUpdateService
{
    private readonly ConcurrentDictionary<int, Action> _characterSubscribers = new();
    private readonly ConcurrentDictionary<int, Action> _campaignSubscribers = new();

    // Character-level events (gauges, rolls, stats, buffs, etc.)

    public event Action<int>? OnCharacterChanged;

    public void NotifyCharacterChanged(int characterId)
    {
        OnCharacterChanged?.Invoke(characterId);
    }

    // Campaign-level events (campaign settings, player list, campaign buffs)

    public event Action<int>? OnCampaignChanged;

    public void NotifyCampaignChanged(int campaignId)
    {
        OnCampaignChanged?.Invoke(campaignId);
    }
}
