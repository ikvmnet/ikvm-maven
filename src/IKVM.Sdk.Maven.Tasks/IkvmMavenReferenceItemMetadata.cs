namespace IKVM.Sdk.Maven.Tasks
{
    internal static class IkvmMavenReferenceItemMetadata
    {
        public const char PropertySeperatorChar = ';';
        public static readonly string PropertySeperatorString = PropertySeperatorChar.ToString();
        public static readonly char[] PropertySeperatorCharArray = new[] { PropertySeperatorChar };
        public static readonly string GroupId = "GroupId";
        public static readonly string Version = "Version";
        public static readonly string Classifier = "Classifier";
        public static readonly string Extension = "Ext"; // NOTE: Extension is a reserved word in ITaskItem metadata
        public const string DefaultExtension = "jar";
    }
}
