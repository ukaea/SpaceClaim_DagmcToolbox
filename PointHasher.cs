using System;
using System.Collections.Generic;
using System.Diagnostics;
using HashItemType=System.UInt32;
using PointHashType=System.UInt64;
using PointType = SpaceClaim.Api.V19.Geometry.Point;
using BoxType = SpaceClaim.Api.V19.Geometry.Box;


namespace Dagmc_Toolbox
{
	/// <summary>
	/// fast and reversible, perfect/no-collision/unique 3D coordinate hash, for quick point existing check.
	/// other approaches: boundboxTree
	/// currently, assuming double values will not serialized, so no precision loss /rounding is needed
	/// </summary>
	/// <remarks>
	/// the current porting from parallel-preprocessor `UniqueId` may not work, 
	/// as Half's precision is not equal space, when value is big, can not distinguish points close enough
	/// it is worth of checking if boundbox is out of Half value range
	/// find out the GetHashCode of Point class
	/// linear scaled into an equally-distanced 3D box mesh,
	/// if mesh space is less than min length sqrt(2)/2 of min mesh edge lenght 
	/// 2 bounday layers joining at a corner is bit tricky, 2 mesh points may be to closed
	/// </remarks>
	class PointHasher
	{
		// count of float point numbers to be converted
		private readonly int ID_ITEM_COUNT = 3;
		// each float number is converted into ID_ITEM_COUNT unsigned int
		private readonly int ID_ITEM_BITS = 21;  // 64bit dived into 3 parts
		private readonly HashItemType ID_ITEM_MASK = 0x1fffff;  // 21bit

		private double[] maxValues;
		private double[] minValues;
		private double[] gridSteps;
		private double minStep;

		/// <summary>
		/// This hasher must be initialized by the boundbox
		/// </summary>
		/// <param name="box"></param>
		public PointHasher(BoxType box)
        {
			maxValues = new double[] { box.MaxCorner.X, box.MaxCorner.Y, box.MaxCorner.Z};
			minValues = new double[] { box.MinCorner.X, box.MinCorner.Y, box.MinCorner.Z};
			var nStep = 1U >> ID_ITEM_BITS - 1;
			gridSteps = new double[] { (maxValues[0] - minValues[0]) / nStep, 
					maxValues[1] - minValues[1], maxValues[2] - minValues[2] };
			minStep = Math.Min(Math.Min(gridSteps[0], gridSteps[1]), gridSteps[2]);
		}

		/// <summary>
		/// This hasher must be initialized by the boundbox
		/// </summary>
		/// <param name="box"></param>
		public PointHasher(PointType MinCorner, PointType MaxCorner)
		{
			maxValues = new double[] { MaxCorner.X, MaxCorner.Y, MaxCorner.Z };
			minValues = new double[] { MinCorner.X, MinCorner.Y, MinCorner.Z };
			var nStep = 1U >> ID_ITEM_BITS - 1;
			gridSteps = new double[] { (maxValues[0] - minValues[0]) / nStep,
					maxValues[1] - minValues[1], maxValues[2] - minValues[2] };
			minStep = Math.Min(Math.Min(gridSteps[0], gridSteps[1]), gridSteps[2]);
		}

		bool IsDistinshable(double minDist)
        {
			return minDist > Math.Sqrt(2) * minStep;
        }

		double LENGTH_SCALE = 1;  // not quite needed, set as 1 (no effect)

		// if the value is close to zero after scaling, regarded as zero in comparison
		/// point hash is quite diff from paralle-preprocessor's geometry hash, which use a much bigger tol
		/// this also depends on scale
		private  readonly double ZERO_THRESHOLD = 1e-4;  // assuming input length unit is meter!

		/// rounding: mask out east significant bits for approximately comparison
		private  readonly HashItemType ROUND_PRECISION_MASK = 0x0000;

