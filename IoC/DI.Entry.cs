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

// TODO: make a string ID like PropertyName, will also need to make an IEquatable for PropertyName.
namespace Levels.Core.IoC {
	using System;
	using System.Security.Cryptography;
	using System.Text;

	public struct EntryKey : IEquatable<EntryKey> {
		public Core.String.ID<EntryKey> ID;
		public object Source;

		public EntryKey(string key, object source) {
			this.ID = Core.String.ID.Manager<EntryKey>.Reference.GetID(key);
			this.Source = source;
		}

		public static Double HashString(string input) {
			using (MD5 md5Hash = MD5.Create()) {
				byte[] data = Encoding.UTF8.GetBytes(input);
				byte[] hash = md5Hash.ComputeHash(data);
				long longHash = BitConverter.ToInt64(hash, 0);

				return Convert.ToDouble(longHash) / Convert.ToDouble(long.MaxValue);
			}
		}

		public bool Equals(EntryKey other) {
			return ID == other.ID;
		}

		public string Readout() {
			return $"READOUT : [RegisterKey][TagID={ID}][Source={Source}]";
		}
	}

	public interface ImEntry {
		EntryKey Key {
			get;
		}
	}

	public interface IEntry<T> : ImEntry, IEquatable<IEntry<T>> {
		/// <summary>
		/// Func<source, scope, key, targetType, return_service>
		/// </summary>
		Func<object, Scope.ID, string[], Type, T> Fufiller {
			get;
		}
	}

	public struct Entry<T> : IEntry<T> {
		private readonly EntryKey _key;
		public EntryKey Key {
			get => _key;
		}

		private readonly Func<object, Scope.ID, string[], Type, T> _fufiller;
		/// <summary>
		/// Func<source, scope, key, return_service>
		/// </summary>
		/// <todo>Not sure we need source. </todo>
		public Func<object, Scope.ID, string[], Type, T> Fufiller {
			get => _fufiller;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="fufiller">source, scope, key, return_service</param>
		public Entry(EntryKey key, Func<object, Scope.ID, string[], Type, T> fufiller) : this() {
			_key = key;
			_fufiller = fufiller;
		}

		public bool Equals(IEntry<T> other) {
			return Key.Equals(other.Key);
		}
	}

	public struct ServiceEntry<T> : IEntry<T>, IEquatable<ServiceEntry<T>> {
		private readonly EntryKey _key;
		public EntryKey Key {
			get => _key;
		}

		private readonly Func<object, Scope.ID, string[], Type, T> _fufiller;
		public Func<object, Scope.ID, string[], Type, T> Fufiller {
			get => _fufiller;
		}

		public ServiceEntry(EntryKey key, T instance) : this() {
			_key = key;
			_fufiller = new Func<object, Scope.ID, string[], Type, T>((source, scope, key, type) => { return instance; });
		}

		public ServiceEntry(string key, object source, T instance) : this(new EntryKey(key, source), instance) {
		}

		public bool Equals(IEntry<T> other) {
			return Key.Equals(other.Key);
		}

		public bool Equals(ServiceEntry<T> other) {
			return Key.Equals(other.Key);
		}
	}

	public partial class Entry {
		public static class Factory {
			public static IEntry<R> Service<R>(R reference, string key, object source) {

				return new ServiceEntry<R>(new EntryKey(key, source), reference);
			}

			// For structs/DataTypes you want to not copy.
			public static Entry<R> Service<R>(Func<R> reference, string key, object source) {

				return new Entry<R>(new EntryKey(key, source), (source, scope, key, type) => reference.Invoke());
			}

			public static IEntry<R> Scoped<R>(R reference, string key, object source) {

				return new ServiceEntry<R>(new EntryKey(key, source), reference);
			}

			// For structs/DataTypes you want to not copy.
			public static Entry<R> Scoped<R>(Func<R> reference, string key, object source) {

				return new Entry<R>(new EntryKey(key, source), (source, scope, key, type) => reference.Invoke());
			}
			
			/// <summary>
			/// For structs/DataTypes you want to not copy.
			/// </summary>
			/// <typeparam name="R"></typeparam>
			/// <param name="fufiller">source, scope, key, return_service</param>
			/// <param name="key"></param>
			/// <param name="source"></param>
			/// <returns></returns>
			public static Entry<R> Transient<R>(Func<object, Scope.ID, string[], Type, R> fufiller, string key, object source) {

				return new Entry<R>(new EntryKey(key, source), fufiller);
			}

			public static Entry<R> Transient<R>(Func<R> reference, string key, object source) {
				return new Entry<R>(new EntryKey(key, source), (source, scope, key, type) => reference.Invoke());
			}
		}
	}

	public static class IEntry_Extends {
		// TODO: Why?
		public static string Readout<T>(this IoC.IEntry<T> source) {
			return $"READOUT : [IRegistery]\n\n[key='{source.Key.Readout()}'][type='{typeof(T).Name}'][fulfiller='{source.Fufiller}']";
		}
	}
}



// TODO: Move
public interface IGenericMethod<R> {
	public R call<G>();
}