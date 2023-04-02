namespace EddiSpeechResponder.AvalonEdit
{
    /// <summary>
    /// Interaction logic for TextCompletionControl.xaml
    /// </summary>
    public partial class TextCompletionControl
    {
        public string keyword { get; }
        public string type { get; }
        public string description { get; }

        public TextCompletionControl ( string keyword, string type, string description )
        {
            this.keyword = keyword;
            this.type = type;
            this.description = description;
            InitializeComponent();
        }
    }
}
