using System;

namespace AbstractMechanics.TimeTracking.Models.Dtos
{
    public class TimeQueryDto
    {
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
    }
}