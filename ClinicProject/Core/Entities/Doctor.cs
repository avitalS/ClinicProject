using System;
using System.Collections.Generic;

namespace Core.Entities;

public partial class Doctor
{
    public int DoctorId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Specialization { get; set; } = null!;
    public int YearsOfExperience { get; set; } = 1;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
