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
	using System;
	
	using UnityEngine;
	
	using Core;
	using Core.IoC;
	#endregion

	/// <summary>
	/// 
	/// </summary>
	/// <todo>
	/// Cleanup
	/// In-Out
	/// </todo>
	[DefaultExecutionOrder(32000)]
	public class _ScopeEnd_ : MonoBehaviour {
		private _Scope_ _scope;
		private Scope.ID _id;
		private bool isRoot;

		public bool isRootEnd;

		public bool Debug_Injectables;

		public event Action AwakeCallback;

		// Start is called before the first frame update
		public void Awake() {
			AwakeCallback?.Invoke();
		}
		
		public void SetScope(IoC._Scope_ scope) {
			_scope = scope;
			isRoot = scope.isRoot;
			_id = scope.Get().Get().ScopeID;
		}

		public void SetAsRoot(bool value) {
			isRoot = value;
		}

		public void SetID(Scope.ID value) {
			_id = value;
		}
	}
}