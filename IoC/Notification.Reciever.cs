// TODO : Header, and change log.
// TODO : Check if we need scope and source on most of these(removed them from the IRegister interface so could just be relics).

namespace Levels.Core.IoC {
	using System;
	using System.Collections.Generic;

	using Levels.Core.General;

	using UnityEngine;

	public interface IListen : IoC.IRegister
	{
		bool SubscribeToRoot { get; set;  }
		public Action OnNotified { get; set; }
		void Notify<I>(ref I messageID) where I : struct, INotification, IEquatableRef<I>;
	}

	/// <summary>
	/// The interfaces for Listeners.
	/// </summary>
	public interface IListen<N> : IListen, IEquatable<IListen<N>>
		 where N : struct, INotification, IEquatableRef<N>, IConvertible<N> {
		public void Notify(ref N notification);

		void IListen.Notify<I>(ref I messageID) {
			if (messageID.GetType().IsAssignableFrom(typeof(N))) {
				// TODO : Double check what this does.
				Notify(ref Board<N>.GetActive());
			}
		}
	}

	// TODO: Why have this and IReciever?
	public interface IReceive<N1> : IListen<N1>, IEquatable<IListen<N1>>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1> {

		public Receiver<N1> Reciever1 {
			get;
		}
		// SUGGESTED
		/****************************************************
		protected Receiver<TNotification> _reciever1 = new();
		public Receiver<TNotification> Reciever1 {
			get { return _reciever1; }
			set {
				var scope = _reciever1.Scope;
				var source = _reciever1.Source;
				_reciever1?.Unregister();
				_reciever1 = value;
				_reciever1.Register(scope, source);
				_reciever1.OnNotified += OnNotified;
			}
		}
		****************************************************/
	}

	public interface IReceiver<N1> : IReceive<N1>, IEquatable<IReceive<N1>>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1> {

		// IREGISTER -- START
		#region IREGISTER
		/// <summary>
		/// Get Source from retrievers.
		/// </summary>
		object source { get { return Reciever1?.Source; } set { } }
		/// <summary>
		/// Get Scope from retrievers.
		/// </summary>
		IoC.Scope.ID scope {
			get {
				if (Reciever1 == null)
					return IoC.Scope.ID.Null();
				return Reciever1.Scope;
			}
			set { }
		}

		void IoC.IRegister.Register(in IoC.Scope.ID scope) {
			this.scope = scope;
			
			#if UNITY_EDITOR
			if (this is UnityEngine.Object src)
				Debug.Log("<color=grey>[INFO]</color> " + this.GetType() + " : src=" + src.name, src);
			#endif

			this.Reciever1.Register(scope);
		}
		
		void IoC.IRegister.Unregister() {
			this.Reciever1.Unregister();
		}
		#endregion
		// IREGISTER -- END

		bool IEquatable<IReceive<N1>>.Equals(IReceive<N1> other) {
			return this.Reciever1.Equals(other.Reciever1);
		}
	}

	public static class IReciever_Extends {
		public static IReceiver<T> Interface_IReciever<T>(this IReceiver<T> receiver)
			where T : struct, INotification, IEquatableRef<T>, IConvertible<T>{

			return receiver;
		}
	}
	
