﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using java.io;
using java.lang;
using java.util;

using Microsoft.Build.Utilities;

using org.apache.maven.repository.@internal;
using org.apache.maven.settings;
using org.apache.maven.settings.building;
using org.apache.maven.settings.crypto;
using org.eclipse.aether;
using org.eclipse.aether.connector.basic;
using org.eclipse.aether.impl;
using org.eclipse.aether.repository;
using org.eclipse.aether.spi.connector;
using org.eclipse.aether.spi.connector.transport;
using org.eclipse.aether.transport.file;
using org.eclipse.aether.transport.http;
using org.eclipse.aether.util.graph.selector;
using org.eclipse.aether.util.graph.transformer;
using org.eclipse.aether.util.repository;
using org.sonatype.plexus.components.cipher;
using org.sonatype.plexus.components.sec.dispatcher;

using Path = System.IO.Path;

namespace IKVM.Maven.Sdk.Tasks
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
            public SecDispatcher(string configurationFile)f
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
                if (exception == null)
                    log.LogError("Service {0} failed from {1}.", type, impl);
                else
                    log.LogErrorFromException(exception, true, true, null);
            }

        }

        const string SettingsXml = "settings.xml";
        const string SettingsSecurityXml = "settings-security.xml";
        const string DefaultRepositoryType = "default";

        static readonly string DefaultRepositoryPath = Path.Combine(java.lang.System.getProperty("user.home"), ".m2");

        readonly TaskLoggingHelper log;
        readonly string repositoryPath;
        readonly Settings settings;
        readonly RepositorySystem repositorySystem;
        readonly List repositories;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="repositories"></param>
        /// <param name="log"></param>
        public IkvmMavenEnvironment(IList<MavenRepositoryItem> repositories, TaskLoggingHelper log)
        {
            if (repositories is null)
                throw new ArgumentNullException(nameof(repositories));

            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.repositoryPath = DefaultRepositoryPath;
            this.settings = ReadSettings() ?? throw new NullReferenceException("Null result reading Settings.");
            this.repositorySystem = CreateRepositorySystem() ?? throw new NullReferenceException("Null result creating RepositorySystem.");
            this.repositories = Arrays.asList(repositories.Select(i => CreateRepository(i)).ToArray());
        }

        /// <summary>
        /// Gets the configured <see cref="RepositorySystem"/>.
        /// </summary>
        public RepositorySystem RepositorySystem => repositorySystem;

        /// <summary>
        /// Gets the configured repositories.
        /// </summary>
        public List Repositories => repositories;

        /// <summary>
        /// Handles a settings problem.
        /// </summary>
        /// <param name="problem"></param>
        void HandleSettingsProblem(SettingsProblem problem)
        {
            if (problem.getSeverity() == SettingsProblem.Severity.WARNING)
                log.LogWarning(problem.getMessage());
            else if (problem.getSeverity() == SettingsProblem.Severity.ERROR)
                log.LogError(problem.getMessage());
            else if (problem.getSeverity() == SettingsProblem.Severity.FATAL)
                log.LogCriticalMessage(null, null, null, null, 0, 0, 0, 0, problem.getMessage(), null);
        }

        /// <summary>
        /// Reads the Maven settings.
        /// </summary>
        /// <returns></returns>
        Settings ReadSettings()
        {
            var request = new DefaultSettingsBuildingRequest();
            request.setUserSettingsFile(new File(Path.Combine(repositoryPath, SettingsXml)));

            var builder = new DefaultSettingsBuilderFactory().newInstance();
            var settingsResult = builder.build(request);
            if (settingsResult.getProblems() is List settingsProblem)
                for (var i = settingsProblem.iterator(); i.hasNext();)
                    HandleSettingsProblem((SettingsProblem)i.next());

            // get currently effective settings
            var settings = settingsResult.getEffectiveSettings();

            // use settings-security.xml to decrypt loaded settings
            var secDispatcher = new SecDispatcher(Path.Combine(repositoryPath, SettingsSecurityXml));
            var secDecrypter = new DefaultSettingsDecrypter(secDispatcher);
            var secResult = secDecrypter.decrypt(new DefaultSettingsDecryptionRequest(settings));
            if (secResult.getProblems() is List secProblems)
                for (var i = secProblems.iterator(); i.hasNext();)
                    HandleSettingsProblem((SettingsProblem)i.next());

            // apply decrypted settings
            settings.setServers(secResult.getServers());
            settings.setProxies(secResult.getProxies());

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
            locator.setErrorHandler(new ErrorHandler(log));
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
                selector.add(new org.eclipse.aether.repository.Proxy(proxy.getProtocol(), proxy.getHost(), proxy.getPort(), builder.build()), proxy.getNonProxyHosts());
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
        public RepositorySystemSession CreateRepositorySystemSession(bool noError = false)
        {
            var session = MavenRepositorySystemUtils.newSession();
            session.setLocalRepositoryManager(repositorySystem.newLocalRepositoryManager(session, new LocalRepository(GetDefaultLocalRepoDir())));
            session.setCache(new DefaultRepositoryCache());
            session.setProxySelector(CreateProxySelector());
            session.setMirrorSelector(CreateMirrorSelector());
            session.setAuthenticationSelector(CreateAuthenticationSelector());
            session.setDependencySelector(new AndDependencySelector(new ScopeDependencySelector("test"), new OptionalDependencySelector(), new ExclusionDependencySelector()));
            session.setTransferListener(new MavenTransferListener(log, noError));
            session.setRepositoryListener(new MavenRepositoryListener(log));
            session.setConfigProperty(ConflictResolver.CONFIG_PROP_VERBOSE, "true");
            return session;
        }

        /// <summary>
        /// Creates a new <see cref="ArtifactRepository"/> representing Maven Central.
        /// </summary>
        /// <returns></returns>
        ArtifactRepository CreateRepository(MavenRepositoryItem repository)
        {
            return new RemoteRepository.Builder(repository.Id, DefaultRepositoryType, repository.Url).build();
        }

    }

}
