using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

}
public enum Role
{
	Admin = 1,
	Partime = 2,
	FullTime = 3
}
public static class RoleExtensions
{
	public const string ADMIN_ROLE = "AdminRole";
	public const string PARTIME_ROLE = "PartimeRole";
	public const string FULLTIME_ROLE = "FulltimeRole";
}
