using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Logging;

namespace ArtifactsOfMight.Logger
{
    /// <summary>
    /// An abstraction on top of the logging system so we can enable/disable specific namespaces/classes
    /// </summary>
    internal static class ScoppedLogger
    {
        private static ManualLogSource _root;
        private static readonly HashSet<string> enabledScopes = new(StringComparer.Ordinal);
        private static bool defaultEnabled = true;

        /// <summary>
        /// Initialize the logger system with bepinex's log
        /// </summary>
        /// <param name="root"></param>
        internal static void Init(ManualLogSource root)
        {
            _root = root;
        }

        internal static void LogEnabledScopes()
        {
            if (_root == null)
            {
                return;
            }

            _root.LogInfo("=== [ArtifactsOfMight] Log Scope Configuration ===");

            if (defaultEnabled)
            {
                _root.LogInfo("Default: ENABLED for all scopes");
                return;
            }


            if (enabledScopes.Count == 0)
            {
                _root.LogInfo("Default: DISABLED for all scopes (no overrides)");
                return;
            }

            _root.LogInfo("Default: DISABLED except for the following scopes:");
            foreach (var scope in enabledScopes)
            {
                _root.LogInfo($"  • {scope}");
            }
        }

        internal static void EnableAll() => defaultEnabled = true;
        internal static void DisableAll() => defaultEnabled = false;
        internal static void EnableScope(string scope) => enabledScopes.Add(scope);
        internal static void DisableScope(string scope) => enabledScopes.Remove(scope);
        internal static void Only(params string[] scopes)
        {
            defaultEnabled = false;
            enabledScopes.Clear();
            foreach (var s in scopes)
            {
                enabledScopes.Add(s);
            }
        }

        /// <summary>
        /// Create a scoped logger for the given class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static Scoped For<T>() => new Scoped(_root, typeof(T).FullName ?? typeof(T).Name);

        /// <summary>
        /// Create a scope logger for the desired name
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        internal static Scoped For(string scope) => new Scoped(_root, scope);


        internal static void Info(object data) => _root.LogInfo(data);
        internal static void Warn(object data) => _root.LogWarning(data);
        internal static void Error(object data) => _root.LogError(data);

        internal static bool IsEnabled(string scope)
        {
            if (defaultEnabled)
            {
                return true;
            }

            // lets say we enabled a namespace, and not just a class
            // this should count the class as enabled
            foreach (var es in enabledScopes)
            {
                if (scope.StartsWith(es, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }


        internal readonly struct Scoped
        {
            private readonly ManualLogSource src;
            private readonly string scope;

            public Scoped(ManualLogSource src, string scope)
            {
                this.src = src;

                // trim to last part so a long namespace:namespace:namespace ends up as just a class name
                this.scope = scope = scope.Contains('.') ? scope[(scope.LastIndexOf('.') + 1)..] : scope;
            }

            // Add member name automatically for breadcrumbs
            public void Debug(object msg, [CallerMemberName] string member = "")
            {
                if (IsEnabled(scope))
                {
                    src.LogDebug($"[{scope}::{member}] {msg}");
                }
            }

            public void Info(object msg, [CallerMemberName] string member = "")
            {
                if (IsEnabled(scope))
                {
                    src.LogInfo($"[{scope}::{member}] {msg}");
                }
            }

            public void Warn(object msg, [CallerMemberName] string member = "")
            {
                if (IsEnabled(scope))
                {
                    src.LogWarning($"[{scope}::{member}] {msg}");
                }
            }

            public void Error(object msg, [CallerMemberName] string member = "")
            {
                if (IsEnabled(scope))
                {
                    src.LogError($"[{scope}::{member}] {msg}");
                }
            }
        }
    }
}
