using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Essentials.Constants
{
	/// <summary>
	/// Corrected Among Us layer mask values.
	/// </summary>
    public static class AmongUs
    {
		public static readonly int ShipOnlyMask = LayerMask.GetMask(new string[]
		{
			"Ship"
		});

		public static readonly int ShipAndObjectsMask = LayerMask.GetMask(new string[]
		{
			"Ship",
			"Objects"
		});

		public static readonly int ShipAndAllObjectsMask = LayerMask.GetMask(new string[]
		{
			"Ship",
			"Objects",
			"ShortObjects"
		});

		public static readonly int NotShipMask = ~LayerMask.GetMask(new string[]
		{
			"Ship"
		});

		public static readonly int Usables = ~LayerMask.GetMask(new string[]
		{
			"Ship",
			"UI"
		});

		public static readonly int PlayersOnlyMask = LayerMask.GetMask(new string[]
		{
			"Players",
			"Ghost"
		});

		public static readonly int ShadowMask = LayerMask.GetMask(new string[]
		{
			"Shadow",
			"Objects",
			"IlluminatedBlocking"
		});
	}
}
