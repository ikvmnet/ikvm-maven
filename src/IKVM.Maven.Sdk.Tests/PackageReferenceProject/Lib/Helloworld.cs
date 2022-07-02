namespace IKVM.Maven.Sdk.Tests.PackageReferenceProject.Lib
{

    public static class Helloworld
    {

        public static string TestJava(string value)
        {
            return IKVM.Maven.Sdk.Tests.PackProject.Lib.Helloworld.TestJava(value);
        }

    }

}
