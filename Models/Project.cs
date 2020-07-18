using Microsoft.Azure.Cosmos.Table;

namespace AbstractMechanics.TimeTracking.Models
{
    public class Project : TableEntity {
        public string Color { get; set; }
    }
}