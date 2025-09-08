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


// TODO : Change this to something more relevant.
namespace Levels.Core.IoC {
	#region Usings
	using System;

	using Levels.Core.General;

	using UnityEngine.Playables;
	#endregion

	// TODO: This might cause boxing/unboxing issues as I am not sure how the behavior of implemented interfaces and structs will work.
	// TODO: DO I need this?
	public interface _Notification_<TImplemented, TContext> : INotification<TContext>, IEquatableRef<TImplemented>, IConvertible<TImplemented>
		where TImplemented : struct, INotification, IEquatableRef<TImplemented> {

		bool IEquatableRef<TImplemented>.Equals(ref TImplemented other) => Equals(Board<TImplemented>.GetID(ref other));
		TImplemented IConvertible<TImplemented>.Convert<I>() => (TImplemented)this;

		static bool refEquals(ref TImplemented source, Notification.ID other)  => Board<TImplemented>.GetID(ref source).Equals(other);
	}

	public partial interface INotification : IEquatable<Notification.ID> {
	}

	public interface INotification<T> : INotification {
		public T Context { get; set; }
	}

	public partial struct Notification<T> : INotification<T>, IEquatableRef<Notification<T>>, IConvertible<Notification<T>> {
		public T Context { get; set; }

		public Notification(T context, string id, object source = null) {
			Context = context;
		}

		public bool Equals(ref Notification<T> other) => Equals(other);

		public bool Equals(Notification.ID other) {
			return Board<Notification<T>>.GetID(ref this).Equals(other);
		}

		public Notification<T> Convert<I>() => this;
	}

	// TODO : Move this either to Utilities.Notifications or Notifications.Utilities,,, or both, probably both.
	public static partial class Utilities {
		public static void NotifyScope<TNotification>(
			Scope.ID scope,
			ref TNotification notification)
			where TNotification : struct, INotification, IEquatableRef<TNotification>, IConvertible<TNotification> {

			Mediator.Collection<TNotification>.NotifyScope(scope, ref notification);
		}

		public static void NotifyScope<TNotification>(
			Scope.ID scope,
			ref TNotification notification,
			Action OnComplete)
			where TNotification : struct, INotification, IEquatableRef<TNotification>, IConvertible<TNotification> {

			Mediator.Collection<TNotification>.NotifyScope(scope, ref notification, OnComplete);
		}
		
		public static ref TNotification CreateNotification<TNotification, TContext>(
			TContext context)
			where TNotification : struct, INotification<TContext>, IEquatableRef<TNotification>, IConvertible<TNotification> {
			
			ref var ntf = ref Board<TNotification>.GetNew<TNotification, TContext>(context);
			
			return ref ntf;
		}
		
		public static ref TNotification CreateNotification<TNotification, TContext>(
			TContext context,
			object src)
			where TNotification : struct, INotification<TContext>, IEquatableRef<TNotification>, IConvertible<TNotification> {
			
			ref var ntf = ref Board<TNotification>.GetNew<TNotification, TContext>(context, src);
			
			return ref ntf;
		}
		
		public static ref TNotification CreateNotificationAndNotifyScope<TNotification, TContext>(
			Scope.ID scope,
			TContext context)
			where TNotification : struct, INotification<TContext>, IEquatableRef<TNotification>, IConvertible<TNotification> {
			
			ref var ntf = ref Board<TNotification>.GetNew<TNotification, TContext>(context);
			
			Mediator.Collection<TNotification>.NotifyScope(scope, ref ntf);
			
			return ref ntf;
		}
		
		public static void NotifyAllScopes<TNotification>(
			ref TNotification notification)
			where TNotification : struct, INotification, IEquatableRef<TNotification>, IConvertible<TNotification> {
			
			Mediator.Collection<TNotification>.NotifyAllScopes(ref notification);
		}
		
		public static void CreateAndNotifyAllScopes<TNotification, TContext>(
			TContext context)
			where TNotification : struct, INotification<TContext>, IEquatableRef<TNotification>, IConvertible<TNotification> {
			
			Mediator.Collection<TNotification>.NotifyAllScopes(ref Board<TNotification>.GetNew<TNotification, TContext>(context));
		}
		
		public static void SubscribeResponderToScope<TNotification>(ref Chain.Responce<TNotification> responce, Scope.ID scope) 
			where TNotification : struct, INotification, IEquatableRef<TNotification>, IConvertible<TNotification> {

			Mediator.Collection<TNotification>.SubToChain(scope, responce);
		}

		public static void UnsubscribeResponderFromScope<TNotification>(ref Chain.Responce<TNotification> responce, Scope.ID scope) 
			where TNotification : struct, INotification, IEquatableRef<TNotification>, IConvertible<TNotification> {

			Mediator.Collection<TNotification>.UnsubFromChain(scope, responce);
		}

		public static Notification.ID GetNotificationID<T>(ref T notif) where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {
			return Board<T>.GetID(ref notif);
		}

		public static object GetNotificationSource<T>(ref T notif) where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {
			return Board<T>.GetSource(ref notif);
		}
	}

	public static class INotificationG_Extends {
		public static ref R CastTo<R, I, T>(this ref I from)
			where R : struct, INotification<T>, IEquatableRef<R>
			where I : struct, INotification<T>, IEquatableRef<I> {

			return ref Board<R>.Add(Factory<R, T>.Create(from.Context));
		}

		public static ref To Copy<To, TFrom, TCntx>(this ref To copyTo, ref TFrom from)
			where To : struct, INotification<TCntx>, IEquatableRef<To>
			where TFrom : struct, INotification<TCntx>, IEquatableRef<TFrom> {

			copyTo.Context = from.Context;

			return ref copyTo;
		}

		public static void BroadcastToScope<T>(this ref T ntf, Scope.ID scope)
			where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {
			Utilities.NotifyScope<T>(scope, ref ntf);
		}
	}

	// TODO : Can we replace this with a string.ID?
	public partial class Notification {
		public struct ID : IEquatable<ID> {
			public ID(int id) {
				Value = id;
			}
			public int Value;

			public bool Equals(ID other) => Value == other.Value;
		}
	}

	public static class RefQue_ID_Extends {
		public static Notification.ID ToNotificationID(this RefQueue.ID id) {
			return new Notification.ID(id.value);
		}
	}
}