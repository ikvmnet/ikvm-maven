using System;
using System.Collections;

using ikvm.@internal;

using IKVM.Runtime;

using java.io;
using java.lang;
using java.util;

using javax.inject;

using Microsoft.Build.Utilities;

using org.apache.maven.model;
using org.apache.maven.model.building;
using org.apache.maven.repository.@internal;
using org.apache.maven.settings;
using org.apache.maven.settings.building;
using org.apache.maven.settings.crypto;
using org.eclipse.aether;
using org.eclipse.aether.artifact;
using org.eclipse.aether.collection;
using org.eclipse.aether.connector.basic;
using org.eclipse.aether.impl;
using org.eclipse.aether.repository;
using org.eclipse.aether.resolution;
using org.eclipse.aether.spi.connector;
using org.eclipse.aether.spi.connector.transport;
using org.eclipse.aether.spi.locator;
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

        [Named]
        [Singleton]
        public class DefaultArtifactDescriptorReader2 : java.lang.Object, ArtifactDescriptorReader, Service
        {
            private RemoteRepositoryManager remoteRepositoryManager;

            private VersionResolver versionResolver;

            private VersionRangeResolver versionRangeResolver;

            private ArtifactResolver artifactResolver;

            private RepositoryEventDispatcher repositoryEventDispatcher;

            private ModelBuilder modelBuilder;

            public virtual DefaultArtifactDescriptorReader2 setRemoteRepositoryManager(RemoteRepositoryManager remoteRepositoryManager)
            {
                this.remoteRepositoryManager = (RemoteRepositoryManager)Objects.requireNonNull(remoteRepositoryManager, "remoteRepositoryManager cannot be null");
                return this;
            }

            public virtual DefaultArtifactDescriptorReader2 setVersionResolver(VersionResolver versionResolver)
            {
                this.versionResolver = (VersionResolver)Objects.requireNonNull(versionResolver, "versionResolver cannot be null");
                return this;
            }

            public virtual DefaultArtifactDescriptorReader2 setVersionRangeResolver(VersionRangeResolver versionRangeResolver)
            {
                this.versionRangeResolver = (VersionRangeResolver)Objects.requireNonNull(versionRangeResolver, "versionRangeResolver cannot be null");
                return this;
            }

            public virtual DefaultArtifactDescriptorReader2 setArtifactResolver(ArtifactResolver artifactResolver)
            {
                this.artifactResolver = (ArtifactResolver)Objects.requireNonNull(artifactResolver, "artifactResolver cannot be null");
                return this;
            }

            public virtual DefaultArtifactDescriptorReader2 setModelBuilder(ModelBuilder modelBuilder)
            {
                this.modelBuilder = (ModelBuilder)Objects.requireNonNull(modelBuilder, "modelBuilder cannot be null");
                return this;
            }

            public virtual DefaultArtifactDescriptorReader2 setRepositoryEventDispatcher(RepositoryEventDispatcher repositoryEventDispatcher)
            {
                this.repositoryEventDispatcher = (RepositoryEventDispatcher)Objects.requireNonNull(repositoryEventDispatcher, "repositoryEventDispatcher cannot be null");
                return this;
            }

            private Model loadPom(RepositorySystemSession P_0, ArtifactDescriptorRequest P_1, ArtifactDescriptorResult P_2)
            {
                throw new NotSupportedException();
            }

            private void invalidDescriptor(RepositorySystemSession P_0, RequestTrace P_1, Artifact P_2, java.lang.Exception P_3)
            {
                throw new NotSupportedException();
            }

            private int getPolicy(RepositorySystemSession P_0, Artifact P_1, ArtifactDescriptorRequest P_2)
            {
                ArtifactDescriptorPolicy artifactDescriptorPolicy = P_0.getArtifactDescriptorPolicy();
                if (artifactDescriptorPolicy == null)
                {
                    return 0;
                }

                int policy = artifactDescriptorPolicy.getPolicy(P_0, new ArtifactDescriptorPolicyRequest(P_1, P_2.getRequestContext()));
                return policy;
            }

            private void missingDescriptor(RepositorySystemSession P_0, RequestTrace P_1, Artifact P_2, java.lang.Exception P_3)
            {
                throw new NotSupportedException();
            }

            private Properties toProperties(Map P_0)
            {
                Properties properties = new Properties();
                properties.putAll(P_0);
                return properties;
            }

            private Relocation getRelocation(Model P_0)
            {
                Relocation result = null;
                DistributionManagement distributionManagement = P_0.getDistributionManagement();
                if (distributionManagement != null)
                {
                    result = distributionManagement.getRelocation();
                }

                return result;
            }

            public DefaultArtifactDescriptorReader2()
            {
            }

            public DefaultArtifactDescriptorReader2(RemoteRepositoryManager P_0, VersionResolver P_1, VersionRangeResolver P_2, ArtifactResolver P_3, ModelBuilder P_4, RepositoryEventDispatcher P_5)
            {
                setRemoteRepositoryManager(P_0);
                setVersionResolver(P_1);
                setVersionRangeResolver(P_2);
                setArtifactResolver(P_3);
                setModelBuilder(P_4);
                setRepositoryEventDispatcher(P_5);
            }

            public virtual void initService(ServiceLocator locator)
            {
                setRemoteRepositoryManager((RemoteRepositoryManager)locator.getService(ClassLiteral<RemoteRepositoryManager>.Value));
                setVersionResolver((VersionResolver)locator.getService(ClassLiteral<VersionResolver>.Value));
                setVersionRangeResolver((VersionRangeResolver)locator.getService(ClassLiteral<VersionRangeResolver>.Value));
                setArtifactResolver((ArtifactResolver)locator.getService(ClassLiteral<ArtifactResolver>.Value));
                modelBuilder = (ModelBuilder)locator.getService(ClassLiteral<ModelBuilder>.Value);
                if (modelBuilder == null)
                {
                    setModelBuilder(new DefaultModelBuilderFactory().newInstance());
                }

                setRepositoryEventDispatcher((RepositoryEventDispatcher)locator.getService(ClassLiteral<RepositoryEventDispatcher>.Value));
            }

            public virtual ArtifactDescriptorResult readArtifactDescriptor(RepositorySystemSession session, ArtifactDescriptorRequest request)
            {
                ArtifactDescriptorResult artifactDescriptorResult = new ArtifactDescriptorResult(request);
                Model model = loadPom(session, request, artifactDescriptorResult);
                if (model != null)
                {
                    Map configProperties = session.getConfigProperties();
                    ArtifactDescriptorReaderDelegate artifactDescriptorReaderDelegate = (ArtifactDescriptorReaderDelegate)configProperties.get(ClassLiteral<ArtifactDescriptorReaderDelegate>.Value.getName());
                    if (artifactDescriptorReaderDelegate == null)
                    {
                        artifactDescriptorReaderDelegate = new ArtifactDescriptorReaderDelegate();
                    }

                    artifactDescriptorReaderDelegate.populateResult(session, artifactDescriptorResult, model);
                }

                return artifactDescriptorResult;
            }
        }

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

            readonly TaskLoggingHelper log;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="log"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public ErrorHandler(TaskLoggingHelper log)
            {
                this.log = log ?? throw new ArgumentNullException(nameof(log));
            }

            /// <summary>
            /// Invoked when service creation fails.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="impl"></param>
            /// <param name="exception"></param>
            public override void serviceCreationFailed(Class type, Class impl, System.Exception exception)
            {
                log.LogMessage("Service {0} failed with {1}.", type, impl);
                log.LogMessage(exception.Message ?? "");
                log.LogMessage(exception.StackTrace ?? "");
            }

        }

        const string SettingsXml = "settings.xml";
        const string SettingsSecurityXml = "settings-security.xml";
        const string CentralRepositoryUrl = "https://repo.maven.apache.org/maven2/";
        const string CentralRepositoryType = "default";
        const string CentralRepostiroyId = "central";

        static readonly string DefaultRepositoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".m2");

        readonly TaskLoggingHelper log;
        readonly string repositoryPath;
        readonly Settings settings;
        readonly RepositorySystem repositorySystem;
        readonly RepositorySystemSession repositorySystemSession;
        readonly List repositories;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="log"></param>
        public IkvmMavenEnvironment(TaskLoggingHelper log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));

            repositoryPath = DefaultRepositoryPath;
            settings = ReadSettings() ?? throw new NullReferenceException("Null result reading Settings.");
            repositorySystem = CreateRepositorySystem() ?? throw new NullReferenceException("Null result creating RepositorySystem.");
            repositorySystemSession = CreateRepositorySystemSession() ?? throw new NullReferenceException("Null result creating RepositorySystemSession.");
            repositories = CreateRepositories() ?? throw new NullReferenceException("Null result creating Repositories.");
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
            request.setUserSettingsFile(new File(Path.Combine(repositoryPath, SettingsXml)));

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
            //locator.addService(typeof(ArtifactDescriptorReader), typeof(DefaultArtifactDescriptorReader2));
            locator.addService(typeof(RepositoryConnectorFactory), typeof(BasicRepositoryConnectorFactory));
            locator.addService(typeof(TransporterFactory), typeof(FileTransporterFactory));
            locator.addService(typeof(TransporterFactory), typeof(HttpTransporterFactory));
            locator.setErrorHandler(new ErrorHandler(log));
            var c = (RemoteRepositoryManager)locator.getService(typeof(RemoteRepositoryManager));
            if (c == null)
                throw new System.Exception();
            var d = (VersionResolver)locator.getService(typeof(VersionResolver));
            if (d == null)
                throw new System.Exception();
            var e = (VersionRangeResolver)locator.getService(typeof(VersionRangeResolver));
            if (e == null)
                throw new System.Exception();
            var b = (ArtifactResolver)locator.getService(typeof(ArtifactResolver));
            if (b == null)
                throw new System.Exception();
            var a = (ArtifactDescriptorReader)locator.getService(typeof(ArtifactDescriptorReader));
            if (a == null)
                throw new System.Exception();
            var f = (ModelBuilder)locator.getService(typeof(ModelBuilder));
            if (f == null)
                throw new System.Exception();
            var g = (RepositoryEventDispatcher)locator.getService(typeof(RepositoryEventDispatcher));
            if (g == null)
                throw new System.Exception();
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
        /// <returns></returns>
        RepositorySystemSession CreateRepositorySystemSession()
        {
            var session = MavenRepositorySystemUtils.newSession();
            var repository = new LocalRepository(GetDefaultLocalRepoDir());
            session.setLocalRepositoryManager(repositorySystem.newLocalRepositoryManager(session, repository));
            session.setCache(new DefaultRepositoryCache());
            session.setProxySelector(CreateProxySelector());
            session.setMirrorSelector(CreateMirrorSelector());
            session.setAuthenticationSelector(CreateAuthenticationSelector());
            session.setDependencyGraphTransformer(CreateDependencyGraphTransformer());
            session.setTransferListener(new MavenTransferListener());
            session.setRepositoryListener(new MavenRepositoryListener(log));
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
