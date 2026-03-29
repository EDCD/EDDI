using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiSpeechResponder.ScriptResolverService
{
    internal class TemplateBuilder
	{
        private List<TemplateItem> templates = [ ];

		public void Append ( string scriptName, string scriptValue, bool addPrependSeparator )
        {
            if ( addPrependSeparator )
            {
                scriptValue += Environment.NewLine + "{_ End of prepended script " + scriptName + @" }";
			}
			templates = templates.Append(new TemplateItem(scriptName, scriptValue ) ).ToList();
		}

        public string Render ()
        {
            return string.Join( Environment.NewLine, templates.Select( t => t.itemValue ) );
        }

        public void FetchTemplateItemFromLine ( int line, out string scriptName, out int scriptLine  )
        {
            scriptName = null;
            scriptLine = 0;

            var cumulativeLines = 0;
            foreach ( var template in templates )
            {
                scriptName = template.itemName;
                if ( ( cumulativeLines + template.itemLines ) >= line)
                {
                    scriptLine = line - cumulativeLines;
                    return;
                }
                cumulativeLines += template.itemLines;
            }
        }

        public void FetchTemplateItemFromOffset(string script, int offset, out string scriptName, out int scriptLine)
        {
            scriptName = null;
            scriptLine = 0;

			var lines = script.Split( [ "\r\n" ], StringSplitOptions.None);
            var cumulativeLength = 0;

            var lineNumber = 1; 
            foreach (var line in lines)
            {
                cumulativeLength += line.Length + "\r\n".Length;
                if (cumulativeLength >= offset)
                {
                    FetchTemplateItemFromLine( lineNumber, out scriptName, out scriptLine );
                    return;
                }
                lineNumber += 1;
            }
        }

		private class TemplateItem
        {
            protected internal string itemName { get; }
            protected internal string itemValue { get; }

            protected internal int itemLines =>
                ( itemValue ?? string.Empty ).Split( [ "\r\n" ], StringSplitOptions.None ).Length;

            protected internal TemplateItem ( string itemName, string itemValue )
            {
                this.itemName = itemName;
                this.itemValue = itemValue;
            }
        }
    }
}
