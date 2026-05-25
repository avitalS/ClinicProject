using System;
using System.Collections.Generic;

namespace Core.Entities;

public partial class Visit
{
    public int VisitId { get; set; }

    public int DoctorId { get; set; }

    public int ClientId { get; set; }

    public int Priority { get; set; } 

    public int Duration { get; set; }

    public DateTime DateAndHour { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Doctor Doctor { get; set; } = null!;
}
