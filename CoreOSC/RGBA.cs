﻿namespace LucHeart.CoreOSC;

// ReSharper disable once InconsistentNaming
public readonly struct RGBA
{
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly byte A;

    public RGBA(byte red, byte green, byte blue, byte alpha)
    {
        R = red;
        G = green;
        B = blue;
        A = alpha;
    }

    public override bool Equals(object obj) => obj switch
    {
        RGBA rgba => R == rgba.R && G == rgba.G && B == rgba.B && A == rgba.A,
        byte[] byteObj => R == byteObj[0] && G == byteObj[1] && B == byteObj[2] && A == byteObj[3],
        _ => false
    };

    public static bool operator == (RGBA a, RGBA b) => a.Equals(b);
    public static bool operator != (RGBA a, RGBA b) => !a.Equals(b);
    public override int GetHashCode() => (R << 24) + (G << 16) + (B << 8) + A;
}