using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Utilities;

namespace EddiSpeechResponder
{
    public partial class ShowDiffWindow : Window
    {
        internal class DiffSegment : TextSegment
        {
            public DiffItem.DiffType type;
        }

        internal class DiffHighlighter : IBackgroundRenderer
        {
            private readonly Brush deletedBrush = Brushes.LightCoral;
            private readonly Brush addedBrush = Brushes.LightGreen;
            private readonly TextSegmentCollection<DiffSegment> diffSegments = new TextSegmentCollection<DiffSegment>();

            public void AddSegment(DiffSegment segment)
            {
                diffSegments.Add(segment);
            }

            // this defines which layer we draw in
            public KnownLayer Layer => KnownLayer.Selection;

            public void Draw(TextView textView, DrawingContext drawingContext)
            {
                if (textView == null) { throw new ArgumentNullException(nameof(textView)); }
                if (drawingContext == null) { throw new ArgumentNullException(nameof(drawingContext)); }
                if (diffSegments == null || !textView.VisualLinesValid) { return; }

                var visualLines = textView.VisualLines;
                if (visualLines.Count == 0) { return; }

                int viewStart = visualLines.First().FirstDocumentLine.Offset;
                int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
                var segmentsOnScreen = diffSegments.FindOverlappingSegments(viewStart, viewEnd - viewStart);
                foreach (DiffSegment segment in segmentsOnScreen)
                {
                    DrawSegment(segment, textView, drawingContext);
                }
            }

            private void DrawSegment(DiffSegment segment, TextView textView, DrawingContext drawingContext)
            {
                BackgroundGeometryBuilder geoBuilder = new BackgroundGeometryBuilder
                {
                    AlignToWholePixels = true,
                    BorderThickness = 0,
                    CornerRadius = 3,
                };

                Brush markerBrush;
                switch (segment.type)
                {
                    case DiffItem.DiffType.Deleted:
                        markerBrush = deletedBrush;
                        geoBuilder.AddSegment(textView, segment);
                        break;
                    case DiffItem.DiffType.Inserted:
                        markerBrush = addedBrush;
                        geoBuilder.AddSegment(textView, segment);
                        break;
                    default:
                        markerBrush = null;
                        break;
                }

                Geometry geometry = geoBuilder.CreateGeometry();
                if (geometry != null)
                {
                    drawingContext.DrawGeometry(markerBrush, null, geometry);
                }
            }
        }

        private readonly DiffHighlighter diffHighlighter = new DiffHighlighter();

        public ShowDiffWindow(string oldScript, string newScript)
        {
            InitializeComponent();
            scriptView.TextArea.TextView.BackgroundRenderers.Add(diffHighlighter);
            ParseDiffs(oldScript, newScript);
        }

        private void ParseDiffs(string oldScript, string newScript)
        {
            StringBuilder textBuilder = new StringBuilder();
            List<DiffItem> diffItems = Diff.DiffTexts(oldScript, newScript);

            foreach (DiffItem diffItem in diffItems)
            {
                switch (diffItem.type)
                {
                    case DiffItem.DiffType.Deleted:
                    case DiffItem.DiffType.Inserted:
                        DiffSegment segment = new DiffSegment()
                        {
                            StartOffset = textBuilder.Length,
                            Length = diffItem.data.Length,
                            type = diffItem.type
                        };
                        diffHighlighter.AddSegment(segment);
                        break;
                }
                textBuilder.AppendLine(diffItem.data);
            }

            try
            {
                string newline = Environment.NewLine;
                textBuilder.Remove(textBuilder.Length - newline.Length, newline.Length);
            }
            catch (ArgumentOutOfRangeException) { } // pass

            scriptView.Text = textBuilder.ToString();
        }
    }
}
