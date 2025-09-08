#region License
// Author  : Alexander Nava
// Contact : Alexander.Nava.Contact@Gmail.com
// License : For personal use excluding any artificail or machine learning this is licensed under MIT license.
// License : For commercial software(excluding derivative work to make libraries with the same functionality in any language) use excluding any artificail or machine learning this is licensed under MIT license.
// License : If you are a developer making money writing this software it is expected for you to donate, and thus will be given to you for any perpose other than use with Artificial Intelegence or Machine Learning this is licensed under MIT license.
// License : To any Artificial Intelegence or Machine Learning use there is no license given and is forbiden to use this for learning perposes or for anyone requesting you use these libraries, if done so will break the terms of service for this code and you will be held liable.
// License : For libraries or dirivative works that are created based on the logic, patterns, or functionality of this library must inherit all licenses here in.
// License : If you are not sure your use case falls under any of these clauses please contact me through the email above for a license.
#endregion


namespace Levels.Core.IoC {
	#region Usings
	using System;
	using System.Collections.Generic;
	using System.Text;

	using Results;

	using UnityEngine;

	using Sirenix.Utilities;
	#endregion

	public static class Exports<T> {
		public static Settings DefaultSettings;

		private readonly static Dictionary<Scope.ID, IRegisteryTable<T>> _registeries = new();

		public static Dictionary<Scope.ID, IRegisteryTable<T>> Registeries {
			get => _registeries;
		}

		private static StringBuilder _readoutBuilder { get; } = new StringBuilder();

		public static Result<T> Retrieve(object source, Scope.ID scope, Settings localSettings = null, string key = "", params string[] tags) {
			return Retrieve<T>(source, ref scope, localSettings, key, tags);
		}

		public static Result<T> Retrieve<S>(object source, ref Scope.ID scope, Settings localSettings = null, string key = "", params string[] tags) {
			SetupLocalSettings(ref localSettings);
			Debug.Log($"<color=grey>[INFO]</color>[RETRIEVE]<color=green>[START]</color> Exports.Retrieve() : (type={typeof(T).Name}, key='{key}')\n[scope='{scope.ToString()}']\n", source.TryGetComponent()?.gameObject);

#if UNITY_EDITOR // Useful for debuging
			Type SDebug = typeof(S);
			Type TDebug = typeof(T);
#endif

			bool doRetrieveFromParents = localSettings.DoRetrieveFromParents;

			T value = default;
			
			bool isRetrieved = RetrieveFromRegistered<S>(ref value, source, ref scope, localSettings, key, tags)
							|| RetrieveFromParent<S>(ref value, source, ref scope, localSettings, key, tags)
							|| RetrieveDefault<S>(ref value, source);

			if (value is IInjectable into) {
				Debug.Log($"<color=grey>[INFO]</color>[RETRIEVE]<color=olive>[INJECT]</color> Exports.Retrieve() : Injecting into retreived", source.TryGetComponent()?.gameObject);
				into.Inject(scope);
			}

			if (isRetrieved) {
				Debug.Log($"<color=grey>[INFO]</color>[RETRIEVE]<color=#00ffaa>[END]</color> Exports.Retrieve() : (type={typeof(T).Name}, key='{key}') ", source.TryGetComponent()?.gameObject);
				return new Result<T>(value);
			}

			return new KeyNotFoundException($"There was no entry for scope='{scope.Value}' for Type='{typeof(T)}' and tag='{key}'");
		}

		private static void SetupLocalSettings(ref Settings settings) {
			if (settings == null) {
				settings ??= Exports<T>.DefaultSettings;
				settings ??= new Settings();
			}
		}

		private static bool RetrieveFromRegistered<S>(ref T value, object source, ref Scope.ID scope, Settings settings, string key, string[] tags) {
			bool useKeyAsTag = settings.UseKeyAsTag;
			bool tryDefaultKey = settings.UseDefaultKeyOnMissing;
			bool isRetrieved = false;

			if (Registeries.ContainsKey(scope)) // Note: Key as in dictionary key.
			{
				Debug.Log($"<color=grey>[INFO]</color> Exports.RetrieveFromRegistered()");

				var result = Registeries[scope].Retrieve<S>(source, key, useKeyAsTag ? new string[]{ key } : tags).Handle(
					(r) => {
						isRetrieved = true;
						return r;
					}
					, (e) => {
						Debug.LogWarning(e.Message);
						isRetrieved = false;
						return default(T);
					}
				);

				if (isRetrieved) {
					value = result;
					return true;
				}

				if (!tryDefaultKey)
					return false;

				result = Registeries[scope].Retrieve<S>(null, key, useKeyAsTag ? new string[] { key } : tags).Handle(
					(r) => {
						isRetrieved = true;
						return r;
					}, 
					(e) => {
						Debug.LogWarning(e.Message);
						isRetrieved = false;
						return default(T);
					}
				);

				if (isRetrieved) {
					value = result;
					return true;
				}

				return false;
			}

			return false;
		}

