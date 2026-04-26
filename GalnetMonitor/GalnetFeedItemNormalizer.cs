using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.ServiceModel.Syndication;
using Utilities;

namespace EddiGalnetMonitor
{
    /// <summary>
    /// The <see cref="DefaultFeedItemNormalizer"/> normalizes <see cref="FeedItem.Title"/>, 
    /// <see cref="FeedItem.Content"/> and <see cref="FeedItem.Summary"/> of <see cref="FeedItem"/>s to the point where
    /// they no longer contain any HTML, redundant whitespace, un-normalized unicode chars and other control chars like
    /// tabs, newlines or backspaces. The <see cref="FeedItem"/>'s <see cref="JSType.Date"/> property will contain
    /// whichever date is latest; the <see cref="FeedItem.PublishDate"/> or <see cref="FeedItem.LastUpdatedDate"/>.
    /// </summary>
    /// <remarks>
    /// You can implement a normalizer yourself by implementing the <see cref="IFeedItemNormalizer"/> interface.
    /// </remarks>
    public class GalnetFeedItemNormalizer ( bool fromAltUrl )
    {
        private bool fromAltUrl { get; set; } = fromAltUrl;

        /// <summary>
        /// Normalizes a SyndicationItem into a FeedItem.
        /// </summary>
        /// <param name="feed">The <see cref="SyndicationFeed"/> on which the item was retrieved.</param>
        /// <param name="item">A <see cref="SyndicationItem"/> to normalize into a <see cref="FeedItem"/>.</param>
        /// <returns>Returns a normalized <see cref="FeedItem"/>.</returns>
        public virtual FeedItem Normalize ( SyndicationFeed feed, SyndicationItem item )
        {
            var alternatelink = item.Links.FirstOrDefault(l =>
                l.RelationshipType == null ||
                l.RelationshipType.Equals("alternate", StringComparison.OrdinalIgnoreCase));

            Uri itemuri = null;
            if ( alternatelink == null && !Uri.TryCreate( item.Id, UriKind.Absolute, out var parsed ) )
            {
                itemuri = parsed;
            }
            else
            {
                itemuri = alternatelink?.GetAbsoluteUri();
            }
            var galnetId = fromAltUrl ? FetchId(item.Summary.Text) : item.Id;
            return new FeedItem
            {
                Id = string.IsNullOrEmpty( galnetId ) ? null : galnetId.Trim(),
                Title = item.Title == null ? null : Normalize( item.Title.Text, false ),
                Content = item.Content == null ? null : Normalize( ( (TextSyndicationContent)item.Content ).Text, false ),
                Summary = item.Summary == null ? null : Normalize( item.Summary.Text, true ),
                PublishDate = item.PublishDate,
                LastUpdatedDate = item.LastUpdatedTime == DateTimeOffset.MinValue ? item.PublishDate : item.LastUpdatedTime,
                Uri = itemuri
            };
        }

        private static string FetchId ( string value )
        {
            if ( !string.IsNullOrEmpty( value ) )
            {
                value = HtmlDecode( value );
                if ( string.IsNullOrEmpty( value ) )
                    return value;

                value = StripHTML( value );
                value = StripDoubleOrMoreWhiteSpace( RemoveControlChars( value ) );
                value = value.Normalize().Trim();
                var start = value.IndexOf("GUID", StringComparison.Ordinal) + 5;
                var end = value.IndexOf("en Image", StringComparison.Ordinal) - start;
                value = value.Substring( start, end );
            }
            return value;
        }

        private string Normalize ( string value, bool content )
        {
            if ( !string.IsNullOrEmpty( value ) )
            {
                value = HtmlDecode( value );
                if ( string.IsNullOrEmpty( value ) )
                    return value;

                value = StripHTML( value );
                value = StripDoubleOrMoreWhiteSpace( RemoveControlChars( value ) );
                value = value.Normalize().Trim();
                if ( content && fromAltUrl )
                {
                    var start = value.IndexOf("Body", StringComparison.Ordinal) + 5;
                    var end = value.LastIndexOf("Date", StringComparison.Ordinal) - start;
                    value = value.Substring( start, end - 1 );
                }
            }
            return value;
        }

        private static string RemoveControlChars ( string value )
        {
            return GeneratedRegex.HtmlControlCodesRegex().Replace( value, " " );
        }

        private static string StripDoubleOrMoreWhiteSpace ( string value )
        {
            return GeneratedRegex.WhiteMultiSpaceRegex().Replace( value, " " );
        }

        private static string StripHTML ( string value )
        {
            return GeneratedRegex.HtmlRegex().Replace( value, " " );
        }

        private static string HtmlDecode ( string value, int threshold = 5 )
        {
            var c = 0;
            var newvalue = WebUtility.HtmlDecode(value);
            while ( !newvalue.Equals( value ) && c < threshold )    //Keep decoding (if a string is double/triple/... encoded; we want the original)
            {
                c++;
                value = newvalue;
                newvalue = WebUtility.HtmlDecode( value );
            }
            if ( c >= threshold ) //Decoding threshold exceeded?
                return null;

            return newvalue;
        }
    }

    public class FeedItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public DateTimeOffset LastUpdatedDate { get; set; }
        public Uri Uri { get; set; }
    }
}
