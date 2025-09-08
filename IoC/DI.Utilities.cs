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
	public static partial class Utilities {
		public static void RegisterToScope<T>(T reference, Scope.ID iD, string key = "", object source = null) {
			Exports<T>.Register(iD, Entry.Factory.Service<T>(reference, key, source));
		}

		public static void UnregisterFromScope<T>(T reference, Scope.ID iD, string key = "", object source = null) {
			Exports<T>.Unregister(iD, Entry.Factory.Service<T>(reference, key, source));
		}

		public static Scope.ID GetScopeID(int  ID) {
			return new Scope.ID(ID);
		}
	}
}

