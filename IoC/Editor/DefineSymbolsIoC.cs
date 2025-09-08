#if UNITY_EDITOR

namespace Levels.EditorTools {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    [InitializeOnLoad]
    public static class DefineSymbolsIoC {
        static DefineSymbolsIoC() {
            if (AssetDatabase.IsAssetImportWorkerProcess()) return;
            if (Application.isBatchMode) return;
            if (SessionState.GetBool("Levels.DefineSymbols.Init", false)) return;
            SessionState.SetBool("Levels.DefineSymbols.Init", true);
            TryAdd("Levels_Zero");
        }

        public static void TryAdd(string symbol) {
            if (string.IsNullOrWhiteSpace(symbol)) return;
            if (AssetDatabase.IsAssetImportWorkerProcess()) return;
            if (Application.isBatchMode) return;

            foreach (var t in GetValidTargets()) {
                var set = ToSet(GetDefines(t));
                if (set.Add(symbol))
                    SetDefines(t, string.Join(";", set));
            }
        }

        public static void TryRemove(string symbol) {
            if (string.IsNullOrWhiteSpace(symbol)) return;
            if (AssetDatabase.IsAssetImportWorkerProcess()) return;
            if (Application.isBatchMode) return;

            foreach (var t in GetValidTargets()) {
                var set = ToSet(GetDefines(t));
                if (set.Remove(symbol))
                    SetDefines(t, string.Join(";", set));
            }
        }

        public static bool Exists(string symbol) {
            if (string.IsNullOrWhiteSpace(symbol)) return false;
            if (AssetDatabase.IsAssetImportWorkerProcess()) return false;
            if (Application.isBatchMode) return false;
            return GetValidTargets().Any(t => ToSet(GetDefines(t)).Contains(symbol));
        }

        private static IEnumerable<object> GetValidTargets() {
            #if UNITY_2021_2_OR_NEWER
            foreach (BuildTargetGroup g in Enum.GetValues(typeof(BuildTargetGroup))) {
                if (g == BuildTargetGroup.Unknown) continue;

                NamedBuildTarget nbt;
                try { nbt = NamedBuildTarget.FromBuildTargetGroup(g); }
                catch { continue; }

                if (string.IsNullOrEmpty(nbt.ToString())) continue;

                try { _ = PlayerSettings.GetScriptingDefineSymbols(nbt); }
                catch { continue; }

                yield return nbt;
            }
            #else
            foreach (BuildTargetGroup g in Enum.GetValues(typeof(BuildTargetGroup))) {
                if (g == BuildTargetGroup.Unknown) continue;
                if (IsObsolete(g)) continue;
                if (IsValid(g)) yield return g;
            }
            #endif
        }

        #if UNITY_2021_2_OR_NEWER
        private static string GetDefines(object target)
            => PlayerSettings.GetScriptingDefineSymbols((NamedBuildTarget)target);

        private static void SetDefines(object target, string defines)
            => PlayerSettings.SetScriptingDefineSymbols((NamedBuildTarget)target, defines);
        #else
        private static bool IsObsolete(BuildTargetGroup g) {
            var m = typeof(BuildTargetGroup).GetMember(g.ToString());
            return m.Length > 0 && Attribute.IsDefined(m[0], typeof(ObsoleteAttribute));
        }

        private static bool IsValid(BuildTargetGroup g) {
            try {
                _ = NamedBuildTarget.FromBuildTargetGroup(g);
                return true;
            }
            catch { return false; }
        }

        private static string GetDefines(object target)
            => PlayerSettings.GetScriptingDefineSymbolsForGroup((BuildTargetGroup)target);

        private static void SetDefines(object target, string defines)
            => PlayerSettings.SetScriptingDefineSymbolsForGroup((BuildTargetGroup)target, defines);
        #endif

        private static HashSet<string> ToSet(string defines) {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(defines)) return set;

            foreach (var d in defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                var t = d.Trim();
                if (t.Length > 0) set.Add(t);
            }

            return set;
        }

        [MenuItem("Tools/Defines (Levels)/Log All")]
        private static void LogAll() {
            if (AssetDatabase.IsAssetImportWorkerProcess()) return;
            if (Application.isBatchMode) return;

            foreach (var t in GetValidTargets()) {
                var name =
                    #if UNITY_2021_2_OR_NEWER
                    ((NamedBuildTarget)t).ToString();
                #else
                    ((BuildTargetGroup)t).ToString();
                #endif
                UnityEngine.Debug.Log($"{name}: {GetDefines(t)}");
            }
        }
    }
}
#endif
