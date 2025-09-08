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
	using Levels.Core.IoC.Mediator;


	// TODO : Figure out if I can turn this into a Pipe
	public struct Chain<T>
		where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {

		public struct Settings {
			public bool doCancleAcrossScope;
			public bool doIgnoreCancels;
		}

		public Settings settings;

		// [XAN]DARN YOU UNITY - [Future XAN] Gotta put a reason for these.
		public readonly List<Chain.Responce<T>> Responsabilities;

		public Chain(
			Settings settings) {

			Responsabilities = new List<Chain.Responce<T>>();

			this.settings = settings;
		}

		public void PushToResponsabilities(
			ref T notification,
			ref CancelToken cancelToken) {
			CancelToken cancel = CancelToken.False;

			foreach (int i in Responsabilities.Count) {
				Responsabilities[i].Handle(ref notification, ref cancel);

				if (cancel.IsCancelled) {
					cancelToken = cancel;
				}
			}
		}

		public void SubscribeResponsability(
			ref Chain.Responce<T> toSub) {

			if (Responsabilities.Contains(toSub))
				return;

			Responsabilities.Add(toSub);

			_OrderChain();
		}

		private void _OrderChain() {
			Responsabilities.Sort((a, b) => a.Weight.CompareTo(b.Weight));
		}

		public void UnsubscribeResponsability(
			ref Chain.Responce<T> toUnsub) {

			if (!Responsabilities.Contains(toUnsub))
				return;

			Responsabilities.Remove(toUnsub);
		}
	}

	/// <summary>
	/// The conceptual level of Chain of Responsability
	/// </summary>
	namespace Chain {
		// TODO : If we want this to be a Take, we have to have a special type that also takes the CancelToken(like a struct or tuple).
		public interface IRespond<T> {
			public bool Handle(ref T message, ref CancelToken token);

			public object Source {
				get;
			}

			public float Weight {
				get;
			}
		}

		public interface ImResponce<T> : IRegister, IRespond<T>, IEquatable<ImResponce<T>> where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {

		}

		public class Responce<T> : ImResponce<T> where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {
			private object _source;
			public object Source { get { return _source; } set { _source = value; } }

			private Scope.ID _scope;
			Scope.ID Scope {
				get => _scope;
			}
			Scope.ID scope {
				get => _scope;
				set => _scope = value;
			}

			//TODO: What does this do?
			private float _weight;
			public float Weight => _weight;

			private MixedFunc<T, CancelToken, bool> _handle;
			public MixedFunc<T, CancelToken, bool> Handler {
				get => _handle; set => _handle = value;
			}

			public Responce(float weight = 0, object source = null, MixedFunc<T, CancelToken, bool> handle = null) {
				Setup(weight, source, handle);
			}

			public void Setup(float weight, object source, MixedFunc<T, CancelToken, bool> handle) {
				_weight = weight;
				_source = source;
				_handle = handle;
				_scope = default;
			}

			public bool Handle(ref T message, ref CancelToken token) {

				if (_handle != null)
					return _handle.Invoke(in message, ref token);

				return true;
			}

			public bool Equals(ImResponce<T> other) => object.ReferenceEquals(this, other);

			public void Register(in Scope.ID scope) {
				_scope = scope;

				Collection<T>.SubToChain(scope, this);
			}

			public void Unregister() {
				Collection<T>.UnsubFromChain(_scope, this);
			}
		}

		public partial class Responce {
			public static class factory<T> where T : struct, INotification, IEquatableRef<T>, IConvertible<T> {
				public static Responce<T> CreateCancleAndIntercept(Action intercept = null, float weight = -1, object source = null) {
					return new Responce<T>(weight, source, (in T notice, ref CancelToken token) => {
						if (intercept != null) {
							intercept.Invoke();
						}
						token = CancelToken.True;
						return true;
					});
				}

				public static Responce<T> CreateCancleIfAndIntercept(Func<bool> flag, Action intercept = null, float weight = -1, object source = null) {
					return new Responce<T>(weight, source, (in T notice, ref CancelToken token) => {
						if (!flag()) {
							token = !token.Equals(CancelToken.True) ? CancelToken.False : token;
							return true;
						}

						if (intercept != null) {
							intercept.Invoke();
						}

						token = CancelToken.True;
						return false;
					});
				}
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