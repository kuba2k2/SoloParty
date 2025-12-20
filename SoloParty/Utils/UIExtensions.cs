using System;
using HMUI;
using UnityEngine;

namespace SoloParty.Utils;

public static class UIExtensions
{
	public static void SetHint(this Component component, string text)
	{
		try
		{
			component.gameObject.GetComponentInChildren<HoverHint>()?.text = text;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}
}
