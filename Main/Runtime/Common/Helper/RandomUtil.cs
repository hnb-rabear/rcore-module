/**
 * Author HNB-RaBear - 2019
 **/

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RCore
{
	public static class RandomExtension
	{
		public static void Shuffle<T>(this T[] array)
		{
			var rng = new System.Random();
			int n = array.Length;
			while (n > 1)
			{
				int k = rng.Next(n--);
				(array[n], array[k]) = (array[k], array[n]);
			}
		}
		public static void Shuffle<T>(this List<T> array)
		{
			var rng = new System.Random();
			int n = array.Count;
			while (n > 1)
			{
				int k = rng.Next(n--);
				(array[n], array[k]) = (array[k], array[n]);
			}
		}
	}

	public class RandomUtil
	{
		/// <summary>
		/// Return a random index from an array of chances
		/// NOTE: total of chances value does not need to match 100
		/// </summary>
		public static int GetRandomIndexOfChances(float[] chances)
		{
			int index = 0;
			float totalRatios = 0;
			for (int i = 0; i < chances.Length; i++)
				totalRatios += chances[i];

			float random = UnityEngine.Random.Range(0, totalRatios);
			float temp2 = 0;
			for (int i = 0; i < chances.Length; i++)
			{
				if (chances[i] <= 0)
					continue;

				temp2 += chances[i];
				if (temp2 > random)
				{
					index = i;
					break;
				}
			}
			return index;
		}
		public static int GetRandomIndexOfChances(int[] chances)
		{
			int index = 0;
			int totalRatios = 0;
			for (int i = 0; i < chances.Length; i++)
				totalRatios += chances[i];

			int random = UnityEngine.Random.Range(0, totalRatios + 1);
			int temp2 = 0;
			for (int i = 0; i < chances.Length; i++)
			{
				if (chances[i] <= 0)
					continue;

				temp2 += chances[i];
				if (temp2 > random)
				{
					index = i;
					break;
				}
			}
			return index;
		}
		/// <summary>
		/// Return a random index from a list of chances
		/// NOTE: total of chances value does not need to match 100
		/// </summary>
		public static int GetRandomIndexOfChances(List<float> chances)
		{
			int index = 0;
			float totalRatios = 0;
			for (int i = 0; i < chances.Count; i++)
				totalRatios += chances[i];

			float random = UnityEngine.Random.Range(0, totalRatios);
			float temp2 = 0;
			for (int i = 0; i < chances.Count; i++)
			{
				if (chances[i] <= 0)
					continue;

				temp2 += chances[i];
				if (temp2 > random)
				{
					index = i;
					break;
				}
			}
			return index;
		}
		public static int GetRandomIndexOfChances(List<int> chances)
		{
			int index = 0;
			int totalRatios = 0;
			for (int i = 0; i < chances.Count; i++)
				totalRatios += chances[i];

			int random = UnityEngine.Random.Range(0, totalRatios + 1);
			int temp2 = 0;
			for (int i = 0; i < chances.Count; i++)
			{
				if (chances[i] <= 0)
					continue;

				temp2 += chances[i];
				if (temp2 > random)
				{
					index = i;
					break;
				}
			}
			return index;
		}
		public static int GetRandomIndexOfChances(List<int> chances, int totalRatios)
		{
			int index = 0;
			int random = UnityEngine.Random.Range(0, totalRatios);
			int temp2 = 0;
			for (int i = 0; i < chances.Count; i++)
			{
				if (chances[i] <= 0)
					continue;

				temp2 += chances[i];
				if (temp2 > random)
				{
					index = i;
					break;
				}
			}
			return index;
		}
		public static int GetRandomEnum(Type type)
		{
			var values = Enum.GetValues(type);
			return (int)values.GetValue(Random.Range(0, values.Length));
		}
		public static List<Vector2> GetRandomPositions(int count, float radius, float minDistance, int safeLoops = 100)
		{
			var positions = new List<Vector2>();
			for (int i = 0; i < count; i++)
			{
				Vector2 newPos;
				int attempts = 0;
				bool validPosition;
				do
				{
					newPos = Random.insideUnitCircle * radius;

					validPosition = true;
					foreach (var pos in positions)
					{
						if (Vector2.Distance(newPos, pos) < minDistance)
						{
							validPosition = false;
							break;
						}
					}

					attempts++;
					if (attempts > safeLoops)
					{
#if UNITY_EDITOR
						Debug.LogWarning("Max attempts reached, some points may not be placed.");
#endif
						break;
					}
				} while (!validPosition);

				if (validPosition)
					positions.Add(newPos);
			}
			while (positions.Count < count)
			{
				var pos = Random.insideUnitCircle * radius;
				positions.Add(pos);
			}
			return positions;
		}
		public static string GetRandomCountryCode()
		{
			string[] countryCodes =
			{
				"US", "GB", "CA", "AU", "NZ", "IE", "PH", "ZA", "NG", "IN",
				"RU", "BY", "BG", "UA", "RS", "MK", "KZ", "KG", "MN",
				"CN", "HK", "TW", "SG",
				"JP",
				"KR",
				"SA", "AE", "EG", "IQ", "SY", "JO", "LB", "OM", "QA", "KW", "DZ", "LY",
				"VN",
				"TH"
			};
			return countryCodes[Random.Range(0, countryCodes.Length)];
		}
		public static string GetRandomString(int pLength)
		{
			const string chars = "abcdefghijklmnopqrstuvwxyz";
			var stringChars = new char[pLength];
			var random = new System.Random();
			for (int i = 0; i < stringChars.Length; i++)
				stringChars[i] = chars[random.Next(chars.Length)];
			var finalString = new String(stringChars);
			return finalString;
		}
	}
}