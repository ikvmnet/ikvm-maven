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

using Path = System.IO.Path;

namespace IKVM.Sdk.Maven.Tasks
{

    /// <summary>
    /// Maintains a Maven environment.
    /// </summary>
    class IkvmMavenEnvironment
    {

        /// <summary>
        /// Default internal security dispatcher.
        /// </summary>
        class SecDispatcher : DefaultSecDispatcher
        {

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="configurationFile"></param>
            public SecDispatcher(string configurationFile)
                : base(new DefaultPlexusCipher(), Collections.emptyMap(), configurationFile)
            {

            }

        }

        /// <summary>
        /// Handles errors.
        /// </summary>
        class ErrorHandler : DefaultServiceLocator.ErrorHandler
        {

            /// <summary>
            /// Invoked when service creation fails.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="impl"></param>
            /// <param name="exception"></param>
            public override void serviceCreationFailed(Class type, Class impl, System.Exception exception)
            {

            }

        }

        const string SettingsXml = "settings.xml";
        const string SettingsSecurityXml = "settings-security.xml";
        const string CentralRepositoryUrl = "https://repo.maven.apache.org/maven2/";
        const string CentralRepositoryType = "default";
        const string CentralRepostiroyId = "central";

        static readonly string DefaultRepositoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".m2");

        readonly TaskLoggingHelper logger;
        readonly string repositoryPath;
        readonly Settings settings;
        readonly RepositorySystem repositorySystem;
        readonly RepositorySystemSession repositorySystemSession;
        readonly List repositories;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger"></param>
        public IkvmMavenEnvironment(TaskLoggingHelper logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            repositoryPath = DefaultRepositoryPath;
            settings = ReadSettings();
            repositorySystem = CreateRepositorySystem();
            repositorySystemSession = CreateRepositorySystemSession(repositorySystem, settings);
            repositories = CreateRepositories();
        }

        /// <summary>
        /// Gets the configured <see cref="RepositorySystem"/>.
        /// </summary>
        public RepositorySystem RepositorySystem => repositorySystem;

        /// <summary>
        /// Gets the configured <see cref="RepositorySystemSession"/>.
        /// </summary>
        public RepositorySystemSession RepositorySystemSession => repositorySystemSession;

        /// <summary>
        /// Gets the configured repositories.
        /// </summary>
        public List Repositories => repositories;

        /// <summary>
        /// Reads the Maven settings.
        /// </summary>
        /// <returns></returns>
        Settings ReadSettings()
        {
            // initialize a request for the settings
            var request = new DefaultSettingsBuildingRequest();
            request.setUserSettingsFile(new File(Path.Combine(DefaultRepositoryPath, SettingsXml)));

            var builder = new DefaultSettingsBuilderFactory().newInstance();
            var settings = builder.build(request).getEffectiveSettings();

            // decrypt security sensitive areas of Maven settings
            var securityDispatcher = new SecDispatcher(Path.Combine(repositoryPath, SettingsSecurityXml));
            var settingsDecrypter = new DefaultSettingsDecrypter(securityDispatcher);
            var rlt = settingsDecrypter.decrypt(new DefaultSettingsDecryptionRequest(settings));
            settings.setServers(rlt.getServers());
            settings.setProxies(rlt.getProxies());

            return settings;
        }

        /// <summary>
        /// Gets the default local repository directory path.
        /// </summary>
        /// <returns></returns>
        File GetDefaultLocalRepoDir()
        {
            if (settings.getLocalRepository() != null)
                return new File(settings.getLocalRepository());

            return new File(Path.Combine(DefaultRepositoryPath, "repository"));
        }

        /// <summary>
        /// Creates a new <see cref="org.eclipse.aether.RepositorySystem"/>.
        /// </summary>
        /// <returns></returns>
        RepositorySystem CreateRepositorySystem()
        {
            var locator = MavenRepositorySystemUtils.newServiceLocator();
            locator.addService(typeof(RepositoryConnectorFactory), typeof(BasicRepositoryConnectorFactory));
            locator.addService(typeof(TransporterFactory), typeof(FileTransporterFactory));
            locator.addService(typeof(TransporterFactory), typeof(HttpTransporterFactory));
            locator.setErrorHandler(new ErrorHandler());
            return (RepositorySystem)locator.getService(typeof(RepositorySystem));
        }

