namespace IKVM.Sdk.Maven.Tasks
{

    public static class MavenReferenceItemMetadata
    {

        public const char PropertySeperatorChar = ';';
        public static readonly string PropertySeperatorString = PropertySeperatorChar.ToString();
        public static readonly char[] PropertySeperatorCharArray = new[] { PropertySeperatorChar };
        public static readonly string GroupId = "GroupId";
        public static readonly string ArtifactId = "ArtifactId";
        public static readonly string Version = "Version";
        public static readonly string Dependencies = "Dependencies";
        public static readonly string Compile = "Compile";
        public static readonly string Runtime = "Runtime";
        public static readonly string Sources = "Sources";

    }

}