		/// <summary>
		/// only needed if value may change within a given presion, the id may change bit
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		private  SortedSet<PointHashType> nearbyIds(PointHashType id)
		{
			SortedSet<PointHashType> ids = new SortedSet<PointHashType>();
			List<List<PointHashType>> items = new List<List<PointHashType>>();
			for (int i = 0; i < ID_ITEM_COUNT; i++)
			{
				PointHashType s = ROUND_PRECISION_MASK * (1U << ID_ITEM_BITS * i);
				var l = new List<PointHashType> { id - s, id, id + s };
				items.Add(l); // overflow and underflow is fine
			}
			// generate combination,  3**3, generate ijkl index
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					for (int k = 0; k < 3; k++)
					{
						PointHashType shift = items[0][i] + items[0][j] + items[0][k];
						ids.Add(id + shift);
					}
				}
			}
			return new SortedSet<PointHashType>(ids);
		}

		private  bool HashEqual(PointHashType id1, PointHashType id2)
		{
			return id1 == id2;
			// below is a safer but much slower approach
			//var nearby_ids = nearbyIds(id2);
			//return nearby_ids.Contains(id1);
		}

		HashItemType ToHashItem(double v, int dim)
        {
			Debug.Assert( v >= minValues[dim] && v <= maxValues[dim]);
			return (HashItemType)(Math.Round((v - minValues[dim]) / gridSteps[dim])); 
        }

		// can not get exact, but match to the grid , precision is round precision
		double FromHashItem(HashItemType hv, int dim)
		{
			return (minValues[dim] + hv * gridSteps[dim]);
		}

		/*
		internal void ToHashItems(double value, ref List<HashItemType> items)
		{
			for (int i = 0; i < ID_ITEM_COUNT; i++)
			{
				//HashItemType round_precision = ROUND_PRECISION_MASK;
				if (Math.Abs(value) < ZERO_THRESHOLD * LENGTH_SCALE)
				{
					value = 0.0;
				}
				items[i] = ToIndex(value); 
				//  todo: Math.Round(p, round_precision);
			}
		}
		*/
		internal List<double> ValuesFromHash(PointHashType id)
        {
			List<double> values = new List<double>();
			for (int i = 0; i < ID_ITEM_COUNT; i++)
			{
				HashItemType hv = (HashItemType)((id >> (i * ID_ITEM_BITS)) & ID_ITEM_MASK);  //  `id / 2**(ID_ITEM_BITS*i)`
				double v = FromHashItem(hv, i);
				values.Add(v);  
			}
			return values;
		}

		/// used by GeometryPropertyBuilder class in parallel-preprocessor
		/// assuming native endianess, always calculate Id using the same endianness
		/// this should generate unique Id for a vector of 4 double values
		/// this is not universal unique Id (UUID) yet, usurally std::byte[16]
		public PointHashType PointToHash(in PointType p)
		{
			PointHashType ret = 0x00000000;
			List<double> values = new List<double> { p.X*LENGTH_SCALE, p.Y * LENGTH_SCALE, p.Z * LENGTH_SCALE};

			Debug.Assert(values.Count == ID_ITEM_COUNT);
			for (int i = 0; i < values.Count; i++)
			{
				PointHashType tmp = ToHashItem(values[i], i);
				ret ^= (tmp << (int)i * ID_ITEM_BITS); // XOR is fine, but  OR is more readable
			}
			return ret;
		}

		public PointType HashToPoint(PointHashType id)
        {
			List<double> values = ValuesFromHash(id);

			return PointType.Create(values[0] / LENGTH_SCALE, values[1] / LENGTH_SCALE, values[2] / LENGTH_SCALE);
		}

		public static void Test()
        {
			var minP = PointType.Create(0, 0, 0);
			var maxP = PointType.Create(100, 100, 100);
			PointHasher hasher = new PointHasher(minP, maxP);

			Debug.WriteLine("Test using Debug.Assert for PointHasher class");
			Debug.WriteLine("Hash point (1,2,3) = 0x{0:X}", hasher.PointToHash(PointType.Create(1, 2, 3)));
			Debug.WriteLine("Hash point for max corner = 0x{0:X}", hasher.PointToHash(maxP));
			Debug.Assert(hasher.PointToHash(minP) == 0UL);
			Debug.Assert(hasher.PointToHash(minP) != hasher.PointToHash(PointType.Create(0, 1, 2)));
		}

	}
}