namespace WebBookingSystem.Data.Services
{
    public class BusinessHours
    {    
        public TimeSpan OpenTime { get; set; } = new TimeSpan(9, 0, 0);

        public TimeSpan CloseTime { get; set; } = new TimeSpan(18, 0, 0);

        public List<DayOfWeek> WorkingDays { get; set; } = new List<DayOfWeek>
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };

        public bool IsWorkingDay(DayOfWeek day)
        {
            return WorkingDays.Contains(day);
        }
        public bool IsWithinWorkingHours(TimeSpan time)
        {
            return time >= OpenTime && time < CloseTime;
        }
        public string GetBusinessHoursDisplay()
        {
            // Convert TimeSpan to DateTime for AM/PM formatting
            var openDateTime = DateTime.Today.Add(OpenTime);
            var closeDateTime = DateTime.Today.Add(CloseTime);
            return $"{openDateTime:h:mm tt} - {closeDateTime:h:mm tt}";
        }
    }
}
