using System;

namespace AbstractMechanics.TimeTracking.Models.Dtos
{
  public class TimeEntryDto
  {
    public string Key { get; set; }
    
    public DateTime DateTime { get; set; }

    public string ProjectName { get; set; }

    public double Amount { get; set; }

    public string Unit { get; set; }
  }
}