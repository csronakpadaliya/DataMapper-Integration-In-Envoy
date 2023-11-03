namespace Perigrine.DataMapper.gRPC_Service.Model
{
    public class DataMappingResultModel
    {
        public List<string> TargetDocuments { get; set; }

        public List<CustomPropertyInfo> TargetProperties { get; set; }

        public List<AuditInfo> Audits { get; set; }
    }

    public class CustomPropertyInfo
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class AuditInfo
    {
        public string Message { get; set; }

        public string DocId { get; set; }

        public string DocName { get; set; }

        public string Path { get; set; }

        public string Value { get; set; }

        public EnumAuditStatus Status { get; set; }
    }

    public enum EnumAuditStatus
    {
        ALL,
        INFO,
        WARN,
        ERROR,
        NONE
    }


    public class DataMapperTransformModel
    {
        public string DocumentName { get; set; }

        public string DocumentVersion { get; set; }

        public string MappingJson { get; set; }

        public string MessageData { get; set; }

        public bool IsNullAsEmptyString { get; set; }

        public bool IncludeXmlDeclaration { get; set; }

        public Dictionary<string, string> NeuronEnvironment { get; set; }

        public Dictionary<string, string> CustomProperties { get; set; }

        public DataMapperTransformModel()
        {
            NeuronEnvironment = new Dictionary<string, string>();
            CustomProperties = new Dictionary<string, string>();
        }
    }
}
