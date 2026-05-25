using System;
using System.Collections.Generic;

namespace Core.Entities;

public partial class Client
{
    public int ClientId { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Email { get; set; }

    public string Pass { get; set; }

    public DateTime DateOfBirth { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
