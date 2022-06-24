namespace IKVM.Sdk.Maven.Tasks
{

    static class IkvmReferenceItemMetadata
    {

        public const char PropertySeperatorChar = ';';
        public static readonly string PropertySeperatorString = PropertySeperatorChar.ToString();
        public static readonly char[] PropertySeperatorCharArray = new[] { PropertySeperatorChar };
        public static readonly string AssemblyName = "AssemblyName";
        public static readonly string AssemblyVersion = "AssemblyVersion";
        public static readonly string DisableAutoAssemblyName = "DisableAutoAssemblyName";
        public static readonly string DisableAutoAssemblyVersion = "DisableAutoAssemblyVersion";
        public static readonly string FallbackAssemblyName = "FallbackAssemblyName";
        public static readonly string FallbackAssemblyVersion = "FallbackAssemblyVersion";
        public static readonly string Debug = "Debug";
        public static readonly string Compile = "Compile";
        public static readonly string Sources = "Sources";
        public static readonly string References = "References";
        public static readonly string IkvmIdentity = "IkvmIdentity";

        public static readonly string MavenGroupId = "MavenGroupId";
        public static readonly string MavenArtifactId = "MavenArtifactId";
        public static readonly string MavenClassifier = "MavenClassifier";
        public static readonly string MavenVersion = "MavenVersion";

    }

}
