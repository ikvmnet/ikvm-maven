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

    }

}
