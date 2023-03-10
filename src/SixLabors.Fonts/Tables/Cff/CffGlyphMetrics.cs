// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using SixLabors.Fonts.Tables.General;
using SixLabors.Fonts.Unicode;

namespace SixLabors.Fonts.Tables.Cff
{
    /// <summary>
    /// Represents a glyph metric from a particular Compact Font Face.
    /// </summary>
    internal class CffGlyphMetrics : GlyphMetrics
    {
        private static readonly Vector2 MirrorScale = new(1, -1);
        private CffGlyphData glyphData;

        internal CffGlyphMetrics(
            StreamFontMetrics fontMetrics,
            ushort glyphId,
            CodePoint codePoint,
            CffGlyphData glyphData,
            Bounds bounds,
            ushort advanceWidth,
            ushort advanceHeight,
            short leftSideBearing,
            short topSideBearing,
            ushort unitsPerEM,
            TextAttributes textAttributes,
            TextDecorations textDecorations,
            GlyphType glyphType = GlyphType.Standard,
            GlyphColor? glyphColor = null)
            : base(
                  fontMetrics,
                  glyphId,
                  codePoint,
                  bounds,
                  advanceWidth,
                  advanceHeight,
                  leftSideBearing,
                  topSideBearing,
                  unitsPerEM,
                  textAttributes,
                  textDecorations,
                  glyphType,
                  glyphColor)
            => this.glyphData = glyphData;

        internal CffGlyphMetrics(
            StreamFontMetrics fontMetrics,
            ushort glyphId,
            CodePoint codePoint,
            CffGlyphData glyphData,
            Bounds bounds,
            ushort advanceWidth,
            ushort advanceHeight,
            short leftSideBearing,
            short topSideBearing,
            ushort unitsPerEM,
            Vector2 offset,
            Vector2 scaleFactor,
            TextRun textRun,
            GlyphType glyphType = GlyphType.Standard,
            GlyphColor? glyphColor = null)
            : base(
                  fontMetrics,
                  glyphId,
                  codePoint,
                  bounds,
                  advanceWidth,
                  advanceHeight,
                  leftSideBearing,
                  topSideBearing,
                  unitsPerEM,
                  offset,
                  scaleFactor,
                  textRun,
                  glyphType,
                  glyphColor)
            => this.glyphData = glyphData;

        /// <inheritdoc/>
        internal override GlyphMetrics CloneForRendering(TextRun textRun)
            => new CffGlyphMetrics(
                this.FontMetrics,
                this.GlyphId,
                this.CodePoint,
                this.glyphData,
                this.Bounds,
                this.AdvanceWidth,
                this.AdvanceHeight,
                this.LeftSideBearing,
                this.TopSideBearing,
                this.UnitsPerEm,
                this.Offset,
                this.ScaleFactor,
                textRun,
                this.GlyphType,
                this.GlyphColor);

        /// <inheritdoc/>
        internal override void RenderTo(IGlyphRenderer renderer, float pointSize, Vector2 location, TextOptions options)
        {
            // https://www.unicode.org/faq/unsup_char.html
            if (ShouldSkipGlyphRendering(this.CodePoint))
            {
                return;
            }

            float dpi = options.Dpi;
            location *= dpi;
            float scaledPPEM = dpi * pointSize;
            bool forcePPEMToInt = (this.FontMetrics.HeadFlags & HeadTable.HeadFlags.ForcePPEMToInt) != 0;

            if (forcePPEMToInt)
            {
                scaledPPEM = MathF.Round(scaledPPEM);
            }

            FontRectangle box = this.GetBoundingBox(location, scaledPPEM);

            GlyphRendererParameters parameters = new(this, this.TextRun, pointSize, dpi);
            if (renderer.BeginGlyph(in box, in parameters))
            {
                if (!ShouldRenderWhiteSpaceOnly(this.CodePoint))
                {
                    if (this.GlyphColor.HasValue && renderer is IColorGlyphRenderer colorSurface)
                    {
                        colorSurface.SetColor(this.GlyphColor.Value);
                    }

                    Vector2 scale = new Vector2(scaledPPEM) / this.ScaleFactor * MirrorScale;
                    Vector2 offset = location + (this.Offset * scale * MirrorScale);
                    this.glyphData.RenderTo(renderer, scale, offset);
                }

                this.RenderDecorationsTo(renderer, location, scaledPPEM);
            }

            renderer.EndGlyph();
        }
    }
}
