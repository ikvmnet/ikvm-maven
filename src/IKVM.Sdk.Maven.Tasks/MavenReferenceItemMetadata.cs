namespace IKVM.Sdk.Maven.Tasks
{

    public static class MavenReferenceItemMetadata
    {

        public const char PropertySeperatorChar = ';';
        public static readonly string PropertySeperatorString = PropertySeperatorChar.ToString();
        public static readonly char[] PropertySeperatorCharArray = new[] { PropertySeperatorChar };
        public static readonly string GroupId = "GroupId";
        public static readonly string ArtifactId = "ArtifactId";
        public static readonly string Classifier = "Classifier";
        public static readonly string Version = "Version";
        public static readonly string Dependencies = "Dependencies";
        public static readonly string Scopes = "Scopes";
        public static readonly string IncludeOptional = "IncludeOptional";
        public static readonly string AssemblyName = "AssemblyName";
        public static readonly string AssemblyVersion = "AssemblyVersion";
        public static readonly string Debug = "Debug";

    }

}
