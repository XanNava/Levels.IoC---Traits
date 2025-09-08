namespace Levels.Unity.IoC {
	#region Usings
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Core;
	using Core.IoC;

	using Sirenix.OdinInspector;

	using UnityEngine;
	using UnityEngine.ResourceManagement.ResourceProviders;
	using UnityEngine.SceneManagement;

	using UnityExtends.SceneManagement;
	#endregion

	[DefaultExecutionOrder(31999)]
	public class _Scope_ : MonoBehaviour, IWrap<Scope> {
		Inject<Scope> IInject<Scope>.In1 { get => _source; set => _source = value; }
		private Inject<Scope> _source = new();

		[BoxGroup("IN"), SerializeField]
		public RegisterObject[] EditorRegistery;

		[BoxGroup("SETTINGS")]
		public string scopeName = "";
		[BoxGroup("SETTINGS")]
		public bool isRoot;
		[BoxGroup("SETTINGS")]
		public bool defineEndSeperately;
		[ShowIf("@defineEndSeperately && !isRoot")]
		public _ScopeEnd_ scopeEnd;

		// NEW: container for visualizing registered items
		[BoxGroup("SETTINGS")]
		[Tooltip("Name of the parent GameObject that will hold one empty child per IRegister / IWrap<IRegister> that gets registered.")]
		[SerializeField]
		private string registeredRootName = "Registered";
		private Transform _registeredRoot; // created on demand

		[BoxGroup("OUT"), HideLabel, InlineProperty]
		public EVENTS events;

		[Serializable]
		public class EVENTS {
			public _Action_ OnScopeCreated;
		}

		[BoxGroup("DEBUG")]
		public bool Debug_Injectables;

		private readonly List<IWrap> wrappers = new();
		private readonly List<IRegister> registers = new();

		public void Awake() {
			Debug.Log($"<color=grey>[INFO]</color>[SETUP]<color=green>[START]</color> _Scope_.Awake() : [scope='{scopeName}'][isRoot='{isRoot}']", gameObject);
			_setupSource();
			_registerThis();
			_setupEndScope();
			_setHooks();
			Debug.Log($"<color=grey>[INFO]</color>[SETUP]<color=#00ffaa>[END]</color> _Scope_.Awake() : [scope='{scopeName}']", gameObject);
		}

		#region Awake.METHODS
		private void _registerThis() {
			Debug.Log($"<color=grey>[INFO]</color>[SETUP] _Scope_._registerThis()", gameObject);
			Exports<UnityEngine.GameObject>.Register(_source.Get().ScopeID, Entry.Factory.Service(gameObject, "", this));
			Exports<Transform>.Register(_source.Get().ScopeID, Entry.Factory.Service(transform, "", this));
			Exports<Scope.ID>.Register(_source.Get().ScopeID, Entry.Factory.Service(_source.Get().ScopeID, "", this));
			Exports<Scope>.Register(_source.Get().ScopeID, Entry.Factory.Transient((source, scope, key, type) => new Scope(""), "", this));
		}

		private void _setupSource() {
			if (isRoot) {
				_source.Set(Scope.References.GetScope(Scope.Root));
			} else {
				var scope = Scope.Factory.CreateChildScope(scopeName, GetParentScope());
				_source.Set(scope);
			}

			events?.OnScopeCreated?.Invoke();
		}

		public Scope.ID GetParentScope() {
			if (isRoot) return Scope.ID.Null();

			Transform parent = transform.parent;

			while (parent != null) {
				var holder = parent.GetComponent<IWrap<Scope>>();
				if (holder != null) return holder.Get().Scope;
				parent = parent.parent;
			}

			return Scope.Root;
		}

		private void _setupEndScope() {
			_ScopeEnd_ end = null;

			if (defineEndSeperately && isRoot) {
				for (int i = 0; i < SceneManager.sceneCount; i++) {
					var scene = SceneManager.GetSceneAt(i);
					if (scene == this.gameObject.scene) continue;

					foreach (var rootObject in scene.GetRootGameObjects()) {
						var se = rootObject.GetComponent<_ScopeEnd_>();

						if (se != null && se.isRootEnd) {
							end = se;
							break;
						}
					}

					if (end != null) break;
				}
			} else if (defineEndSeperately && !isRoot && scopeEnd != null) {
				end = scopeEnd;
			} else {
				end = GetComponent<_ScopeEnd_>();
			}

			if (end == null)
				Debug.LogError($"<color=red>[WARN]</color>[SETUP] _Scope_._setupEndScope() : No end scope found!", gameObject);

			end?.SetScope(this);
			scopeEnd = end;
		}

		private void _setHooks() {
			scopeEnd.AwakeCallback += AwakeEnd;
			World.Scene.Management.Events.OnSceneActivating += HandleOnSceneActivating;
			World.Scene.Management.Events.OnSceneActivated += HandleOnSceneActivated;
		}
		#endregion

		public void AwakeEnd() {
			Debug.Log(
				$"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color><color=green>[START]</color> _ScopeEnd_.Start() : [scope='{this.Get().Scope.ToString()}'][isRoot='{isRoot}']",
				gameObject
			);
			_registerEditorServices();

			if (isRoot) {
				_interateScenes();
			} else {
				_iterateChildrenComponents();
			}

			_registerFoundServices();
			Debug.Log($"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color><color=#00ffaa>[END]</color> _ScopeEnd_.Start() : [scope='{this.Get().Key}']", gameObject);
		}

		public void Start() {
			Debug.Log(
				$"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color><color=green>[START]</color> _ScopeEnd_.AwakeEnd() : [scope='{this.Get().Scope.ToString()}'][isRoot='{isRoot}']",
				gameObject
			);

			if (isRoot) _iterateSceneRootGO();
			else _injectChildren();

			Debug.Log(
				$"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color><color=#00ffaa>[END]</color> _ScopeEnd_.AwakeEnd() : [scope='{this.Get().Key}']",
				gameObject
			);
		}

		#region Helpers (existing)
		private void _interateScenes() {
			for (int i = 0; i < SceneManager.sceneCount; i++)
				_collectRegisterablesInScene(SceneManager.GetSceneAt(i));
		}

		private void _registerEditorServices() {
			Debug.Log($"<color=grey>[INFO]</color>[SETUP] _Scope_._registerEditorServices()", gameObject);

			foreach (var service in EditorRegistery) {
				if (service.Obj == null) continue;

				Type serviceType = service.Obj.GetType();
				Type exportClosedType = typeof(Exports<>).MakeGenericType(serviceType);

				Type entryClosed = typeof(ServiceEntry<>).MakeGenericType(serviceType);
				var ctor = entryClosed.GetConstructor(new[] { typeof(EntryKey), serviceType });

				object entryInstance = null;
				var servObj = service.Obj;

				try {
					if (ctor != null) {
						entryInstance = ctor.Invoke(
							new object[]
							{
								new EntryKey(
									service.Tag.ToString(),
									servObj is not GameObject ? ((Component)servObj).gameObject : (GameObject)servObj
								),
								servObj
							}
						);
					} else {
						Debug.LogError("<color=red>[ERROR]</color> _Scope_._registerEditorServices() : Generic ServiceEntry could not be instantiated.", gameObject);
					}
				}
				catch (Exception ex) {
					Debug.LogError($"<color=red>[ERROR]</color> _Scope_._registerEditorServices() : {serviceType} for {servObj}", gameObject);
					Debug.Log(ex);
				}

				var registerMethod = exportClosedType.GetMethod("Register", new[] { typeof(IScope), typeof(object) });

				if (registerMethod != null) registerMethod.Invoke(null, new object[] { _source, entryInstance });
				else Debug.LogError("<color=red>[ERROR]</color> _Scope_._registerEditorServices() : Generic registerMethod was null.", gameObject);
			}
		}

		private void _iterateChildrenComponents() {
			var foundWraps = GetComponentsInChildren<IWrap>(true);
			for (int i = 0; i < foundWraps.Length; i++) {
				var c = foundWraps[i] as Component;
				if (c == null) continue;
				var owner = c.GetComponentInParent<Levels.Unity.IoC._Scope_>();
				if (owner == null || owner == this) wrappers.Add(foundWraps[i]);
			}

			var foundRegs = GetComponentsInChildren<IRegister>(true);
			for (int i = 0; i < foundRegs.Length; i++) {
				var c = foundRegs[i] as Component;
				if (c == null) continue;
				var owner = c.GetComponentInParent<Levels.Unity.IoC._Scope_>();
				if (owner == null || owner == this) registers.Add(foundRegs[i]);
			}
		}


		private void _iterateSceneRootGO() {
			for (int i = 0; i < SceneManager.sceneCount; i++) {
				var scene = SceneManager.GetSceneAt(i);
				Debug.Log($"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color> _ScopeEnd_._iterateSceneRootGO() : scene={scene.name}");
				_injectGameObjects(scene.GetRootGameObjects());
			}
		}

		private void _injectChildren() {
			List<IInjectable> wrappedInjectables;
			_parseIntoWrappers(out wrappedInjectables);

			var injectables = GetComponentsInChildren<IInjectable>(true);
			injectables.Remove(this);

			if (Debug_Injectables) {
				foreach (var injectable in injectables)
					Debug.Log(injectable.GetType());
			}

			var scope = this.Get().Get();
			scope.InjectAll(injectables);
			scope.InjectAll(wrappedInjectables.ToArray());

			var notifiables = GetComponentsInChildren<IListen>(true);
			var verbose = string.Join(" , ", notifiables.Select(n => n.ToString()));
			Debug.Log($"<color=grey>[INFO]</color>[REGISTER][NOTIF] _ScopeEnd_._injectChildren() : [scope={this.name}] \n {verbose}");
		}

		private void _parseIntoWrappers(out List<IInjectable> wrappedInjections) {
			wrappedInjections = new List<IInjectable>();
			var possible = GetComponentsInChildren<IWrap>(true);

			foreach (var wrap in possible) {
				if (wrap.GetWrappedType().GetInterface(nameof(IInjectable)) != null) {
					wrappedInjections.Add(wrap.GetWrapped() as IInjectable);
				}
			}
		}

		private void _injectGameObjects(UnityEngine.GameObject[] gameObjects) {
			Debug.Log($"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color> _ScopeEnd_._injectGameObjects() ", gameObject);

			for (int r = 0; r < gameObjects.Length; r++) {
				var rootObject = gameObjects[r];

				if (rootObject.GetComponent<Levels.Unity.IoC._Scope_>() != null) {
					Debug.Log(
						$"<color=grey>[INFO]</color><color=olive>[INJECT]</color>[IsInjectable=NO(Scoped)] _ScopeEnd_._injectGameObjects() : rootObject={rootObject.name}",
						rootObject
					);
					continue;
				}

				var rawInjectables = rootObject.GetComponentsInChildren<IInjectable>(true);
				var injectablesList = new List<IInjectable>(rawInjectables.Length);

				for (int i = 0; i < rawInjectables.Length; i++) {
					var c = rawInjectables[i] as Component;
					if (c == null) continue;

					var owner = c.GetComponentInParent<Levels.Unity.IoC._Scope_>();

					if (owner == null || owner == this) {
						injectablesList.Add(rawInjectables[i]);
					}
				}

				var rawWrappers = rootObject.GetComponentsInChildren<IWrap>(true);
				var wrappersList = new List<IWrap>(rawWrappers.Length);

				for (int i = 0; i < rawWrappers.Length; i++) {
					var c = rawWrappers[i] as Component;
					if (c == null) continue;

					var owner = c.GetComponentInParent<Levels.Unity.IoC._Scope_>();

					if (owner == null || owner == this) {
						wrappersList.Add(rawWrappers[i]);
					}
				}

				Debug.Log(
					$"<color=grey>[INFO]</color><color=olive>[INJECT]</color> _ScopeEnd_._injectGameObjects() : rootObject={rootObject.name}, IsInjectable={(injectablesList.Count > 0 ? "YES" : "NO")}",
					rootObject
				);

				// Inject direct injectables
				this.Get().Get().InjectAll(injectablesList.ToArray());

				// Inject wrapped injectables
				for (int i = 0; i < wrappersList.Count; i++) {
					var wrap = wrappersList[i];
					var wrappedType = wrap.GetWrappedType();
					if (wrappedType == null) continue;

					// Only if the wrapped implements IInjectable
					if (wrappedType.GetInterface(nameof(IInjectable)) != null) {
						var wrapped = wrap.GetWrapped();

						if (wrapped is IInjectable inj) {
							inj.Inject(this.Get().Scope);
						}
					}
				}
			}
		}

		private void _collectRegisterablesInScene(Scene scene) {
			Debug.Log($"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color> _ScopeEnd_._collectRegisterablesInScene() ", gameObject);

			var rootObjects = scene.GetRootGameObjects();
			for (int r = 0; r < rootObjects.Length; r++) {
				var rootObject = rootObjects[r];
				if (rootObject.GetComponent<Levels.Unity.IoC._Scope_>() != null)
					continue;

				var foundWraps = rootObject.GetComponentsInChildren<IWrap>(true);
				for (int i = 0; i < foundWraps.Length; i++) {
					var c = foundWraps[i] as Component;
					if (c == null) continue;
					var owner = c.GetComponentInParent<Levels.Unity.IoC._Scope_>();
					if (owner == null || owner == this) wrappers.Add(foundWraps[i]);
				}

				var foundRegs = rootObject.GetComponentsInChildren<IRegister>(true);
				for (int i = 0; i < foundRegs.Length; i++) {
					var c = foundRegs[i] as Component;
					if (c == null) continue;
					var owner = c.GetComponentInParent<Levels.Unity.IoC._Scope_>();
					if (owner == null || owner == this) registers.Add(foundRegs[i]);
				}
			}
		}

		#endregion

		// NEW: ensure/create the Registered container under this scope object
		private Transform EnsureRegisteredRoot() {
			if (_registeredRoot != null) return _registeredRoot;

			// try find existing child by name
			var t = transform.Find(registeredRootName);

			if (t == null) {
				var go = new GameObject(string.IsNullOrWhiteSpace(registeredRootName) ? "Registered" : registeredRootName);
				t = go.transform;
				t.SetParent(transform, false);
				t.localPosition = Vector3.zero;
				t.localRotation = Quaternion.identity;
				t.localScale = Vector3.one;
			}

			_registeredRoot = t;
			return _registeredRoot;
		}

		// NEW: helper to add a child node under Registered for a given source name
		private void CreateRegisteredNode(string sourceName) {
			var root = EnsureRegisteredRoot();
			var name = string.IsNullOrEmpty(sourceName) ? "Unnamed" : sourceName;
			var child = new GameObject(name);
			child.transform.SetParent(root, false);
			child.transform.localPosition = Vector3.zero;
			child.transform.localRotation = Quaternion.identity;
			child.transform.localScale = Vector3.one;
		}

		private void _registerFoundServices() {
			Debug.Log($"<color=grey>[INFO]</color>[SETUP] _Scope_._registerFoundServices() ", gameObject);

			// DIRECT IRegister
			foreach (var register in registers) {
				if (register == null) {
					Debug.LogError(
						"<color=red>[ERROR]</color>[SETUP] _Scope_._registerFoundServices() : null register on scope='" + _source.Get().Name + "' at index=" +
						registers.FindIndex((x) => x == null)
					);
					continue;
				}

				var comp = register.TryGetComponent();
				var go = comp ? comp.gameObject : null;

				Debug.Log($"<color=grey>[INFO]</color><color=blue>[REGISTER]</color> _Scope_._registerFoundServices() : sourceName={go?.name} type={register.GetType()}", go);
				register.Register(_source.Get().ScopeID);

				// NEW: create an empty node named after the source GO (or type)
				CreateRegisteredNode(go != null ? go.name : register.GetType().Name);
			}

			registers.Clear();

			// WRAPPED IRegister
			foreach (var wrap in wrappers) {
				if (wrap.GetWrappedType().GetInterface(nameof(IRegister)) == null) continue;

				var wrp = wrap.GetWrapped();

				if (wrp == null) {
					Debug.LogError(
						"<color=red>[WARN]</color>[REG] _Scope_._registerFoundServices() : Wrapper object is null type=" + wrap.GetWrappedType() + " wrapper.GetType()=" +
						wrap.GetType(),
						((Component)wrap).gameObject
					);
					continue;
				}

				if (!wrp.GetType().GetInterfaces().Contains(typeof(IRegister))) {
					Debug.LogError(
						"<color=red>[WARN]</color>[REG] _Scope_._registerFoundServices() : Wrapped object is not IRegister, type=" + wrap.GetWrappedType() +
						" wrp.GetType()=" + wrp.GetType(),
						((Component)wrap).gameObject
					);
					continue;
				}

				var asReg = (IRegister)wrp;
				var srcComp = asReg.TryGetComponent();
				var srcGO = srcComp ? srcComp.gameObject : ((Component)wrap)?.gameObject;

				Debug.Log($"<color=grey>[INFO]</color><color=blue>[REGISTER]</color> _Scope_._registerFoundServices() : {wrp} ", srcGO);
				asReg.Register(_source.Get().ScopeID);

				// NEW: create an empty node named after the wrapper's GO (fallback to wrapped GO/type)
				string nodeName = srcGO != null ? srcGO.name : wrp.GetType().Name;
				CreateRegisteredNode(nodeName);
			}

			wrappers.Clear();
		}

		public void HandleOnSceneActivating(World.Scene.Management.SceneActivatingProcess process) {
			Debug.Log(
				$"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color><color=green>[START]</color> _Scope_.HandleOnSceneActivating() : [scope='{this.Get().Scope.ToString()}'][isRoot='{isRoot}']",
				gameObject
			);
			_injectGameObjects(process.Instance.Scene.GetRootGameObjects());
			_collectRegisterablesInScene(process.Instance.Scene);

			Debug.Log(
				$"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color><color=#00ffaa>[END]</color> _Scope_.HandleOnSceneActivating() : [scope='{this.Get().Key}']",
				gameObject
			);
		}

		public void HandleOnSceneActivated(SceneInstance scene) {
			Debug.Log(
				$"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color><color=green>[START]</color> _Scope_.HandleOnSceneActivated() : [scope='{this.Get().Scope.ToString()}'][isRoot='{isRoot}']",
				gameObject
			);
			_registerFoundServices();

			Debug.Log(
				$"<color=grey>[INFO]</color><color=LightBlue>[SETUP]</color><color=#00ffaa>[END]</color> _Scope_.HandleOnSceneActivated() : [scope='{this.Get().Key}']",
				gameObject
			);
		}

		public void OnValidate() {
			if (isRoot) scopeName = "root";

			if (!defineEndSeperately && GetComponent<_ScopeEnd_>() == null)
				gameObject.AddComponent<_ScopeEnd_>();
		}
	}

	[Serializable]
	public class RegisterObject {
		public PropertyName Tag;
		public string Tag_;

		public UnityEngine.Object Obj;

		public (UnityEngine.Object obj, PropertyName tag) GetTuple() {
			if (Tag == default) Tag = new PropertyName(Tag_);
			return (Obj, Tag);
		}

		public (Type obj, PropertyName Tag) GetTupleType() {
			if (Tag == default) Tag = new PropertyName(Tag_);
			return (Obj.GetType(), Tag);
		}

		public string Readout() {
			if (Tag == default) Tag = new PropertyName(Tag_);
			return $"<color=blue>[READ]</color>[{nameof(RegisterObject)}][tag='{Tag.ToString()}'][type='{Obj.GetType()}']";
		}
	}
}
