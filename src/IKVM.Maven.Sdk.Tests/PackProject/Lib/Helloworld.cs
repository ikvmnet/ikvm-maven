namespace IKVM.Maven.Sdk.Tests.PackProject.Lib
{

    public static class Helloworld
    {

        public static string TestJava(string value)
        {
            var n = new org.apache.maven.DefaultMaven();
            return value;
        }

    }

}
