using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public partial class OrdisContext : DbContext
{
    public OrdisContext(DbContextOptions<OrdisContext> options)
        : base(options) { }

    public virtual DbSet<PlayerCharacter> Characters { get; set; }

    public virtual DbSet<Gauge> Gauges { get; set; }

    public virtual DbSet<RollResult> RollResults { get; set; }
    public virtual DbSet<IndividualRollResult> PastRolls {get; set;} 

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<Campaign> Campaigns { get; set; }

    public virtual DbSet<CharacterPreset> CharacterPresets { get; set; }

    public virtual DbSet<CustomDie> CustomDice { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<BuffTemplate> BuffTemplates { get; set; }

    public virtual DbSet<CharacterBuff> CharacterBuffs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerCharacter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("new_characters_pkey");

            entity.ToTable("characters");

            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("id");

            entity.Property(e => e.Mana).HasColumnName("mana");
            entity.Property(e => e.ManaReadoutChannelId).HasColumnName("mana_readout_channel_id");
            entity.Property(e => e.ManaReadoutMessageId).HasColumnName("mana_readout_message_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.RollServerId).HasColumnName("roll_server_id");
            entity.Property(e => e.SavedRolls).HasColumnName("saved_rolls");
            entity.Property(e => e.SpellBlock).HasColumnName("spell_block");
            entity.Property(e => e.SpellBlockChannelId).HasColumnName("spell_block_channel_id");
            entity.Property(e => e.SpellBlockHash).HasColumnName("spell_block_hash");
            entity.Property(e => e.SpellBlockMessageId).HasColumnName("spell_block_message_id");
            entity.Property(e => e.StatBlockChannelId).HasColumnName("stat_block_channel_id");
            entity.Property(e => e.StatBlockHash).HasColumnName("stat_block_hash");
            entity.Property(e => e.StatBlockMessageId).HasColumnName("stat_block_message_id");
            entity.Property(e => e.StatBlockServerId).HasColumnName("stat_block_server_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(e => e.Campaign).WithMany(e => e.Players).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasMany(e => e.Players).WithOne(e => e.Campaign).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Presets).WithOne(e => e.Campaign).HasForeignKey(e => e.CampaignId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.CustomDice).WithOne(e => e.Campaign).HasForeignKey(e => e.CampaignId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustomDie>(entity =>
        {
            entity.ToTable("custom_dice");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FacesJson).HasColumnName("faces");
        });

        modelBuilder.Entity<CharacterPreset>(entity =>
        {
            entity.ToTable("CharacterPresets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.StatBlockJson).HasColumnName("stat_block");
        });

        modelBuilder.Entity<BuffTemplate>(entity =>
        {
            entity.ToTable("buff_templates");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Owner).WithMany().HasForeignKey(e => e.OwnerId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Campaign).WithMany().HasForeignKey(e => e.CampaignId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SourceTemplate).WithMany().HasForeignKey(e => e.SourceTemplateId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CharacterBuff>(entity =>
        {
            entity.ToTable("character_buffs");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Template).WithMany().HasForeignKey(e => e.TemplateId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PlayerCharacter).WithMany(e => e.Buffs).HasForeignKey(e => e.PlayerCharacterId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("servers_pkey");

            entity.ToTable("servers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DefaultRollChannel).HasColumnName("default_roll_channel");
            entity.Property(e => e.DefaultRollServer).HasColumnName("default_roll_server");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Count).HasDefaultValue(0).HasColumnName("count");
            entity.Property(e => e.SelectedCharacter).HasColumnName("selected_character");
            entity.Property(e => e.SelectedCharacterId).HasColumnName("selected_character_id");
            // entity.Property(e => e.StatBlockChannelId).HasColumnName("stat_block_channel_id");
            // entity.Property(e => e.StatBlockHash).HasColumnName("stat_block_hash");
            // entity.Property(e => e.StatBlockMessageId).HasColumnName("stat_block_message_id");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