	public class Receiver<T> : IReceiver<T>, IEquatable<Receiver<T>>
		where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {
		public Receiver<T> Reciever1 => this;

		// INOTIFY -- START
		#region INOTIFY
		public void Notify(ref T notification) {
			Notify_Hook?.Invoke(ref notification);
			OnNotified?.Invoke();
		}

		public bool SubscribeToRoot { get; set; }
		public Action OnNotified { get; set; }
		public RefAction<T> _onNotify;
		public RefAction<T> Notify_Hook { get => _onNotify; set => _onNotify = value; }

		void IListen<T>.Notify(ref T notification) {
			Notify_Hook.Invoke(ref notification);
			OnNotified();
		}
		#endregion
		// INOTIFY -- END

		// IREGISTER -- START
		#region IREGISTER
		public void Register(
			in IoC.Scope.ID scope) {
			Debug.Log($"<color=grey>[INFO]</color><color=olive>[REG]</color> Notifications.Receiver.Register() : scope='{scope.ToString()}' type={this.GetType()}", source.TryGetComponent()?.gameObject);

			_scope = scope;
			_source = this;
			Mediator.Collection<T>.SubReceiver(scope, this, source);

			if (SubscribeToRoot) {
				Mediator.Collection<T>.SubReceiver(IoC.Scope.ID.RequestReferenceByName("root"), this, source);
			}
		}

		private IoC.Scope.ID _scope;
		public IoC.Scope.ID Scope {
			get => _scope;
		}
		IoC.Scope.ID scope {
			get => this.Scope;
			set => _scope = value;
		}

		object _source;
		public object Source {
			get => _source;
		}
		object source {
			get => this.Source;
			set => _source = value;
		}

		public void Unregister() {
			Debug.Log($"<color=grey>[INFO]</color>[UNREG] Notifications.Receiver.Unregister() : scope='{Scope.ToString()}' type={this.GetType()}", Source.TryGetComponent()?.gameObject);

			Mediator.Collection<T>.UnsubReceiver(Scope, Source);

			if (SubscribeToRoot) {
				Mediator.Collection<T>.UnsubReceiver(IoC.Scope.ID.RequestReferenceByName("root"), Source);
			}

			OnNotified = null;
			_onNotify = null;
		}
		#endregion
		// IREGISTER -- END

		// LOGIC -- START
		#region LOGIC
		public bool Equals(IListen<T> other) {
			return Equals(other);
		}

		public bool Equals(Receiver<T> other) {
			return Source == other.Source && Scope == other.Scope && Notify_Hook == other.Notify_Hook;
		}
		#endregion
		// LOGIC -- END
	}

	public interface IRecieve<N1, N2> : IoC.IRegister, IEquatable<IRecieve<N1, N2>>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
		where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2> {

		public Receiver<N1> Reciever1 {
			get;
		}
		public Receiver<N2> Reciever2 {
			get;
		}
	}

	public interface IReciever<N1, N2> : IRecieve<N1, N2>, IEquatable<IRecieve<N1, N2>>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
		where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2> {

		// IREGISTER -- START
		#region IREGISTER
		/// <summary>
		/// Get Source and Scope from retrievers.
		/// </summary>
		IoC.Scope.ID scope { get => Reciever1.Scope; set { } }

		void IoC.IRegister.Register(in IoC.Scope.ID scope) {
			this.scope = scope;
			this.Reciever1.Register(scope);
			this.Reciever2.Register(scope);
		}

		void IoC.IRegister.Unregister() {
			this.Reciever1.Unregister();
			this.Reciever2.Unregister();
		}
		#endregion
		// IREGISTER -- END

		bool IEquatable<IRecieve<N1, N2>>.Equals(IRecieve<N1, N2> other) {
			return this.Reciever1.Equals(other.Reciever1) && this.Reciever2.Equals(other.Reciever2);
		}
	}

	public struct Reciever<N1, N2> : IReciever<N1, N2>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
		where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2> {

		public Reciever(RefAction<N1> HookOnNotifyN1, RefAction<N2> HookOnNotifyN2) {
			_reciever1 = new Receiver<N1>();
			_reciever1.Notify_Hook = HookOnNotifyN1;

			_reciever2 = new Receiver<N2>();
			_reciever2.Notify_Hook = HookOnNotifyN2;
		}

		private Receiver<N1> _reciever1;
		public Receiver<N1> Reciever1 { get => _reciever1; set => _reciever1 = value; }

		private Receiver<N2> _reciever2;
		public Receiver<N2> Reciever2 { get => _reciever2; set => _reciever2 = value; }

	}

	public interface IRecieve<N1, N2, N3> : IoC.IRegister, IEquatable<IRecieve<N1, N2, N3>>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
		where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2>
		where N3 : struct, INotification, IEquatableRef<N3>, IConvertible<N3> {

		public Receiver<N1> Reciever1 {
			get;
		}
		public Receiver<N2> Reciever2 {
			get;
		}
		public Receiver<N3> Reciever3 {
			get;
		}
	}

