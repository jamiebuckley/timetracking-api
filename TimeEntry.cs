using Microsoft.Azure.Cosmos.Table;

namespace AbstractMechanics.TimeTracking.Function
{
    public class TimeEntry : TableEntity
    {
      public double Amount { get; set; }

      public string Unit { get; set; }

      public string ProjectName { get; set; }
    }
}