        /// <summary>
        /// Creates a new <see cref="ProxySelector"/> based on the currently loaded settings.
        /// </summary>
        /// <returns></returns>
        ProxySelector CreateProxySelector()
        {
            var selector = new DefaultProxySelector();

            foreach (org.apache.maven.settings.Proxy proxy in (IEnumerable)settings.getProxies())
            {
                var builder = new AuthenticationBuilder();
                builder.addUsername(proxy.getUsername()).addPassword(proxy.getPassword());
                selector.add(new org.eclipse.aether.repository.Proxy(
                    proxy.getProtocol(), proxy.getHost(), proxy.getPort(), builder.build()), proxy.getNonProxyHosts());
            }

            return selector;
        }

        /// <summary>
        /// Creates a new <see cref="MirrorSelector"/> based on the currently loaded settings.
        /// </summary>
        /// <returns></returns>
        MirrorSelector CreateMirrorSelector()
        {
            var selector = new DefaultMirrorSelector();

            foreach (Mirror mirror in (IEnumerable)settings.getMirrors())
                selector.add(mirror.getId(), mirror.getUrl(), mirror.getLayout(), false, false, mirror.getMirrorOf(), mirror.getMirrorOfLayouts());

            return selector;
        }

        /// <summary>
        /// Creates a new <see cref="AuthenticationSelector"/> based on the currently loaded settings.
        /// </summary>
        /// <returns></returns>
        AuthenticationSelector CreateAuthenticationSelector()
        {
            var selector = new DefaultAuthenticationSelector();

            foreach (Server server in (IEnumerable)settings.getServers())
            {
                var builder = new AuthenticationBuilder();
                builder.addUsername(server.getUsername()).addPassword(server.getPassword());
                builder.addPrivateKey(server.getPrivateKey(), server.getPassphrase());
                selector.add(server.getId(), builder.build());
            }

            return new ConservativeAuthenticationSelector(selector);
        }

        /// <summary>
        /// Creates a new <see cref="RepositorySystemSession"/> based on the currently loaded settings.
        /// </summary>
        /// <param name="system"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        RepositorySystemSession CreateRepositorySystemSession(RepositorySystem system, Settings settings)
        {
            var session = MavenRepositorySystemUtils.newSession();
            var repository = new LocalRepository(GetDefaultLocalRepoDir());
            session.setLocalRepositoryManager(system.newLocalRepositoryManager(session, repository));
            session.setCache(new DefaultRepositoryCache());
            session.setProxySelector(CreateProxySelector());
            session.setMirrorSelector(CreateMirrorSelector());
            session.setAuthenticationSelector(CreateAuthenticationSelector());
            session.setDependencyGraphTransformer(CreateDependencyGraphTransformer());
            session.setTransferListener(new MavenTransferListener());
            session.setRepositoryListener(new MavenRepositoryListener(logger));
            return session;
        }

        /// <summary>
        /// Creates a new <see cref="DependencyGraphTransformer"/> based on the currently loaded settings.
        /// </summary>
        /// <returns></returns>
        DependencyGraphTransformer CreateDependencyGraphTransformer()
        {
            return new ConflictResolver(new NearestVersionSelector(), new JavaScopeSelector(), new SimpleOptionalitySelector(), new JavaScopeDeriver());
        }

        /// <summary>
        /// Creates a new <see cref="ArtifactRepository"/> representing Maven Central.
        /// </summary>
        /// <returns></returns>
        ArtifactRepository CreateCentralRepository()
        {
            return new RemoteRepository.Builder(CentralRepostiroyId, CentralRepositoryType, CentralRepositoryUrl).build();
        }

        /// <summary>
        /// Creates the set of repositories to be used for resolution.
        /// </summary>
        /// <returns></returns>
        List CreateRepositories()
        {
            return Arrays.asList(CreateCentralRepository());
        }

    }

}
