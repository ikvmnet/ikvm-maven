﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IKVM.Maven.Sdk.Tasks.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SR {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SR() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("IKVM.Maven.Sdk.Tasks.Resources.SR", typeof(SR).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0003: Invalid Maven artifact ID value &apos;{0}&apos;..
        /// </summary>
        internal static string Error_MavenInvalidArtifactId {
            get {
                return ResourceManager.GetString("Error.MavenInvalidArtifactId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0001: Invalid Maven coordinate value &apos;{0}&apos;..
        /// </summary>
        internal static string Error_MavenInvalidCoordinates {
            get {
                return ResourceManager.GetString("Error.MavenInvalidCoordinates", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0002: Invalid Maven group ID value &apos;{0}&apos;..
        /// </summary>
        internal static string Error_MavenInvalidGroupId {
            get {
                return ResourceManager.GetString("Error.MavenInvalidGroupId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0009: Invalid Maven scope &apos;{1}&apos; on &apos;{0}&apos;..
        /// </summary>
        internal static string Error_MavenInvalidScope {
            get {
                return ResourceManager.GetString("Error.MavenInvalidScope", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0004: Invalid Maven version value &apos;{0}&apos;..
        /// </summary>
        internal static string Error_MavenInvalidVersion {
            get {
                return ResourceManager.GetString("Error.MavenInvalidVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0006: Missing Maven artifact ID..
        /// </summary>
        internal static string Error_MavenMissingArtifactId {
            get {
                return ResourceManager.GetString("Error.MavenMissingArtifactId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0005: Missing Maven group ID..
        /// </summary>
        internal static string Error_MavenMissingGroupId {
            get {
                return ResourceManager.GetString("Error.MavenMissingGroupId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0008: Missing Maven scope on &apos;{0}&apos;..
        /// </summary>
        internal static string Error_MavenMissingScope {
            get {
                return ResourceManager.GetString("Error.MavenMissingScope", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0007: Missing Maven version..
        /// </summary>
        internal static string Error_MavenMissingVersion {
            get {
                return ResourceManager.GetString("Error.MavenMissingVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0010: Transfer corrupted from &apos;{0}&apos;..
        /// </summary>
        internal static string Error_MavenTransferCorrupted {
            get {
                return ResourceManager.GetString("Error.MavenTransferCorrupted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0011: Transfer failed from &apos;{0}&apos;..
        /// </summary>
        internal static string Error_MavenTransferFailed {
            get {
                return ResourceManager.GetString("Error.MavenTransferFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAVEN0012: Cyclic reference detected from &apos;{0}&apos; to &apos;{1}&apos;.
        ///.
        /// </summary>
        internal static string Warning_MavenIgnoreCyclicReference {
            get {
                return ResourceManager.GetString("Warning.MavenIgnoreCyclicReference", resourceCulture);
            }
        }
    }
}
