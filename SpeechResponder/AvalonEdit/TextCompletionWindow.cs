using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace EddiSpeechResponder.AvalonEdit
{
    public class TextCompletionWindow : CompletionWindow
    {
        public TextCompletionWindow ( [NotNull] TextArea textArea, [NotNull, ItemNotNull] List<TextCompletionItem> items ) : base( textArea )
        {
            // Hide the title bar and similar window stylings
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            BorderThickness = new Thickness( 0 );

            // Determine the width of the widest control and apply it to each child control
            var longestControl = items
                .OrderByDescending( d => d.CalculatedMinWidth )
                .FirstOrDefault();
            if ( longestControl != null )
            {
                items.ForEach( i =>
                    {
                        i.Width = longestControl.CalculatedMinWidth;
                    }
                );
                Width = longestControl.CalculatedMinWidth + 50;
            }

            // Add items to the completion list
            foreach ( var item in items )
            {
                CompletionList.CompletionData.Add( item );
            }

            Show();
        }
    }
}