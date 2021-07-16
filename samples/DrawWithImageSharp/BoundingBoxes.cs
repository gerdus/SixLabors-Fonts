// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DrawWithImageSharp
{
    public static class BoundingBoxes
    {
        public static void Generate(string text, Font font)
        {
            using var img = new Image<Rgba32>(1000, 1000);
            img.Mutate(x => x.Fill(Color.White));

            FontRectangle box = TextMeasurer.MeasureBounds(text, new RendererOptions(font));
            (IPathCollection paths, IPathCollection boxes) = GenerateGlyphsWithBox(text, new RendererOptions(font));

            Rgba32 f = Color.Fuchsia;
            f.A = 128;

            img.Mutate(x => x.Fill(Color.Black, paths)
                            .Draw(f, 1, boxes)
                            .Draw(Color.Lime, 1, new RectangularPolygon(box.Location, box.Size)));

            img.Save("Output/Boxed.png");
        }

        /// <summary>
        /// Generates the shapes corresponding the glyphs described by the font and with the setting ing withing the FontSpan
        /// </summary>
        /// <param name="text">The text to generate glyphs for</param>
        /// <param name="style">The style and settings to use while rendering the glyphs</param>
        /// <returns>The paths, boxes, and text box.</returns>
        private static (IPathCollection paths, IPathCollection boxes) GenerateGlyphsWithBox(string text, RendererOptions style)
        {
            var glyphBuilder = new CustomGlyphBuilder(Vector2.Zero);

            var renderer = new TextRenderer(glyphBuilder);

            renderer.RenderText(text, style);

            return (glyphBuilder.Paths, glyphBuilder.Boxes);
        }
    }
}
