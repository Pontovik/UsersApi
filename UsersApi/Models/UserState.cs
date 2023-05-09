using System;
using System.Collections.Generic;

namespace UsersApi.Models;

public partial class UserState
{
    public static readonly string Blocked = "Blocked";
    public static readonly string Active = "Active";
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }
}
