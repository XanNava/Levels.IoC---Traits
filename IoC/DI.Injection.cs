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

	using Levels.Core;

	using UnityEngine;
	#endregion

	public interface IInjectable {
		void Inject(Scope.ID scope);
	}

	[Serializable]
	public class Inject<T> : IModify<T>, Core.ISet<T>, IDataRef<T> {
		[SerializeField]
		private T _value;
		private T _valueModified;
		public T Value { get => _value; set => _value = value; }
		[field: SerializeField]
		public string[] Tags { get; set; } = { "" };
		[field: SerializeField]
		public string Key { get; set; } = "";
		private Scope.ID scope;
		public ref Scope.ID Scope { get => ref scope; }
		[field: SerializeField]
		public Settings Settings { get; set; } = new();
		
		public void Set(T value) {
			Value = value;
			Settings.IsInjected = true;
		}

		public static implicit operator T(Inject<T> inject) {
			return inject.Value;
		}

		public static implicit operator Inject<T>(T value) {
			return new Inject<T> { Value = value, Settings = new Settings() { IsInjected = value != null } };
		}

		public Dictionary<object, List<IConfig<T>>> Modifiers;
		Dictionary<object, List<IConfig<T>>> IModify<T>._Modifiers {
			get {
				if (Modifiers == null)
					Modifiers = new Dictionary<object, List<IConfig<T>>>();
				return Modifiers;
			}
		}

		bool IModify<T>._DirtyModifiers { get; set; } = true;
		
		ref T IModify<T>._ValueModified => ref _valueModified;

		public ref T _DataRef => ref _value;
	}

	public static class IsInjectable_Extends {
		public static IInjectable Interface_IsInjectable(this IInjectable source) {
			return source;
		}
	}

	public interface IInject : IInjectable {
		public Inject<Scope.ID> In { get; set; }
		void Parse(Scope.ID scope) {
			In.Set(scope);
		}

		void IInjectable.Inject(Scope.ID scope) {
			if (In == null)
				In = new();

			if (In.Settings.IsInjected)
				return;
			
			In.Scope = scope;

			Parse(scope);

			In.Settings.IsInjected = true;

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color>[END] IInjectable.Inject() : [into='{this.GetType().Name}']", this is Component ? ((Component)this).gameObject : null);
			#endif
		}
	}

	public static class Inject_Extends {
		public static void Parse(this IInject self, Scope.ID scope) {
			self.Parse(scope);
		}
	}

	public interface IInject<IN1> : IInjectable, IHold<Inject<IN1>> {
		Inject<IN1> IData<Inject<IN1>>._Data { get => In1; set => In1 = value; }
		public Inject<IN1> In1 { get; set;  }
		void ParseIn1(IN1 request) {
			In1.Set(request);
		}

		void ValidateIn1() {
			if (In1 == null)
				In1 = new();
		}

		void IInjectable.Inject(Scope.ID scope) {
			ValidateIn1();
			
			if (In1.Settings.IsInjected)
				return;

			In1.Scope = scope;

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color><color=green>[START]</color> IInjectable.Inject() : [into='{this.GetType().Name}']\n[fromScope='{scope.Value}']\n|>[request1=({typeof(IN1).Name},{In1.Key},{In1.Tags})]", this is Component ? ((Component)this).gameObject : null);
			#endif

			ParseIn1(Request1(scope));

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color><color=#00ffaa>[END]</color> IInjectable.Inject() : [into='{this.GetType().Name}']", this is Component ? ((Component)this).gameObject : null);
			#endif
		}

		IN1 Request1(Scope.ID scope = default) {
			scope =  scope == Scope.ID.Null() ? In1.Scope : scope;

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color> IInjectable.Request1() : ({typeof(IN1)})\n[into='{this.GetType().Name}'][fromScope='{scope.Value}']\n|>[request1=({typeof(IN1).Name},{In1.Tags})]", this is Component ? ((Component)this).gameObject : null);
			#endif

			IN1 result;

			if (!In1.Settings.IsInjected) {

				result = Exports<IN1>.Retrieve<IN1>(this, ref scope, In1.Settings.UseLocalSettings ? In1.Settings : null, In1.Key, In1.Tags).Handle((r) => {
					In1.Settings.IsInjected = true;

					return r;
				},
				(e) => {
					#if UNITY_EDITOR
					Debug.LogError($"<color=red>[ERROR]</color><color=olive>[INJECT]</color> IInjectable.Request1() : [{this.GetType().Name}]\n[{e}] on " + (this is Component ? ((Component)this).gameObject.name : ""), this is Component ? ((Component)this).gameObject : null);
					#endif

					return default(IN1);
				});
			} else
				result = In1.Get();


			return result;
		}
	}

	public interface IInject<IN1, IN2> : IInject<IN1> {
		public Inject<IN2> In2 { get; set; }
		void ParseIn2(IN2 request) {
			In2.Set(request);
		}
		void ValidateIn2() {
			if (In2 == null)
				In2 = new();
		}

		void IInjectable.Inject(Scope.ID scope) {
			ValidateIn1();
			ValidateIn2();

			if (In1.Settings.IsInjected && 
				In2.Settings.IsInjected)
				return;
			
			In1.Scope = scope;
			In2.Scope = scope;

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color><color=green>[START]</color> IInjectable.Inject() : [into='{this.GetType().Name}']\n[fromScope='{scope.Value}']\n|>" +
				$"[request1=({typeof(IN1).Name},{In1.Key},{In1.Tags})]" +
				$"[request2=({typeof(IN2).Name},{In2.Key},{In2.Tags})]", this is Component ? ((Component)this).gameObject : null);
			#endif

			ParseIn1(Request1(scope));
			ParseIn2(Request2(scope));

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color><color=#00ffaa>[END]</color> IInjectable.Inject() : [into='{this.GetType().Name}']", this is Component ? ((Component)this).gameObject : null);
			#endif
		}

		IN2 Request2(Scope.ID scope = default)  {
			scope =  scope == Scope.ID.Null() ? In2.Scope : scope;
			
			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color> IInjectable.Request2() : ({typeof(IN2)})\n[into='{this.GetType().Name}'][fromScope='{scope.Value}']\n|>[request2=({typeof(IN2).Name},{In2.Tags})]", this is Component ? ((Component)this).gameObject : null);
			#endif

			IN2 result;

			if (!In2.Settings.IsInjected) {
				result = Exports<IN2>.Retrieve<IN2>(this, ref scope, In2.Settings.UseLocalSettings ? In2.Settings : null, In2.Key, In2.Tags).Handle((r) => {
					In2.Settings.IsInjected = true;

					return r;
				},
				(e) => {
					#if UNITY_EDITOR
					Debug.LogError($"<color=red>[ERROR]</color><color=olive>[INJECT]</color> IInjectable.Request2() : [{typeof(IN2).Name}]\n[{e}]", this is Component ? ((Component)this).gameObject : null);
					#endif

					return default(IN2);
				});
			} else
				result = In2.Get();


			return result;
		}
	}

	public interface IInject<IN1, IN2, IN3> : IInject<IN1, IN2> {
		public Inject<IN3> In3 { get; set; }
		void ParseIn3(IN3 request) {
			In3.Set(request);
		}
		void ValidateIn3() {
			if (In3 == null)
				In3 = new();
		}

		void IInjectable.Inject(Scope.ID scope) {
			ValidateIn1();
			ValidateIn2();
			ValidateIn3();

			if (In1.Settings.IsInjected && 
				In2.Settings.IsInjected && 
				In3.Settings.IsInjected)
				return;
			
			In1.Scope = scope;
			In2.Scope = scope;
			In3.Scope = scope;

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color><color=green>[START]</color> IInjectable.Inject() : [into='{this.GetType().Name}']\n[fromScope='{scope.Value}']\n|>" +
				$"[request1=({typeof(IN1).Name},{In1.Tags})]" +
				$"[request2=({typeof(IN2).Name},{In2.Tags})]" +
				$"[request3=({typeof(IN3).Name},{In3.Tags})]", this is Component ? ((Component)this).gameObject : null);
			#endif

			ParseIn1(Request1(scope));
			ParseIn2(Request2(scope));
			ParseIn3(Request3(scope));

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color><color=#00ffaa>[END]</color> IInjectable.Inject() : [into='{this.GetType().Name}']", this is Component ? ((Component)this).gameObject : null);
			#endif
		}

		protected bool SetupIn3() {
			if (In3 == null)
				In3 = new();

			if (In3.Settings.IsInjected)
				return true;

			return false;
		}

		IN3 Request3(Scope.ID scope = default)  {
			scope =  scope == Scope.ID.Null() ? In3.Scope : scope;
			
			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color> IInjectable.Request3() : ({typeof(IN3)})\n[into='{this.GetType().Name}'][fromScope='{scope.Value}']\n|>[request3=({typeof(IN3).Name},{In3.Tags})]", this is Component ? ((Component)this).gameObject : null);
			#endif

			IN3 result;

			if (!In3.Settings.IsInjected) {
				result = Exports<IN3>.Retrieve<IN3>(this, ref scope, In3.Settings.UseLocalSettings ? In3.Settings : null, In3.Key, In3.Tags).Handle((r) => {
					In3.Settings.IsInjected = true;

					return r;
				},
				(e) => {
					#if UNITY_EDITOR
					Debug.LogError($"<color=red>[ERROR]</color><color=olive>[INJECT]</color> IInjectable.Request3() : [{typeof(IN3).Name}]\n[{e}]", this is Component ? ((Component)this).gameObject : null);
					#endif

					return default(IN3);
				});
			} else
				result = In3.Get();


			return result;
		}
	}

	public interface IInject<IN1, IN2, IN3, IN4> : IInject<IN1, IN2, IN3> {
		public Inject<IN4> In4 { get; set; }
		void ParseIn4(IN4 request) {
			In4.Set(request);
		}
		void ValidateIn4() {
			if (In4 == null)
				In4 = new();
		}

		public new void Inject(Scope.ID scope) {
			ValidateIn1();
			ValidateIn2();
			ValidateIn3();
			ValidateIn4();

			if (In1.Settings.IsInjected && 
				In2.Settings.IsInjected && 
				In3.Settings.IsInjected && 
				In4.Settings.IsInjected)
				return;
			
			In1.Scope = scope;
			In2.Scope = scope;
			In3.Scope = scope;
			In4.Scope = scope;

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color><color=green>[START]</color> IInjectable.Inject() : [into='{this.GetType().Name}']\n[fromScope='{scope.Value}']\n|>" +
				$"[request1=({typeof(IN1).Name},{In1.Tags})]" +
				$"[request2=({typeof(IN2).Name},{In2.Tags})]" +
				$"[request3=({typeof(IN3).Name},{In3.Tags})]" +
				$"[request4=({typeof(IN4).Name},{In4.Tags})]", this is Component ? ((Component)this).gameObject : null);
			#endif

			ParseIn1(Request1(scope));
			ParseIn2(Request2(scope));
			ParseIn3(Request3(scope));
			ParseIn4(Request4(scope));

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color><color=#00ffaa>[END]</color> IInjectable.Inject() : [into='{this.GetType().Name}']", this is Component ? ((Component)this).gameObject : null);
			#endif
		}

		IN4 Request4(Scope.ID scope = default)  {
			scope =  scope == Scope.ID.Null() ? In4.Scope : scope;
			
			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color> IInjectable.Request4() : ({typeof(IN4)})\n[into='{this.GetType().Name}'][fromScope='{scope.Value}']\n|>[request4=({typeof(IN4).Name},{In4.Tags})]", this is Component ? ((Component)this).gameObject : null);
			#endif

			IN4 result;

			if (!In4.Settings.IsInjected) {
				result = Exports<IN4>.Retrieve<IN4>(this, ref scope, In4.Settings.UseLocalSettings ? In4.Settings : null, In4.Key, In4.Tags).Handle((r) => {
					In4.Settings.IsInjected = true;

					return r;
				},
				(e) => {
					#if UNITY_EDITOR
					Debug.LogError($"<color=red>[ERROR]</color><color=olive>[INJECT]</color> IInjectable.Request4() : [{typeof(IN4).Name}]\n[{e}]", this is Component ? ((Component)this).gameObject : null);
					#endif

					return default(IN4);
				});
			} else
				result = In4.Get();


			return result;
		}
	}

	public interface IInject<IN1, IN2, IN3, IN4, IN5> : IInject<IN1, IN2, IN3, IN4> {
		public Inject<IN5> In5 { get; set; }
		void ParseIn5(IN5 request) {
			In5.Set(request);
		}
		void ValidateIn5() {
			if (In5 == null)
				In5 = new();
		}

		public new void Inject(Scope.ID scope) {
			ValidateIn1();
			ValidateIn2();
			ValidateIn3();
			ValidateIn4();
			ValidateIn5();

			if (In1.Settings.IsInjected && 
				In2.Settings.IsInjected && 
				In3.Settings.IsInjected && 
				In4.Settings.IsInjected && 
				In5.Settings.IsInjected)
				return;
			
			In1.Scope = scope;
			In2.Scope = scope;
			In3.Scope = scope;
			In4.Scope = scope;
			In5.Scope = scope;

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color><color=green>[START]</color> IInjectable.Inject() : [into='{this.GetType().Name}']\n[fromScope='{scope.Value}']\n|>" +
				$"[request1=({typeof(IN1).Name},{In1.Tags})]" +
				$"[request2=({typeof(IN2).Name},{In2.Tags})]" +
				$"[request3=({typeof(IN3).Name},{In3.Tags})]" +
				$"[request4=({typeof(IN4).Name},{In4.Tags})]" +
				$"[request5=({typeof(IN5).Name},{In5.Tags})]", this is Component ? ((Component)this).gameObject : null);
			#endif

			ParseIn1(Request1(scope));
			ParseIn2(Request2(scope));
			ParseIn3(Request3(scope));
			ParseIn4(Request4(scope));
			ParseIn5(Request5(scope));

			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color><color=#00ffaa>[END]</color> IInjectable.Inject() : [into='{this.GetType().Name}']", this is Component ? ((Component)this).gameObject : null);
			#endif
		}

		IN5 Request5(Scope.ID scope = default)  {
			scope =  scope == Scope.ID.Null() ? In5.Scope : scope;
			
			#if UNITY_EDITOR
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[INJECT]</color> IInjectable.Request5() : ({typeof(IN5)})\n[into='{this.GetType().Name}'][fromScope='{scope.Value}']\n|>[request5=({typeof(IN4).Name},{In5.Key} ,{In5.Tags})]", this is Component ? ((Component)this).gameObject : null);
			#endif

			IN5 result;

			if (!In5.Settings.IsInjected) {
				result = Exports<IN5>.Retrieve<IN5>(this, ref scope, In5.Settings.UseLocalSettings ? In5.Settings : null, In5.Key, In5.Tags).Handle((r) => {
					In5.Settings.IsInjected = true;

					return r;
				},
				(e) => {
					#if UNITY_EDITOR
					Debug.LogError($"<color=red>[ERROR]</color><color=olive>[INJECT]</color> IInjectable.Request5() : [{typeof(IN5).Name}]\n[{e}]", this is Component ? ((Component)this).gameObject : null);
					#endif

					return default(IN5);
				});
			} else
				result = In5.Get();


			return result;
		}
	}

	public static class IInject_Extends {
		public static IInject<RQ1> Interface_IInject<RQ1>(this IInject<RQ1> source) => source;
		public static IInject<RQ1, RQ2> Interface_IInject<RQ1, RQ2>(this IInject<RQ1, RQ2> source) => source;
		public static IInject<RQ1, RQ2, RQ3> Interface_IInject<RQ1, RQ2, RQ3>(this IInject<RQ1, RQ2, RQ3> source) => source;
		public static IInject<RQ1, RQ2, RQ3, RQ4> Interface_IInject<RQ1, RQ2, RQ3, RQ4>(this IInject<RQ1, RQ2, RQ3, RQ4> source) => source;
		public static IInject<RQ1, RQ2, RQ3, RQ4, RQ5> Interface_IInject<RQ1, RQ2, RQ3, RQ4, RQ5>(this IInject<RQ1, RQ2, RQ3, RQ4, RQ5> source) => source;
	}
}
