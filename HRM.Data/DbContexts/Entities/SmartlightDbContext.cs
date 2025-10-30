using HRM.Data.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class SmartlightDbContext : DbContext
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	public SmartlightDbContext()
    {
    }
	public SmartlightDbContext(DbContextOptions<SmartlightDbContext> options, IHttpContextAccessor httpContextAccessor)
	: base(options)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public SmartlightDbContext(DbContextOptions<SmartlightDbContext> options)
        : base(options)
    {
    }

	public override int SaveChanges()
	{
		UpdateAuditFields();
		return base.SaveChanges();
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		UpdateAuditFields();
		return base.SaveChangesAsync(cancellationToken);
	}

	private void UpdateAuditFields()
	{
		var userId = GetCurrentUserId();

		foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Added))
		{
			if (entry.Entity is IAuditable)
			{
				var entity = (IAuditable)entry.Entity;
				entity.UpdatedAt = DateTime.UtcNow;
				entity.UpdatedBy = userId;

				if (entry.State == EntityState.Added)
				{
					entity.CreatedAt = DateTime.UtcNow;
					entity.CreatedBy = userId;
				}
			}
		}
	}

	public int GetCurrentUserId()
	{
		if (_httpContextAccessor == null || _httpContextAccessor.HttpContext == null)
		{
			return 0;
		}
		if (!_httpContextAccessor.HttpContext.Items.ContainsKey("UserId"))
		{
			return 0;
		}
		var userIdObj = _httpContextAccessor.HttpContext.Items["UserId"];
		if (userIdObj == null)
		{
			return 0;
		}
		if (int.TryParse(userIdObj.ToString(), out int userId))
		{
			return userId;
		}
		return 0;
	}

	public Role GetCurrentUserRole()
	{
		if (_httpContextAccessor == null || _httpContextAccessor.HttpContext == null)
		{
			return 0;
		}
		if (!_httpContextAccessor.HttpContext.Items.ContainsKey("UserRole"))
		{
			return 0;
		}
		var userRoleObj = _httpContextAccessor.HttpContext.Items["UserRole"];
		if (userRoleObj == null)
		{
			return 0;
		}
		return ConvertStringToEnumRole(userRoleObj.ToString()!);
	}
	private Role ConvertStringToEnumRole(string role)
	{
		if (role == "Admin") return Role.Admin;
		else if (role == "Partime") return Role.Partime;
		else return Role.FullTime;
	}

	public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Cabinet> Cabinets { get; set; }

    public virtual DbSet<CommandHistory> CommandHistories { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceCommand> DeviceCommands { get; set; }

    public virtual DbSet<DeviceStatus> DeviceStatuses { get; set; }

    public virtual DbSet<DeviceTelemetry> DeviceTelemetries { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<LogEvent> LogEvents { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-GVN1ESJ,1433;Initial Catalog=SmartlightIOT;User ID=sa;Password=123123mm;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.AreaId).HasName("PK__Area__70B82048636EE963");

            entity.ToTable("Area");

            entity.HasIndex(e => e.AreaCode, "UQ__Area__72299A27665849E5").IsUnique();

            entity.Property(e => e.AreaCode).HasMaxLength(50);
            entity.Property(e => e.AreaName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Cabinet>(entity =>
        {
            entity.HasKey(e => e.CabinetId).HasName("PK__Cabinet__9C173ED30E4F82E2");

            entity.ToTable("Cabinet");

            entity.HasIndex(e => e.CabinetCode, "UQ__Cabinet__1CD39EE307E24AB0").IsUnique();

            entity.Property(e => e.CabinetCode).HasMaxLength(50);
            entity.Property(e => e.CabinetName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(255);

            entity.HasOne(d => d.Route).WithMany(p => p.Cabinets)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cabinet__RouteId__412EB0B6");
        });

        modelBuilder.Entity<CommandHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__CommandH__4D7B4ABDE2B79001");

            entity.ToTable("CommandHistory");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Result).HasMaxLength(500);

            entity.HasOne(d => d.Command).WithMany(p => p.CommandHistories)
                .HasForeignKey(d => d.CommandId)
                .HasConstraintName("FK__CommandHi__Comma__5BE2A6F2");

            entity.HasOne(d => d.Device).WithMany(p => p.CommandHistories)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK__CommandHi__Devic__5AEE82B9");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.DeviceId).HasName("PK__Device__49E1231158021037");

            entity.ToTable("Device");

            entity.HasIndex(e => e.DeviceCode, "UQ__Device__AFFB3E95AB770E46").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeviceCode).HasMaxLength(50);
            entity.Property(e => e.DeviceName).HasMaxLength(255);
            entity.Property(e => e.FirmwareVersion).HasMaxLength(50);
            entity.Property(e => e.InstallDate).HasColumnType("datetime");
            entity.Property(e => e.LastSeen).HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.MqttTopic).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Offline");

            entity.HasOne(d => d.Cabinet).WithMany(p => p.Devices)
                .HasForeignKey(d => d.CabinetId)
                .HasConstraintName("FK__Device__CabinetI__49C3F6B7");

            entity.HasOne(d => d.DeviceType).WithMany(p => p.Devices)
                .HasForeignKey(d => d.DeviceTypeId)
                .HasConstraintName("FK__Device__DeviceTy__48CFD27E");
        });

        modelBuilder.Entity<DeviceCommand>(entity =>
        {
            entity.HasKey(e => e.CommandId).HasName("PK__DeviceCo__6B410B06EFAACF72");

            entity.ToTable("DeviceCommand");

            entity.Property(e => e.AcknowledgedAt).HasColumnType("datetime");
            entity.Property(e => e.CommandType).HasMaxLength(50);
            entity.Property(e => e.Payload).HasMaxLength(500);
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceCommands)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK__DeviceCom__Devic__5629CD9C");
        });

        modelBuilder.Entity<DeviceStatus>(entity =>
        {
            entity.HasKey(e => e.DeviceStatusId).HasName("PK__DeviceSt__17D8CDD2F2AFBF65");

            entity.ToTable("DeviceStatus");

            entity.Property(e => e.ReceivedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceStatuses)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK__DeviceSta__Devic__4E88ABD4");
        });

        modelBuilder.Entity<DeviceTelemetry>(entity =>
        {
            entity.HasKey(e => e.TelemetryId).HasName("PK__DeviceTe__157CACF72D672AFC");

            entity.ToTable("DeviceTelemetry");

            entity.Property(e => e.ReceivedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceTelemetries)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK__DeviceTel__Devic__52593CB8");
        });

        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.HasKey(e => e.DeviceTypeId).HasName("PK__DeviceTy__07A6C7F633A2C972");

            entity.ToTable("DeviceType");

            entity.HasIndex(e => e.TypeCode, "UQ__DeviceTy__3E1CDC7C02BAF8EE").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TypeCode).HasMaxLength(50);
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<LogEvent>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__LogEvent__5E5486486F9EDCC7");

            entity.ToTable("LogEvent");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.EventType).HasMaxLength(100);
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PK__Route__80979B4D30DD931C");

            entity.ToTable("Route");

            entity.HasIndex(e => e.RouteCode, "UQ__Route__FDC345854922DA15").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RouteCode).HasMaxLength(50);
            entity.Property(e => e.RouteName).HasMaxLength(255);

            entity.HasOne(d => d.Area).WithMany(p => p.Routes)
                .HasForeignKey(d => d.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Route__AreaId__3C69FB99");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C986CDA13");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "UQ__User__536C85E495E54F53").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