	public interface IReciever<N1, N2, N3> : IRecieve<N1, N2, N3>, IEquatable<IRecieve<N1, N2, N3>>
	where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
	where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2>
	where N3 : struct, INotification, IEquatableRef<N3>, IConvertible<N3> {

		// IREGISTER -- START
		#region IREGISTER
		/// <summary>
		/// Get Source and Scope from retrievers.
		/// </summary>
		IoC.Scope.ID scope { get => Reciever1.Scope; set { } }

		void IoC.IRegister.Register(in IoC.Scope.ID scope) {
			this.scope = scope;
			this.Reciever1.Register(scope);
			this.Reciever2.Register(scope);
			this.Reciever3.Register(scope);
		}

		void IoC.IRegister.Unregister() {
			this.Reciever1.Unregister();
			this.Reciever2.Unregister();
			this.Reciever3.Unregister();
		}
		#endregion
		// IREGISTER -- END

		bool IEquatable<IRecieve<N1, N2, N3>>.Equals(IRecieve<N1, N2, N3> other) {
			return this.Reciever1.Equals(other.Reciever1) && this.Reciever2.Equals(other.Reciever2) && this.Reciever3.Equals(other.Reciever3);
		}
	}


	public struct Reciever<N1, N2, N3> : IReciever<N1, N2, N3>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
		where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2>
		where N3 : struct, INotification, IEquatableRef<N3>, IConvertible<N3> {

		public int ID;

		private IoC.Scope.ID _scope;
		IoC.Scope.ID Scope { get => _scope; }
		IoC.Scope.ID scope { get => this.Scope; set => _scope = value; }

		public Receiver<N1> Reciever1 { get; }
		public Receiver<N2> Reciever2 { get; }
		public Receiver<N3> Reciever3 { get; }
	}

	public interface IRecieve<N1, N2, N3, N4> : IoC.IRegister, IEquatable<IRecieve<N1, N2, N3, N4>>
	where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
	where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2>
	where N3 : struct, INotification, IEquatableRef<N3>, IConvertible<N3>
	where N4 : struct, INotification, IEquatableRef<N4>, IConvertible<N4> {

		public Receiver<N1> Reciever1 { get; }
		public Receiver<N2> Reciever2 { get; }
		public Receiver<N3> Reciever3 { get; }
		public Receiver<N4> Reciever4 { get; }
	}


	public interface IReciever<N1, N2, N3, N4> : IRecieve<N1, N2, N3, N4>, IEquatable<IRecieve<N1, N2, N3, N4>>
	where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
	where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2>
	where N3 : struct, INotification, IEquatableRef<N3>, IConvertible<N3>
		where N4 : struct, INotification, IEquatableRef<N4>, IConvertible<N4> {

		// IREGISTER -- START
		#region IREGISTER
		/// <summary>
		/// Look at the recievers for Source and Scope.
		/// </summary>
		object source { get => Reciever1; set { } }
		/// <summary>
		/// Get Source and Scope from retrievers.
		/// </summary>
		IoC.Scope.ID scope { get => Reciever1.Scope; set { } }

		void IoC.IRegister.Register(in IoC.Scope.ID scope) {
			this.scope = scope;
			Reciever1.Register(scope);
			Reciever2.Register(scope);
			Reciever3.Register(scope);
			Reciever4.Register(scope);
		}

		void IoC.IRegister.Unregister() {
			Reciever1.Unregister();
			Reciever2.Unregister();
			Reciever3.Unregister();
			Reciever4.Unregister();
		}
		#endregion
		// IREGISTER -- START

		bool IEquatable<IRecieve<N1, N2, N3, N4>>.Equals(IRecieve<N1, N2, N3, N4> other) {
			return Reciever1.Equals(other.Reciever1) && this.Reciever2.Equals(other.Reciever2) && this.Reciever3.Equals(other.Reciever3) && this.Reciever4.Equals(other.Reciever4);
		}
	}

	public struct Reciever<N1, N2, N3, N4> : IReciever<N1, N2, N3, N4>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
		where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2>
		where N3 : struct, INotification, IEquatableRef<N3>, IConvertible<N3>
		where N4 : struct, INotification, IEquatableRef<N4>, IConvertible<N4> {

		public int ID;

		public IoC.Scope.ID _scope;
		IoC.Scope.ID Scope { get => _scope; }
		IoC.Scope.ID scope { get => this.Scope; set { } }

		public object _source;

		object Source { get => _source; }
		object source { get => this.Source; set { } }

		public Receiver<N1> Reciever1 { get; }
		public Receiver<N2> Reciever2 { get; }
		public Receiver<N3> Reciever3 { get; }
		public Receiver<N4> Reciever4 { get; }

	}

