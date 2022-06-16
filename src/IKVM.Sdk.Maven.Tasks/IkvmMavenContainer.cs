using java.io;
using java.lang;
using java.util;
using org.apache.maven.repository.@internal;
using org.apache.maven.settings;
using org.apache.maven.settings.building;
using org.apache.maven.settings.crypto;
using org.eclipse.aether;
using org.eclipse.aether.collection;
using org.eclipse.aether.connector.basic;
using org.eclipse.aether.impl;
using org.eclipse.aether.repository;
using org.eclipse.aether.spi.connector;
using org.eclipse.aether.spi.connector.transport;
using org.eclipse.aether.transport.file;
using org.eclipse.aether.transport.http;
using org.eclipse.aether.util.graph.transformer;
using org.eclipse.aether.util.repository;
using org.sonatype.plexus.components.cipher;
using org.sonatype.plexus.components.sec.dispatcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Path = System.IO.Path;

namespace IKVM.Sdk.Maven.Tasks
{
    internal class IkvmMavenContainer
    {
        private const string SettingsXml = "settings.xml";
        private const string SettingsSecurityXml = "settings-security.xml";
        private const string CentralRepositoryUrl = "https://repo.maven.apache.org/maven2/";
        private const string CentralRepositoryType = "default";
        private const string CentralRepostiroyId = "central";
        private static readonly string DefaultMavenRepositoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".m2");

        public string MavenRepositoryPath { get; }

        public RepositorySystem System { get; }

        public RepositorySystemSession Session { get; }

        public List Repositories { get; }

        public IkvmMavenContainer(IDictionary<string, string> systemProperties, IDictionary<string, string> userProperties)
        {
            MavenRepositoryPath = DefaultMavenRepositoryPath; // TODO: Override default if MSBuild property is provided
            var settings = GetSettings(systemProperties, userProperties);
            //MavenRepositoryPath = DefaultMavenRepositoryPath; // TODO: This should be set after we get settings
            System = NewRepositorySystem();
            Session = NewRepositorySystemSession(System, settings);
            Repositories = NewRepositories(System, Session, settings);
        }

        class SecDispatcher : DefaultSecDispatcher
        {
            public SecDispatcher(string configurationFile)
                : base(new DefaultPlexusCipher(), Collections.emptyMap(), configurationFile)
            {
            }
        }

        class ErrorHandler : DefaultServiceLocator.ErrorHandler
        {

            public override void serviceCreationFailed(Class type, Class impl, System.Exception exception)
            {
                Trace.WriteLine(string.Format("Service creation failed for {0} with implementation {1}", type, impl, exception));
            }

        }

        private Settings GetSettings(IDictionary<string, string> systemProperties, IDictionary<string, string> userProperties)
        {
            // TODO: Process properties passed in from MSBuild to replace Maven system and user properties
  

            DefaultSettingsBuildingRequest request = new DefaultSettingsBuildingRequest();
            request.setUserSettingsFile(new File(Path.Combine(DefaultMavenRepositoryPath, SettingsXml)));


            var settingsBuilder = new DefaultSettingsBuilderFactory().newInstance();
            var set = settingsBuilder.build(request).getEffectiveSettings();

            var securityDispatcher = new SecDispatcher(Path.Combine(MavenRepositoryPath, SettingsSecurityXml));
            var settingsDecrypter = new DefaultSettingsDecrypter(securityDispatcher);
            var rlt = settingsDecrypter.decrypt(new DefaultSettingsDecryptionRequest(set));
            set.setServers(rlt.getServers());
            set.setProxies(rlt.getProxies());
            return set;
        }

        static File GetDefaultLocalRepoDir(Settings settings)
        {
            if (settings.getLocalRepository() != null)
                return new File(settings.getLocalRepository());

            return new File(Path.Combine(DefaultMavenRepositoryPath, "repository"));
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

        static ProxySelector GetProxySelector(Settings settings)
        {
            var selector = new DefaultProxySelector();

            foreach (org.apache.maven.settings.Proxy proxy in settings.getProxies().toArray())
            {
                AuthenticationBuilder auth = new AuthenticationBuilder();
                auth.addUsername(proxy.getUsername()).addPassword(proxy.getPassword());
                selector.add(new org.eclipse.aether.repository.Proxy(
                    proxy.getProtocol(), proxy.getHost(), proxy.getPort(), auth.build()), proxy.getNonProxyHosts());
            }

            return selector;
        }

        static MirrorSelector GetMirrorSelector(Settings settings)
        {
            var selector = new DefaultMirrorSelector();

            foreach (Mirror mirror in settings.getMirrors().toArray())
                selector.add(mirror.getId(), mirror.getUrl(), mirror.getLayout(), false, false, mirror.getMirrorOf(), mirror.getMirrorOfLayouts());

            return selector;
        }

        static AuthenticationSelector GetAuthSelector(Settings settings)
        {
            DefaultAuthenticationSelector selector = new DefaultAuthenticationSelector();


            foreach (Server server in settings.getServers().toArray())
            {
                AuthenticationBuilder auth = new AuthenticationBuilder();
                auth.addUsername(server.getUsername()).addPassword(server.getPassword());
                auth.addPrivateKey(server.getPrivateKey(), server.getPassphrase());
                selector.add(server.getId(), auth.build());
            }

            return new ConservativeAuthenticationSelector(selector);
        }

        static DefaultRepositorySystemSession NewRepositorySystemSession(RepositorySystem system, Settings settings)
        {
            var session = MavenRepositorySystemUtils.newSession();
            var repository = new LocalRepository(GetDefaultLocalRepoDir(settings));
            session.setLocalRepositoryManager(system.newLocalRepositoryManager(session, repository));
            session.setCache(new DefaultRepositoryCache());
            session.setProxySelector(GetProxySelector(settings));
            session.setMirrorSelector(GetMirrorSelector(settings));
            session.setAuthenticationSelector(GetAuthSelector(settings));
            session.setDependencyGraphTransformer(GetDependencyGraphTransformer());
            session.setTransferListener(new IkvmMavenTransferListener());
            session.setRepositoryListener(new IkvmMavenRepositoryListener());
            return session;
        }

        static DependencyGraphTransformer GetDependencyGraphTransformer()
        {
            // Get default dependency conflict resolution. Note: This can cycle in invalid configurations.
            return new ConflictResolver(
                new NearestVersionSelector(), new JavaScopeSelector(), new SimpleOptionalitySelector(), new JavaScopeDeriver()
            );
        }

        static RemoteRepository NewCentralRepository()
        {
            return new RemoteRepository.Builder(CentralRepostiroyId, CentralRepositoryType, CentralRepositoryUrl).build();
        }

        static List NewRepositories(RepositorySystem system, RepositorySystemSession session, Settings settings)
        {
            // TODO: Read user-defined repos from settings
            return Arrays.asList(NewCentralRepository());
        }
    }
}
