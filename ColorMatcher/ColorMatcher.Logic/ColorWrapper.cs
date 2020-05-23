using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ColorMatcher.Logic
{
    /// <summary>
    /// A wrapper round Color that makes it easier to deserialize from the desired human readable RGB format.
    /// </summary>
    public class ColorWrapper
    {
        public Color Color { get; set; }

        public byte Red
        {
            get { return Color.R; }
            set { SetColor(value, Green, Blue); }
        }
        public byte Green
        {
            get { return Color.G; }
            set { SetColor(Red, value, Blue); }
        }

        public byte Blue
        {
            get { return Color.B; }
            set { SetColor(Red, Green, value); }
        }
        public ColorWrapper(Color color)
        {
            this.Color = color;
        }

        public ColorWrapper()
        {
            // default constructor for serialization
        }

        public void SetColor(byte red, byte green, byte blue)
        {
            this.Color = Color.FromArgb(255, red, green, blue);
        }
    }
}
