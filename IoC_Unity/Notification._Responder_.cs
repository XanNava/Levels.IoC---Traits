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
	#region Usings
	using Levels.Core;
	using Levels.Core.IoC;
	using Levels.Core.General;

	using Sirenix.OdinInspector;

	using UnityEngine;
	using Levels.Core.IoC.Chain;
	#endregion

	public class _Responder_<TNotification, TType> : MonoBehaviour, IWrap<Responce<TNotification>>, IRegister
		where TNotification : struct, INotification<TType>, IEquatableRef<TNotification>, IConvertible<TNotification> {

		protected Responce<TNotification> _Source { get; set; } = new();
		Inject<Responce<TNotification>> IInject<Responce<TNotification>>.In1 { get => _Source; set => _Source = value; }

		public object source { get; set; }

		#if UNITY_EDITOR
		[SerializeField, Show_InfoLabel(""), BoxGroup("SETTINGS"), HideLabel]
		private bool _;
		#endif

		public void Register(in Scope.ID scope) {
			this.Get().Get().Register(scope);
			this.Get().Scope = scope;
			this.source = source;
		}

		public void Unregister() {
			this.Get().Get().Unregister();
		}

		#if UNITY_EDITOR
		[SerializeField, ReadOnly, BoxGroup("STATE"), InlineProperty]
		private TType _LastContext;
		[SerializeField, ReadOnly, BoxGroup("STATE")]
		private float _LastCalledOn;
		#endif

		// TODO : Get rid of bool return, just use token. Maybe?
		protected virtual bool Handle(in TNotification arg1, ref CancelToken arg2) {
			#if UNITY_EDITOR
			_LastContext = arg1.Context;
			_LastCalledOn = Time.time;
			#endif

			return false;
		}
	}
}
