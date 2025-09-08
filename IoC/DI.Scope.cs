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
	using System.Linq;

	using UnityEngine;
	
	using Levels.Core.General;
	#endregion

	public interface IScope {
		public Scope.ID ScopeID { get; set; }
		public string Name { get; }
		public Settings DefaultDISettings { get; }

		public Scope.ID Parent { get; }

		public void InjectAll(IInjectable[] injections);

		public void Inject(IInjectable injection);

		public void BroadcastNotification<T>(ref T notification) where T : struct, INotification, IEquatableRef<T>, IConvertible<T>;

		public void BroadcastNotification<T, V>(V context) where T : struct, INotification<V>, IEquatableRef<T>, IConvertible<T>;
	}
	
	public class Scope : IScope {
		public static readonly string RootName = "root";
		private static ID _root;
		public static ID Root {
			get {
				if (_root == default(ID)) {
					_root = new Scope("root", default(ID), new Settings()).ScopeID;
				}
				
				return _root;
			}
			set => _root = value;
		}
		
		private ID _id;
		public ID ScopeID { get => _id; set => _id = value; }
		
		public string Name { get; }
		
		public Settings DefaultDISettings { get; }
		
		private ID _parent;
		ID IScope.Parent => _parent;
		public ref ID Parent => ref _parent;
		
		public void SetParent(ID id) {
			_parent = id;
		}
		
		public Scope(
			string name,
			ID parent = default,
			Settings defaultDISettings = default) {
			_id = ID.RequestReferenceByName(name);
			
			Name = name;
			_parent = parent;
			DefaultDISettings = defaultDISettings;
			
			// TODO : I had this here, but I don't think this is a good practice, if you are reading this past [10/16/2024] DELETE
			//Exports<Scope.ID>.Register(_id, Entry.Factory.Service(_id, "", this));

			if (!References.Collection.ContainsKey(_id))
				References.Collection.Add(_id, this);
			else
				Debug.Log($"<color=grey>[INFO]</color>[SCOPE] Scope.Scope() already contains the key {_id.ToString()}");
		}
		
		public void InjectAll(IInjectable[] injections) {
			if (injections == null) {
				Debug.LogError("<color=red>[INFO]</color>[SCOPE] Scope.InjectAll() : Null injectable passed.");
				return;
			}

			foreach (int i in injections.Length) {
				if (injections[i] == null)
					Debug.LogError("<color=red>[INFO]</color>[SCOPE] Scope.InjectAll() : Null injectable passed.");
				
				try
				{
					injections[i]?.Inject(_id);
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}
		}
		
		public void Inject(IInjectable injection) 
			=> injection.Inject(ScopeID);
		
		public void BroadcastNotification<T>(ref T notification) where T : struct, INotification, IEquatableRef<T>, IConvertible<T> 
			=> IoC.Utilities.NotifyAllScopes(ref notification);
		
		public void BroadcastNotification<T, V>(V context) where T : struct, INotification<V>, IEquatableRef<T>, IConvertible<T> 
			=> IoC.Utilities.NotifyAllScopes(ref IoC.Utilities.CreateNotification<T, V>(context));
		
		public void BroadcastNotification<T, V>(V context, object src) where T : struct, INotification<V>, IEquatableRef<T>, IConvertible<T> 
			=> IoC.Utilities.NotifyAllScopes(ref IoC.Utilities.CreateNotification<T, V>(context, src));
		
		public void Notify<T>(ref T notification) where T : struct, INotification, IEquatableRef<T>, IConvertible<T> 
			=> IoC.Utilities.NotifyScope(_id, ref notification);
		
		public void Notify<T, V>(V context, object src) where T : struct, INotification<V>, IEquatableRef<T>, IConvertible<T> 
			=> IoC.Utilities.NotifyScope(_id, ref IoC.Utilities.CreateNotification<T, V>(context, src));
		
		public bool HasParent()
			=> !_parent.Equals(default);
		
		public Scope CreateChildScope(string name, Settings setting = default) 
			=> new Scope(name, ScopeID, setting);
		
		public static class References {
			public static readonly Dictionary<ID, Scope> Collection = new();
			
			public static bool AddScope(Scope scope) {
				if (Collection.ContainsKey(scope._id)) return false;
				Collection.Add(scope.ScopeID, scope);
				return true;
			}
			
			public static Scope GetScope(ID scope) {
				if (!Collection.ContainsKey(scope)) return null;
				return Collection[scope];
			}
			
			public static Scope GetScope(string name) {
				return Collection.FirstOrDefault(scope => string.Equals(scope.ToString(), name)).Value;
			}
		}

		public static class Factory {
			public static Scope CreateRoot() {
				if (Root == default) {
					var root = new Scope(RootName, ID.RequestReferenceByName(RootName), new Settings());
					Root = root.ScopeID;
					References.AddScope(root);
				}

				return References.GetScope(default(ID));
			}

			public static Scope CreateChildScope(string name, ID parent) {
				if (References.GetScope(name) != null) {
					return References.GetScope(name);
				}
				
				var scope = new Scope(name, parent, new Settings());
				References.AddScope(scope);
				return scope;
			}

			public static Scope CreateScope(string name) {
				var scope = new Scope(name, default, new Settings());
				References.AddScope(scope);

				return scope;
			}
		}
		
		[Serializable]
		public struct ID : IEquatable<ID> {
			public ID(string name) 
				=> Value = Core.String.ID.Manager<IScope>.RequestID(name);

			public ID(int id) 
				=> Value = Core.String.ID.Manager<IScope>.GetID(id);

			public ID(Core.String.ID<IScope> id) 
				=> Value = id;

			public Core.String.ID<IScope> Value;

			public bool Equals(ID other) 
				=> Value.Equals(other.Value);

			public static bool operator ==(ID left, ID right) 
				=> left.Equals(right);

			public static bool operator !=(ID left, ID right) 
				=> !left.Equals(right);

			public static ID RequestReferenceByName(string name) 
				=> new ID(Core.String.ID.Manager<IScope>.RequestID(name));

			public static ID Null()
				=> new ID(Core.String.ID<IScope>.Null());

			// WARN : Possibly impure struct method called on readonly variable: struct value always copied before invocation.
			public void RegisterService<T>(T reference, string key, object src) 
				=> Exports<T>.Register(this, Entry.Factory.Service(reference, key, src));

			public void RegisterService<T>(Func<T> reference, string key, object src) 
				=> Exports<T>.Register(this, Entry.Factory.Service(reference, key, src));
			
			public void UnregisterAll<T>(object src) 
				=> Exports<T>.UnregisterAll(this, src);
			
			public void NotifyScope<T>(ref T notification) where T : struct, INotification, IEquatableRef<T>, IConvertible<T> 
				=> IoC.Utilities.NotifyScope(this, ref notification);
			
			public void NotifyScope<I, C>(C cotnext) where I : struct, INotification<C>, IEquatableRef<I>, IConvertible<I> 
				=> IoC.Utilities.NotifyScope(this, ref IoC.Utilities.CreateNotification<I, C>(cotnext));
			
			public void NotifyScope<I, C>(C cotnext, Action onComplete) where I : struct, INotification<C>, IEquatableRef<I>, IConvertible<I> 
				=> IoC.Utilities.NotifyScope(this, ref IoC.Utilities.CreateNotification<I, C>(cotnext), onComplete);
			
			public override bool Equals(object obj) 
				=> base.Equals(obj);

			public override int GetHashCode()
				=> Value.GetHashCode();

			public override string ToString() 
				=> Core.String.ID.Manager<IScope>.IDtoString(this.Value);
		}
	}
}

public static class IScope_Extends {
	public static Levels.Core.IoC.IScope Interface_IScope(this Levels.Core.IoC.IScope scope) 
		=> scope;
}