		private static bool RetrieveFromParent<S>(ref T value, object source, ref Scope.ID scope, Settings settings, string key, params string[] tags) {
			if (!settings.DoRetrieveFromParents)
				return false;

			bool isRetrieved = false;

			if (scope.GetParentID() != default) {
				Debug.Log($"<color=grey>[INFO]</color>[RETRIEVE] Exports.RetrieveFromParent() : parent={scope.GetParentID().ToString()}");

				var retrieved = Retrieve<S>(source, ref scope.GetParentID(), settings, key, tags).Handle(
					(r) => {
						isRetrieved = true;
						return r;
					},
					(e) => {
						Debug.LogWarning(e.Message);
						isRetrieved = false;
						return default;
					}
				);

				if (isRetrieved) {
					value = retrieved;
					return true;
				}
			}

			return false;
		}

		private static bool RetrieveDefault<S>(
			ref T value, 
			object source) 
		{
			
			// Handle MonoBehavior missing
			var safeIsUnityNull = !source.TryGetComponent().SafeIsUnityNull();
			var isSubclassOf = typeof(T).IsSubclassOf(typeof(MonoBehaviour));
			if (safeIsUnityNull && isSubclassOf) {
				Debug.Log($"<color=grey>[INFO]</color><color=magenta>[CREATE]</color> DI.Exports.RetrieveDefault() : Created component of type {typeof(T).Name} on {source.TryGetComponent().name}");
				
				var value2 = source.TryGetComponent()?.gameObject.AddComponent(typeof(T));
				if (value2 is T result) {
					value = result;
					return true;
				}
			}

			// Handle Class and Value types
			var isClass = typeof(T).IsClass && typeof(T).GetConstructor(Type.EmptyTypes) != null;
			if (isClass) {
				Debug.Log($"<color=grey>[INFO]</color><color=magenta>[CREATE]</color> DI.Exports.RetrieveDefault()");

				value = (T)Activator.CreateInstance(typeof(T));
				return true;
			}
			else if (typeof(T).IsValueType) {
				Debug.Log($"<color=grey>[INFO]</color><color=magenta>[CREATE]</color> DI.Exports.RetrieveDefault()");

				value = default(T);
				return true;
			}

			// Will return false if isClass and doesn't have empty constructor.
			return false;
		}

		public static void Register(Scope.ID scope, IEntry<T> reg) {
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[REG]</color> DI.Exports.Register() : scope='{scope.ToString()}' type='{typeof(T)}'", reg.Key.Source.TryGetComponent()?.gameObject);

			if (!Registeries.ContainsKey(scope)) {
				Registeries.Add(scope, new Registeries<T>(scope, _readoutBuilder));
			}

			Registeries[scope].Register(reg);
		}

		public static void Unregister(Scope.ID scope, IEntry<T> reg) {
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[REG]</color> DI.Exports.Unregister() : scope='{scope.ToString()}' type='{typeof(T)}'", reg.Key.Source.TryGetComponent()?.gameObject);

			if (!Registeries.ContainsKey(scope)) {
				return;
			}

			Registeries[scope].Unregister(reg);
		}

		public static void UnregisterAll(Scope.ID scope, object source) {
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[REG]</color> DI.Exports.Unregister() : scope='{scope.ToString()}' type='{typeof(T)}'", source.TryGetComponent()?.gameObject);

			if (!Registeries.ContainsKey(scope)) {
				return;
			}

			Registeries[scope].UnregisterAll(source);
		}

		public static void TryRegister(Scope.ID scope, object source) {
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[REG]</color> DI.Exports.TryRegister() : scope='{scope.ToString()}' type='{typeof(T)}']", source.TryGetComponent()?.gameObject);

			if (source.GetType().GetInterface(nameof(IEntry<T>)) != null) {
				throw new InvalidCastException($"The reg object passed to Register needs to be of type IRegistery<T>, but was of type {source.GetType()}");
			}

			var regInst = source as IEntry<T>;

			if (!Registeries.ContainsKey(scope)) {
				Registeries.Add(scope, new Registeries<T>(scope, _readoutBuilder));
			}

			Registeries[scope].Register(regInst);
		}
	}

	
	public static class StringIDScope_Extends {
		public static Scope.ID nullRef;

		public static ref Scope.ID GetParentID(this Scope.ID id) {
			var scope = IoC.Scope.References.GetScope(id);

			if (scope == null || scope.Parent == null) {
				Debug.LogError("<color=red>[WARN]</color>[DI] StringIDScope_Extends.GetParentID() : Passed ID is null, defaulting to ID.Null");
				
				nullRef = Scope.ID.Null();
				return ref nullRef;
			}

			return ref scope.Parent;
		}

		public static IoC.Scope GetScope(this Scope.ID id) {
			return IoC.Scope.References.GetScope(id);
		}
	};
}

#region Changelog
/// <changelog>
///		<change>
///			<author></author>
///			<id></id>
///			<comment></comment>
///		</change>
/// </changelog>
#endregion