﻿namespace Blazor.Diagrams.Core.Models.Core
{
    public class Size
    {
        public static Size Zero { get; } = new Size(0, 0);

        public Size() { }

        public Size(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double Width { get; set; }
        public double Height { get; set; }

        public bool Equals(Size size) => size != null && Width == size.Width && Height == size.Height;

        public override string ToString() => $"Size(width={Width}, height={Height})";
    }
}
