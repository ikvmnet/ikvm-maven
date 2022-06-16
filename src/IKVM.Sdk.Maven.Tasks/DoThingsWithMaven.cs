using System;
using System.Collections;

using java.io;
using java.lang;
using java.util;

using Microsoft.Build.Utilities;

using org.apache.maven.repository.@internal;
using org.apache.maven.settings;
using org.apache.maven.settings.building;
using org.apache.maven.settings.crypto;
using org.eclipse.aether;
using org.eclipse.aether.artifact;
using org.eclipse.aether.collection;
using org.eclipse.aether.connector.basic;
using org.eclipse.aether.graph;
using org.eclipse.aether.impl;
using org.eclipse.aether.repository;
using org.eclipse.aether.resolution;
using org.eclipse.aether.spi.connector;
using org.eclipse.aether.spi.connector.transport;
using org.eclipse.aether.transport.file;
using org.eclipse.aether.transport.http;
using org.eclipse.aether.util.artifact;
using org.eclipse.aether.util.filter;
using org.eclipse.aether.util.repository;
using org.sonatype.plexus.components.cipher;
using org.sonatype.plexus.components.sec.dispatcher;

namespace IKVM.Sdk.Maven.Tasks
{

    public class DoThingsWithMaven : Task
    {

        class SecDispatcher : DefaultSecDispatcher
        {

            public SecDispatcher() : base(new DefaultPlexusCipher(), Collections.emptyMap(), "~/.m2/settings-security.xml")
            {

            }

        }

        class ErrorHandler : DefaultServiceLocator.ErrorHandler
        {

            public override void serviceCreationFailed(Class type, Class impl, System.Exception exception)
            {
                System.Console.WriteLine("Service creation failed for {0} with implementation {1}", type, impl, exception);
            }

        }

        static readonly SettingsBuilder SETTINGS_BUILDER = new DefaultSettingsBuilderFactory().newInstance();
        static readonly SettingsDecrypter SETTINGS_DECRYPTER = new DefaultSettingsDecrypter(new SecDispatcher());


        static Settings GetSettings()
        {
            DefaultSettingsBuildingRequest request = new DefaultSettingsBuildingRequest();
            request.setUserSettingsFile(new File(new File(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".m2"), Names.SETTINGS_XML));

            var set = SETTINGS_BUILDER.build(request).getEffectiveSettings();
            var rlt = SETTINGS_DECRYPTER.decrypt(new DefaultSettingsDecryptionRequest(set));
            set.setServers(rlt.getServers());
            set.setProxies(rlt.getProxies());
            return set;
        }

        static File GetDefaultLocalRepoDir()
        {
            var settings = GetSettings();
            if (settings.getLocalRepository() != null)
                return new File(settings.getLocalRepository());

            return new File(new File(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".m2"), "repository");
        }

        static RepositorySystem NewRepositorySystem()
        {
            var locator = MavenRepositorySystemUtils.newServiceLocator();
            locator.addService(typeof(RepositoryConnectorFactory), typeof(BasicRepositoryConnectorFactory));
            locator.addService(typeof(TransporterFactory), typeof(FileTransporterFactory));
            locator.addService(typeof(TransporterFactory), typeof(HttpTransporterFactory));
            locator.setErrorHandler(new ErrorHandler());
            return (RepositorySystem)locator.getService(typeof(RepositorySystem));
        }

        static ProxySelector GetProxySelector()
        {
            var selector = new DefaultProxySelector();

            var settings = GetSettings();
            foreach (org.apache.maven.settings.Proxy proxy in settings.getProxies().toArray())
            {
                AuthenticationBuilder auth = new AuthenticationBuilder();
                auth.addUsername(proxy.getUsername()).addPassword(proxy.getPassword());
                selector.add(new org.eclipse.aether.repository.Proxy(proxy.getProtocol(), proxy.getHost(), proxy.getPort(), auth.build()), proxy.getNonProxyHosts());
            }

            return selector;
        }

        static MirrorSelector GetMirrorSelector()
        {
            var selector = new DefaultMirrorSelector();

            var settings = GetSettings();
            foreach (org.apache.maven.settings.Mirror mirror in settings.getMirrors().toArray())
                selector.add(mirror.getId(), mirror.getUrl(), mirror.getLayout(), false, false, mirror.getMirrorOf(), mirror.getMirrorOfLayouts());

            return selector;
        }

        static AuthenticationSelector GetAuthSelector()
        {
            DefaultAuthenticationSelector selector = new DefaultAuthenticationSelector();


            Settings settings = GetSettings();
            foreach (Server server in settings.getServers().toArray())
            {
                AuthenticationBuilder auth = new AuthenticationBuilder();
                auth.addUsername(server.getUsername()).addPassword(server.getPassword());
                auth.addPrivateKey(server.getPrivateKey(), server.getPassphrase());
                selector.add(server.getId(), auth.build());
            }

            return new ConservativeAuthenticationSelector(selector);
        }

        static DefaultRepositorySystemSession NewRepositorySystemSession(RepositorySystem system)
        {
            var session = MavenRepositorySystemUtils.newSession();
            var repository = new LocalRepository(GetDefaultLocalRepoDir());
            session.setLocalRepositoryManager(system.newLocalRepositoryManager(session, repository));
            session.setCache(new DefaultRepositoryCache());
            session.setProxySelector(GetProxySelector());
            session.setMirrorSelector(GetMirrorSelector());
            session.setAuthenticationSelector(GetAuthSelector());
            return session;
        }
        static RemoteRepository NewCentralRepository()
        {
            return new RemoteRepository.Builder("central", "default", "https://repo.maven.apache.org/maven2/").build();
        }

        static List NewRepositories(RepositorySystem system, RepositorySystemSession session)
        {
            return new java.util.ArrayList(Collections.singletonList(NewCentralRepository()));
        }

        public override bool Execute()
        {
            var system = NewRepositorySystem();

            var session = NewRepositorySystemSession(system);
            var artifact = new DefaultArtifact("org.apache.maven.resolver:maven-resolver-impl:1.3.3");
            var classpathFlter = DependencyFilterUtils.classpathFilter(JavaScopes.COMPILE);
            var collectRequest = new CollectRequest();
            collectRequest.setRoot(new Dependency(artifact, JavaScopes.COMPILE));
            collectRequest.setRepositories(NewRepositories(system, session));
            var dependencyRequest = new DependencyRequest(collectRequest, classpathFlter);
            var artifactResults = system.resolveDependencies(session, dependencyRequest).getArtifactResults();

            foreach (ArtifactResult a in (IEnumerable)artifactResults)
            {
                System.Console.WriteLine("Resolved: {0}:{1}:{2}", a.getArtifact().getGroupId(), a.getArtifact().getArtifactId(), a.getArtifact().getVersion());
            }

            return false;
        }

    }

}
