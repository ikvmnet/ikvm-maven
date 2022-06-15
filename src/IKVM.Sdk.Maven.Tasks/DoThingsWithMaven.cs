using Microsoft.Build.Utilities;

using org.eclipse.aether;
using org.eclipse.aether.artifact;
using org.eclipse.aether.collection;
using org.eclipse.aether.graph;
using org.eclipse.aether.resolution;

namespace IKVM.Sdk.Maven.Tasks
{

    public class SampleTask : Task
    {

        public override bool Execute()
        {
            RepositorySystem system = Booter.newRepositorySystem(Booter.selectFactory(args));

            RepositorySystemSession session = Booter.newRepositorySystemSession(system);

            Artifact artifact = new DefaultArtifact("org.apache.maven.resolver:maven-resolver-impl:1.3.3");

            DependencyFilter classpathFlter = DependencyFilterUtils.classpathFilter(JavaScopes.COMPILE);

            CollectRequest collectRequest = new CollectRequest();
            collectRequest.setRoot(new Dependency(artifact, JavaScopes.COMPILE));
            collectRequest.setRepositories(Booter.newRepositories(system, session));

            DependencyRequest dependencyRequest = new DependencyRequest(collectRequest, classpathFlter);

            java.util.List artifactResults = system.resolveDependencies(session, dependencyRequest).getArtifactResults();

            return false;
        }

    }

}
