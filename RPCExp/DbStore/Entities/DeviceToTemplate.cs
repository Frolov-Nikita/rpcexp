namespace RPCExp.DbStore.Entities
{
    /// <summary>
    /// Класс для связи many2many
    /// </summary>
    public class DeviceToTemplate
    {
        public int DeviceId { get; set; }

        public DeviceCfg Device { get; set; }

        public int TemplateId { get; set; }

        public Template Template { get; set; }
    }

}
