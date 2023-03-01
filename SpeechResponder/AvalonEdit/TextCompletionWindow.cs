using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utilities;

namespace EddiSpeechResponder.AvalonEdit
{
    public class TextCompletionWindow : CompletionWindow
    {
        public TextCompletionWindow ( TextArea textArea, string[] lookupKeys, IEnumerable<MetaVariable> metaVars ) : base( textArea )
        {
            // Hide the title bar and similar window stylings
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            BorderThickness = new Thickness( 0 );

            // Get the list to which completion data can be added
            IList<ICompletionData> data = this.CompletionList.CompletionData;

            var filteredMetaVars = metaVars
                .Where(v => string.Join(".", v.keysPath).StartsWith(string.Join(".", lookupKeys)));

            foreach ( var item in filteredMetaVars.OrderBy( v => string.Concat( v.keysPath, '.' ) ) )
            {
                if ( item.keysPath.Count > lookupKeys.Length &&
                     data.All( d => d.Text != item.keysPath[ lookupKeys.Length ] ) )
                {
                    data.Add( new TextCompletionItem( item.keysPath[ lookupKeys.Length ], item.description ) );
                }
            }

            if ( data.Count > 0 )
            {
                Show();
            }
        }
    }
}