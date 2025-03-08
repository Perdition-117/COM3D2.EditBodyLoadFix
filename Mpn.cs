using System;

namespace COM3D2.EditBodyLoadFix;

internal static class Mpn {
	public static readonly MPN body = (MPN)Enum.Parse(typeof(MPN), "body");

	public static readonly MPN MuneL = (MPN)Enum.Parse(typeof(MPN), "MuneL");
	public static readonly MPN MuneS = (MPN)Enum.Parse(typeof(MPN), "MuneS");
	public static readonly MPN MuneTare = (MPN)Enum.Parse(typeof(MPN), "MuneTare");
	public static readonly MPN MuneUpDown = (MPN)Enum.Parse(typeof(MPN), "MuneUpDown");
	public static readonly MPN MuneYori = (MPN)Enum.Parse(typeof(MPN), "MuneYori");
	public static readonly MPN MuneYawaraka = (MPN)Enum.Parse(typeof(MPN), "MuneYawaraka");

	public static readonly MPN EyeBallPosY = (MPN)Enum.Parse(typeof(MPN), "EyeBallPosY");
	public static readonly MPN EyeBallSclX = (MPN)Enum.Parse(typeof(MPN), "EyeBallSclX");
	public static readonly MPN EyeBallSclY = (MPN)Enum.Parse(typeof(MPN), "EyeBallSclY");
}
