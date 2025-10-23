using System;
using System.Collections.Generic;
using System.Text;

namespace ExamplePlugin.UI.Branding.Panel
{
    public readonly struct Insets
    {
        public readonly float Left, Right, Top, Bottom;
        public Insets(float l, float r, float t, float b) { Left = l; Right = r; Top = t; Bottom = b; }

        public static Insets Uniform(float v) => new Insets(v, v, v, v);
    }
}
