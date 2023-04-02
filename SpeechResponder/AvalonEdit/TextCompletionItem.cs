using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EddiSpeechResponder.AvalonEdit
{
    public class TextCompletionItem : ICompletionData
    {
        public TextCompletionItem ( string text, Type type, string description = null )
        {
            // Cottle value names are typically suffixed with "Value". This can be removed as redundant.
            this.Text = text;
            this.Description = description;
            this.Type = type.Name.Replace( "Value", "" );
            this.CalculatedMinWidth = GetMinWidth();
        }

        public ImageSource Image => null;

        public string Text { get; }
        
        // Use this property if you want to show a fancy UIElement in the drop down list.
        public object Content => Control;

        public object Description { get; }

        public double Priority => 0;

        public void Complete ( TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs )
        {
            textArea.Document.Replace( completionSegment, this.Text );
        }

        public double CalculatedMinWidth { get; }

        public double Width { get; set; }

        private string Type { get; }

        private TextCompletionControl Control => new TextCompletionControl( Text, Type, Description?.ToString() )
        {
            Width = this.Width
        };

        private static Size MeasureTextBlock ( string candidate, TextBlock textBlock )
        {
            if ( string.IsNullOrEmpty( candidate ) || textBlock is null )
            { return new Size(); }
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);
            return new Size( 
                formattedText.Width + textBlock.Margin.Right + textBlock.Margin.Left, 
                formattedText.Height + textBlock.Margin.Top + textBlock.Margin.Bottom );
        }

        private double GetMinWidth ()
        {
            if ( Control is null ) { return 0; }
            var longestKeywordLength = MeasureTextBlock( Text ?? string.Empty, Control.keywordTextBlock ).Width;
            var longestTypeLength = MeasureTextBlock( Type ?? string.Empty, Control.typeTextBlock ).Width;
            return longestKeywordLength + longestTypeLength;
        }
    }
}