	public interface IRecieve<N1, N2, N3, N4, N5> : IoC.IRegister, IEquatable<IRecieve<N1, N2, N3, N4, N5>>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
		where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2>
		where N3 : struct, INotification, IEquatableRef<N3>, IConvertible<N3>
		where N4 : struct, INotification, IEquatableRef<N4>, IConvertible<N4>
		where N5 : struct, INotification, IEquatableRef<N5>, IConvertible<N5> {

		public Receiver<N1> Reciever1 { get; }
		public Receiver<N2> Reciever2 { get; }
		public Receiver<N3> Reciever3 { get; }
		public Receiver<N4> Reciever4 { get; }
		public Receiver<N5> Reciever5 { get; }
	}

	public interface IReciever<N1, N2, N3, N4, N5> : IRecieve<N1, N2, N3, N4, N5>, IEquatable<IRecieve<N1, N2, N3, N4, N5>>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
		where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2>
		where N3 : struct, INotification, IEquatableRef<N3>, IConvertible<N3>
		where N4 : struct, INotification, IEquatableRef<N4>, IConvertible<N4>
		where N5 : struct, INotification, IEquatableRef<N5>, IConvertible<N5> {

		// IREGISTER -- START
		#region IREGISTER
		/// <summary>
		/// Look at the recievers for Source and Scope.
		/// </summary>
		object source { get => Reciever1; set { } }
		/// <summary>
		/// Get Source and Scope from retrievers.
		/// </summary>
		IoC.Scope.ID scope { get => Reciever1.Scope; set { } }

		void IoC.IRegister.Register(in IoC.Scope.ID scope) {
			this.scope = scope;
			Reciever1.Register(scope);
			Reciever2.Register(scope);
			Reciever3.Register(scope);
			Reciever4.Register(scope);
			Reciever5.Register(scope);
		}

		void IoC.IRegister.Unregister() {
			Reciever1.Unregister();
			Reciever2.Unregister();
			Reciever3.Unregister();
			Reciever4.Unregister();
			Reciever5.Unregister();
		}
		#endregion
		// IREGISTER -- END

		bool IEquatable<IRecieve<N1, N2, N3, N4, N5>>.Equals(IRecieve<N1, N2, N3, N4, N5> other) {
			return Reciever1.Equals(other.Reciever1) && this.Reciever2.Equals(other.Reciever2) && this.Reciever3.Equals(other.Reciever3) && this.Reciever4.Equals(other.Reciever4) && this.Reciever5.Equals(other.Reciever5);
		}
	}

	public struct Receiver<N1, N2, N3, N4, N5> : IRecieve<N1, N2, N3, N4, N5>
		where N1 : struct, INotification, IEquatableRef<N1>, IConvertible<N1>
		where N2 : struct, INotification, IEquatableRef<N2>, IConvertible<N2>
		where N3 : struct, INotification, IEquatableRef<N3>, IConvertible<N3>
		where N4 : struct, INotification, IEquatableRef<N4>, IConvertible<N4>
		where N5 : struct, INotification, IEquatableRef<N5>, IConvertible<N5> {

		// TODO : Is this used?
		public int id;

		// IREGISTER -- START
		#region IREGISTER
		private IoC.Scope.ID _scope;
		IoC.Scope.ID Scope { get => _scope; }
		IoC.Scope.ID scope { get => this.Scope; set { } }
		object _source;

		object Source { get => _source; }
		object source { get => this.Source; set { } }

		public Receiver<N1> Reciever1 { get; }
		public Receiver<N2> Reciever2 { get; }
		public Receiver<N3> Reciever3 { get; }
		public Receiver<N4> Reciever4 { get; }
		public Receiver<N5> Reciever5 { get; }

		public void Register(
			in IoC.Scope.ID scope) {

			_scope = scope;

			Reciever1.Register(scope);
			Reciever2.Register(scope);
			Reciever3.Register(scope);
			Reciever4.Register(scope);
			Reciever5.Register(scope);
		}

		public void Unregister() {
			Reciever1.Unregister();
			Reciever2.Unregister();
			Reciever3.Unregister();
			Reciever4.Unregister();
			Reciever5.Unregister();
		}
		#endregion
		// IREGISTER -- END

		public bool Equals(IRecieve<N1, N2, N3, N4, N5> other) {
			return Reciever1.Equals(other.Reciever1) &&
					Reciever2.Equals(other.Reciever2) &&
					Reciever3.Equals(other.Reciever3) &&
					Reciever4.Equals(other.Reciever4) &&
					Reciever5.Equals(other.Reciever5);
		}
	}

