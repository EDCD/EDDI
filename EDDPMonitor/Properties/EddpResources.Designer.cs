﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EddiEddpMonitor.Properties {
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
    public class EddpResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal EddpResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EddiEddpMonitor.Properties.EddpResources", typeof(EddpResources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to (anything).
        /// </summary>
        public static string anything {
            get {
                return ResourceManager.GetString("anything", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Delete.
        /// </summary>
        public static string delete_btn {
            get {
                return ResourceManager.GetString("delete_btn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Monitor EDDP for changes in system control and state, and generate events that match the watch list..
        /// </summary>
        public static string desc {
            get {
                return ResourceManager.GetString("desc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Faction.
        /// </summary>
        public static string faction_header {
            get {
                return ResourceManager.GetString("faction_header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Max distance from home.
        /// </summary>
        public static string max_dist_home_header {
            get {
                return ResourceManager.GetString("max_dist_home_header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Max distance from ship.
        /// </summary>
        public static string max_dist_ship_header {
            get {
                return ResourceManager.GetString("max_dist_ship_header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EDDP Monitor.
        /// </summary>
        public static string name {
            get {
                return ResourceManager.GetString("name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Name.
        /// </summary>
        public static string name_header {
            get {
                return ResourceManager.GetString("name_header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to New Watch.
        /// </summary>
        public static string new_watch {
            get {
                return ResourceManager.GetString("new_watch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You can create, edit and delete watches below. When EDDI receives information about a change somewhere in the galaxy it will compare it to each of the watches in turn. If the information matches all of the selected criteria for a given watch then it will trigger an event. Please be aware that this relies on information coming from EDDN, which in turn comes from commanders running programs like EDDI, so will only report changes of which the network is aware..
        /// </summary>
        public static string p1 {
            get {
                return ResourceManager.GetString("p1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to State.
        /// </summary>
        public static string state_header {
            get {
                return ResourceManager.GetString("state_header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to System.
        /// </summary>
        public static string system_header {
            get {
                return ResourceManager.GetString("system_header", resourceCulture);
            }
        }
    }
}
