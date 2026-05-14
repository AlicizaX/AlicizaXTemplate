using System;
using Game.Config;
using UnityEngine;

public static class ExternalTypeUtil
{
    public static UnityEngine.Vector2 NewVector2(vec2 v)
    {
        return new UnityEngine.Vector2(v.X, v.Y);
    }

    public static UnityEngine.Vector3 NewVector3(vec3 v)
    {
        return new UnityEngine.Vector3(v.X, v.Y, v.Z);
    }

    public static UnityEngine.Vector4 NewVector4(vec4 v)
    {
        return new UnityEngine.Vector4(v.X, v.Y, v.Z, v.W);
    }

    public static Color ParseColor(color color)
    {
        return ParseColor(color.Value);
    }

    public static Color ParseColor(string colorStr)
    {
        try
        {
            if (colorStr.StartsWith("#"))
                return HexToColor(colorStr);
            return SplitToColor(colorStr);
        }
        catch
        {
            Debug.LogWarning($"无效颜色值: {colorStr}");
            return Color.magenta; // 错误提示色
        }
    }
    private static Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");
        if (hex.Length == 3) hex += "F"; // 补全Alpha通道
        if (hex.Length == 6) hex += "FF";
        var r = Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
        var g = Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
        var b = Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;
        var a = Convert.ToInt32(hex.Substring(6, 2), 16) / 255f;
        return new Color(r, g, b, a);
    }

    private static Color SplitToColor(string str)
    {
        var parts = str.Split(',');
        return new Color(
            float.Parse(parts[0]),
            float.Parse(parts[1]),
            parts.Length > 2 ? float.Parse(parts[2]) : 1f,
            parts.Length > 3 ? float.Parse(parts[3]) : 1f);
    }
}