	/// <summary>
	/// The static concept of Recievers and thier componensts
	/// </summary>
	public partial class Reciever {
		public struct Collection<T>
			where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {
			// [XAN] Stupid C#9.0 STUPID UNITY -- [Future XAN] Okay need to list a reason for these.
			public readonly Dictionary<object, Receiver<T>> Listeners;
			public Collection(bool _) { Listeners = new Dictionary<object, Receiver<T>>(); }

			public void Notify(
				ref T notification) {

				foreach (var key in Listeners.Keys) {
					Listeners[key].Notify(ref notification);
				}
			}

			public void AddListener(
				object source,
				in Receiver<T> receiver) {

				if (Listeners.ContainsKey(source))
					return;

				Listeners.Add(source, receiver);
			}

			public void RemoveListener(
				object source) {

				if (!Listeners.ContainsKey(source))
					return;

				Listeners.Remove(source);
			}
		}
	}

	public class TestReceiver<T>
		where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {
		private Receiver<T> _test;

		public TestReceiver() {
			_test = new Receiver<T>();
		}
	}
}

namespace Levels.Core.IoC {
	using Levels.Core.General;

	public static class Notification_IReciever_Extends {
		/// <summary>
		/// WARNING: Boxing on structs.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IListen<T> Interface_IReciever<T>(
			this IListen<T> instance)
			where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {

			return instance;
		}

		/// <summary>
		/// WARNING: Boxing on structs.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IReceive<T> Interface_IReciever<T>(
			this IReceive<T> instance)
			where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {

			return instance;
		}

		/// <summary>
		/// WARNING: Boxing on structs.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IRecieve<T, T2> Interface_IReciever<T, T2>(
			this IRecieve<T, T2> instance)
			where T : struct, INotification, IEquatableRef<T>, IConvertible<T>
			where T2 : struct, INotification, IEquatableRef<T2>, IConvertible<T2> {

			return instance;
		}

		/// <summary>
		/// WARNING: Boxing on structs.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IRecieve<T, T2, T3> Interface_IReciever<T, T2, T3>(
			this IRecieve<T, T2, T3> instance)
			where T : struct, INotification, IEquatableRef<T>, IConvertible<T>
			where T2 : struct, INotification, IEquatableRef<T2>, IConvertible<T2>
			where T3 : struct, INotification, IEquatableRef<T3>, IConvertible<T3> {

			return instance;
		}

		/// <summary>
		/// WARNING: Boxing on structs.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IRecieve<T, T2, T3, T4> Interface_IReciever<T, T2, T3, T4>(
			this IRecieve<T, T2, T3, T4> instance)
			where T : struct, INotification, IEquatableRef<T>, IConvertible<T>
			where T2 : struct, INotification, IEquatableRef<T2>, IConvertible<T2>
			where T3 : struct, INotification, IEquatableRef<T3>, IConvertible<T3>
			where T4 : struct, INotification, IEquatableRef<T4>, IConvertible<T4> {

			return instance;
		}

		/// <summary>
		/// WARNING: Boxing on structs.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IRecieve<T, T2, T3, T4, T5> Interface_IReciever<T, T2, T3, T4, T5>(
			this IRecieve<T, T2, T3, T4, T5> instance)
			where T : struct, INotification, IEquatableRef<T>, IConvertible<T>
			where T2 : struct, INotification, IEquatableRef<T2>, IConvertible<T2>
			where T3 : struct, INotification, IEquatableRef<T3>, IConvertible<T3>
			where T4 : struct, INotification, IEquatableRef<T4>, IConvertible<T4>
			where T5 : struct, INotification, IEquatableRef<T5>, IConvertible<T5> {

			return instance;
		}
	}
}
