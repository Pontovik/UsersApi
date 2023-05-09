using System;
using System.Collections.Generic;

namespace UsersApi.Models;

public partial class UserGroup
{
	public static readonly string Admin = "Admin";
	public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }
}
