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


namespace Levels.Unity.IoC {
	#region usings
	using System;

	using Core;

	using Sirenix.OdinInspector;

	using Levels.Core.IoC;
	using Levels.Core.General;

	using UnityEngine;
	using UnityEngine.Serialization;
	#endregion

	// TODO : When Sub to root is checked durring playtime, sub it.
	[DefaultExecutionOrder(10000)]
	public class Receiver<TNotification, TContext> : MonoBehaviour, IReceiver<TNotification>, IDebug, ITake<TContext>
	where TNotification : struct, INotification<TContext>, IConvertible<TNotification>, IEquatableRef<TNotification> {

		protected Core.IoC.Receiver<TNotification> _reciever1;

		// SETTINGS -- START

		#region SETTINGS
		public Core.IoC.Receiver<TNotification> Reciever1
		{
			get { return _reciever1; }
			set
			{
				Scope.ID scope = default;
				object source = this;

				if (_reciever1 != null) {
					scope = _reciever1.Scope;
					source = _reciever1.Source;
				}

				_reciever1?.Unregister();
				_reciever1 = value;
				_reciever1.SubscribeToRoot = this.SubscribeToRoot;
				_reciever1.Register(scope);
				_reciever1.OnNotified += OnNotified;
			}
		}

		[field: SerializeField, BoxGroup("SETTINGS")]
		public bool SubscribeToRoot { get; set; }
		
		public virtual void Notify(ref TNotification notification) {
			if (!this.enabled)
				return;

			if (DoDebug)
				Debug.Log($"<color=grey>[INFO]</color><color=blue>[RECEIVE]</color> {notification.GetType().Name} on {gameObject.name}", gameObject);

			#if UNITY_EDITOR
			_lastContextOn = Time.time;
			#endif

			Take(notification.Context);

			if (OnNotifySource.HasListeners())
				OnNotifySource.Invoke(Core.IoC.Utilities.GetNotificationSource(ref notification));
		}
		
		public virtual void Take(TContext context) {
			_lastContext = context;

			OnNotifyContext?.Invoke(context);
		}
		#endregion

		// SETTINGS -- END

		// OUT -- START

		#region OUT
		public Action OnNotified { get; set; }
		[BoxGroup("OUT")]
		public _Action_<TContext> OnNotifyContext;
		[BoxGroup("OUT")]
		public _Action_<object> OnNotifySource;

		#if UNITY_EDITOR
		[SerializeField, BoxGroup("STATE"), ReadOnly]
		private float _lastContextOn;
		#endif

		[SerializeField, BoxGroup("STATE"), ReadOnly]
		protected TContext _lastContext;
		#endregion

		// OUT -- END

		public virtual void Awake() {
			Reciever1 = new();
			_reciever1.Notify_Hook += Notify;
		}
		
		[SerializeField, BoxGroup("DEBUG")]
		private bool _doDebug;
		public bool DoDebug {
			get {
				return _doDebug;
			}
			set {
				_doDebug = value;
				SetChildren();
			}
		}
		
		public virtual void SetChildren() {
			this.Debug_OnValidate();
		}

		#pragma warning disable CS0253 // Possible unintended reference comparison; right hand side needs cast
		public bool Equals(IListen<TNotification> other) => this == other;
		#pragma warning restore CS0253 // Possible unintended reference comparison; right hand side needs cast
	}
	
	public class Receiver<TNotification> : MonoBehaviour, IReceiver<TNotification>, IDebug
		where TNotification : struct, INotification, IConvertible<TNotification>, IEquatableRef<TNotification> {

		protected Core.IoC.Receiver<TNotification> _reciever1;

		// SETTINGS -- START
		#region SETTINGS
		public Core.IoC.Receiver<TNotification> Reciever1 {
			get { return _reciever1; }
			set {
				Scope.ID scope = default;
				if (_reciever1 != null) {
					scope = _reciever1.Scope;
				}

				_reciever1?.Unregister();
				_reciever1 = value;
				_reciever1.SubscribeToRoot = this.SubscribeToRoot;
				_reciever1.Register(scope);
				_reciever1.OnNotified += OnNotified;
			}
		}

		[field: SerializeField, BoxGroup("SETTINGS")]
		public bool SubscribeToRoot { get; set; }

		public virtual void Notify(ref TNotification notification) {
			if (!this.enabled)
				return;

			if (DoDebug)
				Debug.Log($"<color=grey>[INFO]</color><color=blue>[RECEIVE]</color> {notification.GetType().Name} on {gameObject.name}", gameObject);

#if UNITY_EDITOR
			_lastReceivedOn = Time.time;
#endif

			OnNotifyContext?.Invoke();
			if (OnNotifySource.HasListeners())
				OnNotifySource.Invoke(Core.IoC.Utilities.GetNotificationSource(ref notification));
		}
		#endregion
		// SETTINGS -- END

		// OUT -- START
		#region OUT
		public Action OnNotified { get; set; }
		[BoxGroup("OUT")]
		public _Action_ OnNotifyContext;
		[BoxGroup("OUT")]
		public _Action_<object> OnNotifySource;

		#if UNITY_EDITOR
		[FormerlySerializedAs("_lastContextOn"),SerializeField, BoxGroup("STATE"), ReadOnly]
		private float _lastReceivedOn;
		#endif
		#endregion
		// OUT -- END

		public virtual void Awake() {
			Reciever1 = new();
			_reciever1.Notify_Hook += Notify;
		}
		
		[SerializeField, BoxGroup("DEBUG")]
		private bool _doDebug;
		public bool DoDebug {
			get {
				return _doDebug;
			}
			set {
				_doDebug = value;
				SetChildren();
			}
		}
		
		public virtual void SetChildren() {
			this.Debug_OnValidate();
		}

		#pragma warning disable CS0253 // Possible unintended reference comparison; right hand side needs cast
		public bool Equals(IListen<TNotification> other) => this == other;
		#pragma warning restore CS0253 // Possible unintended reference comparison; right hand side needs cast
}
}