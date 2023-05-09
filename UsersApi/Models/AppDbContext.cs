using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace UsersApi.Models;

public interface IUsersApiDbContext
{
    DbSet<User> Users { get; set; }

    DbSet<UserGroup> UserGroups { get; set; }

    DbSet<UserState> UserStates { get; set; }

    int SaveChanges();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    async Task<int> GetStateIdByCode(string Code)
    {
        var state = await UserStates.SingleOrDefaultAsync(x => x.Code == Code);
        return state.Id;
    }

    async Task<int> GetGroupIdByCode(string Code)
    {
        var group = await UserGroups.SingleOrDefaultAsync(x => x.Code == Code);
        return group.Id;
    }

    Task<string> GetGroupCodeById(int? id);

    bool IsAdmin(int groupId);

    async Task<IEnumerable<User>> GetUsersAsync() => await Users.ToListAsync();
}
public partial class AppDbContext : DbContext, IUsersApiDbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    public virtual DbSet<UserState> UserStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.Login)
                .HasMaxLength(50)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.UserGroupId).HasColumnName("user_group_id");
            entity.Property(e => e.UserStateId).HasColumnName("user_state_id");
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_group_pkey");

            entity.ToTable("user_group");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(5)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasColumnType("character varying")
                .HasColumnName("description");
        });

        modelBuilder.Entity<UserState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_state_pkey");

            entity.ToTable("user_state");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(7)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasColumnType("character varying")
                .HasColumnName("description");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public async Task<int> GetStateIdByCode(string Code)
    {
        var state = await UserStates.SingleOrDefaultAsync(n => n.Code == Code);
        if (state == null)
            return 0;
        return state.Id;
    }

    public async Task<int> GetGroupIdByCode(string Code)
    {
        var group = await UserGroups.SingleOrDefaultAsync(n => n.Code == Code);
        if (group == null)
            return 0;
        return group.Id;
    }

    public async Task<string> GetGroupCodeById(int? id)
    {
        var group = await UserGroups.SingleOrDefaultAsync(n => n.Id == id);
        if (group == null)
            return null;
        return group.Code;
    }

    public bool IsAdmin(int groupId)
    {
        var adminId = GetGroupIdByCode(UserGroup.Admin).Result;
        return groupId == adminId;
    }

    public async Task<IEnumerable<User>> GetUsersAsync() => await Users.ToListAsync();

}
