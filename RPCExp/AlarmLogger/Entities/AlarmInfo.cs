namespace RPCExp.AlarmLogger.Entities
{
    public class AlarmInfo
    {
        public int Id { get; set; }

        public AlarmCategory Category { get; set; }

        public string TemplateTxt { get; set; }

        public string FacilityAccessName { get; set; }

        public string DeviceName { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Condition { get; set; }
    }
}