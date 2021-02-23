using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmidtFramework
{
	namespace Utility
	{
		public class Pair<T, U>
		{
			public T First { get; set; }
			public U Second { get; set; }

			public Pair(T first, U second)
			{
				First = first;
				Second = second;
			}

			public Pair()
			{
				First = default;
				Second = default;
			}

			public override bool Equals(object obj)
			{
				return obj is Pair<T, U> pair &&
					   EqualityComparer<T>.Default.Equals(First, pair.First) &&
					   EqualityComparer<U>.Default.Equals(Second, pair.Second);
			}

			public override int GetHashCode()
			{
				var hashCode = 43270662;
				hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(First);
				hashCode = hashCode * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Second);
				return hashCode;
			}
		}
	}
}
