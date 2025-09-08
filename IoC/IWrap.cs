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


namespace Levels.Core {
	using System;

	using Levels.Core.IoC;

	public interface IWrap {
		public Type GetWrappedType();

		public Object GetWrapped();
	}

	/// <summary>
	/// Not sure if this should be here, it is more declaring intent for DI.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IWrap<T> : IWrap, IInject<T> {
		Type IWrap.GetWrappedType() {
			return typeof(T);
		}

		Object IWrap.GetWrapped() {
			return this.Get().Get();
		}
	}

	public static class WrapExtensions {
		public static IWrap Interface_IWrap<T, I>(this I instance) where I : IWrap
			=> instance;

		public static IWrap<T> Interface_IWrapT<T, I>(this I instance) where I : IWrap<T>
			=> instance;

		public static Type GetWrappedType<T, I>(this I instance) where I : IWrap
			=> instance.GetWrappedType();

		public static Object GetWrapped<T, I>(this I instance) where I : IWrap
			=> instance.GetWrapped();
	}
}