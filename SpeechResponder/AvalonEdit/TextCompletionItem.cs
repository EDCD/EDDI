using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows;
using System.Windows.Media;

namespace EddiSpeechResponder.AvalonEdit
{
    public class TextCompletionItem : ICompletionData
    {
        public TextCompletionItem ( string text, Type type, string description = null )
        {
            this.Text = text;
            this.Description = description;
        }

        public ImageSource Image => null;

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the drop down list.
        public object Content => this.Text;

        public object Description { get; private set; }

        public double Priority => 0;

        public void Complete ( TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs )
        {
            textArea.Document.Replace( completionSegment, this.Text );
        }
    }
}