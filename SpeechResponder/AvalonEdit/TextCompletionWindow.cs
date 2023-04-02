using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using JetBrains.Annotations;
using System.Collections.Generic;
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
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;

            foreach ( var item in items )
            {
                CompletionList.CompletionData.Add( item );
            }
            Show();
        }
    }
}