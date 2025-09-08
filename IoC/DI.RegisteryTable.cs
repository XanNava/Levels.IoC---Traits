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

	using Levels.Core.General;

	using Results;
	#endregion

	public interface IRegisteryTable<T> {
		public Scope.ID Scope { get; }
		public List<IEntry<T>> Collection { get; }
		public Settings DefaultDISettings { get; set; }
		StringBuilder LogBuilder { get; }

		public IRegisteryTable<T> Register<I>(I reg) where I : IEntry<T>, IEquatable<I>;
		public IRegisteryTable<T> Unregister<I>(I reg) where I : IEntry<T>, IEquatable<I>;
		public IRegisteryTable<T> UnregisterAll(object reg);
		public Result<T> Retrieve<S>(object source = null, string key = "", params string[] tag);

		public Result<T> Retrieve<S>(EntryKey key, params string[] tag);

		public bool Contains(string key);

		public bool Contains(EntryKey key);

		public string Readout();
	}

	// TODO: make it so you can use Registery specific DI manager.
	public struct Registeries<T> : IRegisteryTable<T> {
		private readonly List<IEntry<T>> _collection;
		public readonly List<IEntry<T>> Collection {
			get => _collection;
		}

		private Scope.ID _scope;
		public Scope.ID Scope {
			get => _scope;
		}

		private Settings _defaultDISettings;
		public Settings DefaultDISettings {
			get => _defaultDISettings; set => _defaultDISettings = value;
		}

		private StringBuilder _logBuilder;
		public StringBuilder LogBuilder {
			get => _logBuilder;
		}

		public Registeries(Scope.ID scope, StringBuilder builder, Settings defaultDISettings = null) {
			if (defaultDISettings == null)
				defaultDISettings = new Settings();

			_collection = new List<IEntry<T>>();
			_logBuilder = builder;
			_scope = scope;
			_defaultDISettings = defaultDISettings;
		}

		public IRegisteryTable<T> Register<I>(I reg) where I : IEntry<T>, IEquatable<I> {
			Console.WriteLine($"++ REG : (type={typeof(T).Name}', key='{reg.Key.ID.ToString()}')\n[Scope='{Scope.ToString()}']\n|>{reg.Readout()}");
			if (!Contains(reg.Key)) {
				Collection.Add(reg);
			}

			return this;
		}

		public IRegisteryTable<T> Unregister<I>(I reg) where I : IEntry<T>, IEquatable<I> {
			Console.WriteLine($"-- UNREG : (type={typeof(T).Name}', key='{reg.Key.ID.ToString()}')\n[Scope='{Scope.ToString()}']\n|>{reg.Readout()}");
			if (Contains(reg.Key)) {
				Collection.Remove(reg);
			}

			return this;
		}

		public IRegisteryTable<T> UnregisterAll(object reg) {
			for (int i = 0; i < Collection.Count; i++) {
				if (Collection[i].Key.Source == reg) {
					Collection.RemoveAt(i);
				}
			}

			return this;
		}

		public bool Contains(string key) {
			var idKey = Core.String.ID.Manager<EntryKey>.Reference.GetID(key);

			// If the String.ID doesn't exist in the manager it will return the null String.ID
			if (idKey.Value == 0)
				return false;

			return Collection.FindIndex(r => r.Key.ID == idKey) >= 0;
		}

		public bool Contains(EntryKey key) {
			return Collection.FindIndex(r => r.Key.ID == key.ID) >= 0;
		}

		public Result<T> Retrieve<S>(EntryKey key, params string[] tags) {
			Console.WriteLine($"RETRIEVE : [RegisterTable] (key={key.ID.ToString()}, tag={tags})\n\n[{Readout()}]");

			if (Contains(key)) {
				return Collection.Find((t) => {

					return t.Key.Equals(key);
				}).Fufiller(this, Scope, tags, typeof(S));
			}

			throw new KeyNotFoundException(key.ID.ToString());
		}

		public Result<T> Retrieve<S>(object source = null, string key = "", params string[] tags) {
			return Retrieve<S>(new EntryKey(key, source), tags);
		}

		public string Readout() {
			LogBuilder.Clear();

			LogBuilder.Append($"READOUT : [RegisteryTable]\n[scope='{Scope.ToString()}']\n|>[Type='{typeof(T).Name}'][Settings={DefaultDISettings.Readout()}]");
			foreach (int i in Collection.Count) {
				LogBuilder.AppendLine(Collection[i].Readout());
			}
			return LogBuilder.ToString();
		}

	}
}