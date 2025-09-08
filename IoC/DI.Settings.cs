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

	[System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Interface), Serializable]
	public sealed class Settings
			: System.Attribute {
		private static Settings _default = new Settings();
		public static Settings Default { get => _default; set => _default = value; }


		//<todo> implement these </todo>
		public bool UseLocalSettings = false;

		public bool DoInjectMethods = false;
		public bool DoInjectConstructor = true;
		public bool DoInjectInterface = false;
		public bool DoInjectPublicFields = false;
		public bool RegisterNotificationReceiver = true;

		/// <summary>
		/// Will default to key="" if the Registery has no entry with provided key.
		/// </summary>
		public bool UseDefaultKeyOnMissing = false;

		/// <summary>
		/// Will pass the key to the type being injected as tags.
		/// </summary>
		public bool UseKeyAsTag = false;

		[Obsolete("TODO: setup reflection injection.")]
		public bool DoAutoInject = true;

		public bool IsInjected = false;

		public bool DoRetrieveFromRoot = false;
		public bool DoRetrieveFromParents = true;

		[Obsolete("Create Instance Entry Instead")]
		public bool DoCreateInstance = true;

		public bool GetDoInjectPublicFields() {
			return DoInjectPublicFields;
		}

		public string Readout() {
			return $"[InjectInterface:{DoInjectInterface}][InjectPublicFields:{DoInjectPublicFields}][DefaultTags:{UseDefaultKeyOnMissing}]";
		}
	}
}