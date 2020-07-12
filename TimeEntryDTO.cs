using System;

namespace AbstractMechanics.TimeTracking.Function
{
  public class TimeEntryDTO
  {
    public DateTime DateTime { get; set; }

    public string ProjectName { get; set; }

    public double Amount { get; set; }

    public string Unit { get; set; }
  }
}