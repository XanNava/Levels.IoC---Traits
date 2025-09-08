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
	using System;
	using System.Collections.Generic;
	using Levels.Core.General;
	
	/// <summary>
	/// The Manager/Dispatcher of the notifications.
	/// </summary>
	/// <typeparam name="T"> Type of Notification.</typeparam>
	/// TODO: Make it so the Handler calls the chain?
	public struct Mediator<T>
		where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {
		private Reciever.Collection<T> _listeners;

		// TODO: Modify the chain so they can cross comunicate.
		// So each chain can cancel a notification, and determine if it should stop
		// and/or respect other cancel tokens.
		public readonly Chain<T> chain;

		public object Source {
			get => this;
		}

		private float _weight;
		public float Weight {
			get => 0;
		}

		// Used for cross mediator/scope notification canceling
		private RefFunc<T, CancelToken, bool> _handler;
		public RefFunc<T, CancelToken, bool> Handler {
			get => _handler;
		}

		public void AddHandler(RefFunc<T, CancelToken, bool> handler) {
			_handler += handler;
		}

		public void RemoveHandler(RefFunc<T, CancelToken, bool> handler) {
			_handler -= handler;
		}
		
		public Mediator(float weight, object source = null) {
			handledLocker = default;
			_weight = weight;
			_listeners = new Reciever.Collection<T>(false);
			this.chain = new Chain<T>(new Chain<T>.Settings());
			_handler = null;
		}
		
		public Mediator.NotifyLock<T> handledLocker;
		public void Notify(
			ref T notification) {
			
			if (!handledLocker.notification.Equals(notification) ||
				!chain.settings.doIgnoreCancels &&
				handledLocker.token.IsCancelled) {
				//TODO: Probably return why it cancelled.
				handledLocker = default; // clear the locker for future states.
				
				return;
			}
			
			handledLocker = default; // clear the locker for future states.
			
			_listeners.Notify(ref notification);
		}
		
		public static void NotifyScope(
			IoC.Scope.ID Scope,
			ref T notification) {

			Mediator.Collection<T>.NotifyScope(Scope, ref notification);
		}

		public static void BroadcastNotification(
			ref T notification) {

			Mediator.Collection<T>.NotifyAllScopes(ref notification);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="notification"></param>
		/// <param name="token"></param>
		/// <returns>Indecates if it will wait for other chains.</returns>
		public bool Handle(ref T notification, ref CancelToken token) {
			chain.PushToResponsabilities(ref notification, ref token);

			handledLocker = new Mediator.NotifyLock<T>() { notification = notification, token = token };

			if (chain.settings.doCancleAcrossScope || token.IsCancelled) {
				return true;
			}

			Notify(ref notification);

			return false;
		}

		public void Cancel(T notification) {
			handledLocker = new Mediator.NotifyLock<T>() {
				notification = notification,
				token = CancelToken.True
			};
		}

		public void SubscribeListener(
			in Receiver<T> listener,
			object source) {

			_listeners.AddListener(source, in listener);
		}

		public void UnsubscribeListener(
			object source) {

			_listeners.RemoveListener(source);
		}

		public void SubToChain(
			ref Chain.Responce<T> toSub) {

			chain.SubscribeResponsability(ref toSub);
		}

		public void UnsubFromChain(
			ref Chain.Responce<T> toUnsud) {

			chain.UnsubscribeResponsability(ref toUnsud);
		}

		public void Take(T context) => throw new NotImplementedException();
	}
	namespace Mediator {
		/// <summary>
		/// Used to make sure the notification passes through the chain(CoR) first.
		/// </summary>
		/// <typeparam name="T">The type of Notification this is mediating for.</typeparam>
		public struct NotifyLock<T>
			where T : INotification, IEquatableRef<T> {

			public T notification;
			public CancelToken token;
		}

		/// <summary>
		/// Contains Notifications
		/// </summary>
		/// <typeparam name="T">Type of Notification this collection Mediats</typeparam>
		public static class Collection<T>
			where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {
			
			public static readonly Dictionary<IoC.Scope.ID, Mediator<T>> collection = new Dictionary<IoC.Scope.ID, Mediator<T>>();
			
			public static void NotifyAllScopes(
				ref T notification) {
				CancelToken tokenScoped = CancelToken.False;
				
				foreach (var key in collection.Keys) {
					CancelToken _token = CancelToken.False;
					ref CancelToken token = ref _token;
					
					collection[key].Handle(ref notification, ref token);
					
					if (token.IsCancelled) {
						tokenScoped.Cancel();
					}
				}
				
				if (tokenScoped.IsCancelled) {
					foreach (var key in collection.Keys) {
						if (collection[key].chain.settings.doCancleAcrossScope)
							collection[key].Cancel(notification);
					}
				}
				
				foreach (var key in collection.Keys) {
					collection[key].Notify(ref notification);
				}
			}

			public static void NotifyScope(
				IoC.Scope.ID Scope,
				ref T notification,
				Action OnComplete = null) {
				
				CancelToken _ = new();
				if (collection.ContainsKey(Scope)) {
					collection[Scope].Handle(ref notification, ref _);
					if (!_.IsCancelled) {
						OnComplete?.Invoke();
					}
					// TODO: Figure out why this is here, and also in handle, and why we can't just clear it from handle(doesn't work if not there).
					//collection[Scope].Notify(ref notification);
				}
			}
			
			public static void SubToChain(
				IoC.Scope.ID scope,
				Chain.Responce<T> toSub) {
				
				if (!collection.ContainsKey(scope))
					return;
				
				collection[scope].SubToChain(ref toSub);
			}
			
			public static void UnsubFromChain(
				in IoC.Scope.ID scope,
				Chain.Responce<T> toUnsud) {

				if (!collection.ContainsKey(scope))
					return;

				collection[scope].UnsubFromChain(ref toUnsud);
			}

			public static void SubReceiver(
				in IoC.Scope.ID scope,
				Receiver<T> listener,
				object source) {

				if (!collection.ContainsKey(scope)) {
					collection.Add(scope, new Mediator<T>(0));
				}

				collection[scope].SubscribeListener(in listener, source);
			}

			public static void UnsubReceiver(
				IoC.Scope.ID scope,
				object source) {

				if (!collection.ContainsKey(scope)) {
					return;
				}

				collection[scope].UnsubscribeListener(source);
			}

			public static void AddHandler(
				IoC.Scope.ID scope,
				Mediator<T> Mediator) {

				if (collection.ContainsKey(scope)) {
					return;
				}
				collection.Add(scope, Mediator);
			}

			public static void RemoveHandler(
				IoC.Scope.ID scope) {

				if (!collection.ContainsKey(scope)) {
					return;
				}

				collection.Remove(scope);
			}

			public static bool HasHandler(
				IoC.Scope.ID scope) {

				return collection.ContainsKey(scope);
			}
		}
	}
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