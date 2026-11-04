using EddiDataDefinitions;
using EddiMissionMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Tests
{
    [TestClass, TestCategory( "WpfTests" ), DoNotParallelize]
    public class MissionMonitorCargoUiTests : TestBase
    {
        public TestContext TestContext { get; set; }

        [STATestMethod]
        public void CargoReadinessUpdatesActualGridBindingsUnderBothThemes ()
        {
            MakeSafe();
            foreach ( var theme in new[] { "Classic", "Modern" } )
            {
                var view = new ConfigurationWindow();
                view.Resources.MergedDictionaries.Add( new ResourceDictionary
                {
                    Source = new Uri( $"pack://application:,,,/EddiUI;component/Themes/Theme{( theme == "Classic" ? "Classic" : "Light" )}.xaml" )
                } );
                if ( theme == "Modern" )
                {
                    view.Resources.MergedDictionaries.Add( new ResourceDictionary
                    {
                        Source = new Uri( "pack://application:,,,/EddiUI;component/Themes/ThemeModern.xaml" )
                    } );
                }
                var now = new DateTime( 2026, 10, 4, 12, 0, 0, DateTimeKind.Utc );
                var salvage = new Mission( 1067632524, "MISSION_Salvage_CivilUnrest", now.AddDays( 4 ), MissionStatus.Active )
                {
                    localisedname = "Counter-insurgency Black Box Salvage Mission", originsystem = "Laksak",
                    originstation = "Sekelj Port", destinationsystem = "Laksak", destinationstation = "Sekelj Port",
                    amount = 2
                };
                salvage.SetRedirectClaimability( true );
                salvage.ObserveOnboardCargo( now, 2 );
                var delivery = new Mission( 2, "Mission_Delivery", now.AddDays( 1 ), MissionStatus.Active )
                {
                    localisedname = "Partial delivery", destinationsystem = "Laksak", destinationstation = "Bugrov Dock",
                    amount = 5, delivered = 3
                };
                var goal = new Mission( 3, "MISSION_CommunityGoal", now.AddHours( -1 ), MissionStatus.Complete )
                {
                    localisedname = "Completed community goal", communal = true
                };
                goal.ObserveCommunityGoalProgress( 1234, 25 );
                var tour = new Mission( 4, "Mission_Sightseeing", now.AddDays( 1 ), MissionStatus.Active )
                {
                    localisedname = "Two-stop sightseeing", destinationsystem = "First",
                    destinationsystems = [ new NavWaypoint( "First", 1, 0, 0, 0 ), new NavWaypoint( "Second", 2, 1, 0, 0 ) ]
                };
                var combat = new Mission( 5, "Mission_MassacreWing", now.AddDays( 1 ), MissionStatus.Active )
                {
                    localisedname = "Pirate massacre estimate", faction = "Issuer", targetfaction = "Pirates",
                    targetTypeEDName = "Pirate", destinationsystem = "Hunting ground"
                };
                combat.ObserveCombatAcceptance( now, 1, 30, "Hunting ground" );
                var grid = (DataGrid)view.FindName( "missionsData" );
                grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                grid.ItemsSource = new[] { salvage, delivery, goal, tour, combat };
                Layout( view, 1100 );
                var statusColumn = grid.Columns.Single( column => column.SortMemberPath == "statusEDName" );
                var progressColumn = grid.Columns.Single( column => column.SortMemberPath == "progress" );
                var remainingColumn = grid.Columns.OfType<DataGridTextColumn>().Single( column =>
                    column.Binding is System.Windows.Data.Binding binding && binding.Path.Path == "timeRemaining" );
                Assert.AreEqual( string.Empty, ((TextBlock)remainingColumn.GetCellContent( salvage )).Text );
                Assert.IsFalse( string.IsNullOrEmpty( ((TextBlock)remainingColumn.GetCellContent( delivery )).Text ),
                    "A partially claimable delivery retains its countdown." );
                Assert.AreEqual( now.AddDays( 4 ), salvage.expiry, "Hiding the countdown must preserve the original deadline." );
                Assert.AreEqual( "Estimated 0/30 kills", ((TextBlock)progressColumn.GetCellContent( combat )).Text );
                combat.ObserveMatchingCombatBounty( now.AddSeconds( 1 ) );
                Layout( view, 1100 );
                Assert.AreEqual( "Estimated 1/30 kills", ((TextBlock)progressColumn.GetCellContent( combat )).Text );
                Assert.Contains( "mission-target", ( (TextBlock)progressColumn.GetCellContent( combat )).ToolTip.ToString());
                Assert.IsFalse( combat.progressKnown );
                Assert.IsFalse( combat.claimable );
                combat.MarkCombatHistoryIncomplete();
                Layout( view, 1100 );
                Assert.AreEqual( "Estimated 1/30 kills", ((TextBlock)progressColumn.GetCellContent( combat )).Text );
                var badge = Descendants<TextBlock>( statusColumn.GetCellContent( salvage ) ).Single( block => block.Text.Contains( '✓' ) );
                Assert.AreEqual( Visibility.Visible, badge.Visibility );
                Assert.IsFalse( string.IsNullOrWhiteSpace( AutomationProperties.GetName( badge ) ) );
                Assert.AreEqual( badge.ToolTip, AutomationProperties.GetName( badge ) );
                Assert.AreEqual( "100%", ((TextBlock)progressColumn.GetCellContent( salvage )).Text );
                Assert.AreEqual( delivery.localizedProgress, ((TextBlock)progressColumn.GetCellContent( delivery )).Text );
                Assert.AreEqual( "0%", ((TextBlock)progressColumn.GetCellContent( tour )).Text );
                Assert.IsTrue( tour.ObserveSightseeingStop( now, "First", 1 ) );
                Layout( view, 1100 );
                Assert.AreEqual( "50%", ((TextBlock)progressColumn.GetCellContent( tour )).Text );
                Assert.IsFalse( tour.claimable );
                Assert.Contains( block => block.Text == goal.localizedStatus, Descendants<TextBlock>( statusColumn.GetCellContent( goal ) ));
                var converter = new MissionProgressConverter();
                Assert.AreEqual( converter.Convert( [ goal.localizedProgress, true, true, 1234L, 25 ],
                    typeof( string ), null, System.Globalization.CultureInfo.CurrentCulture ),
                    ((TextBlock)progressColumn.GetCellContent( goal )).Text );
                goal.ObserveCommunityGoalProgress( 2345, 10 );
                Layout( view, 1100 );
                Assert.AreEqual( converter.Convert( [ goal.localizedProgress, true, true, 2345L, 10 ],
                    typeof( string ), null, System.Globalization.CultureInfo.CurrentCulture ),
                    ((TextBlock)progressColumn.GetCellContent( goal )).Text );
                Render( view, $"mission-phase4d-{theme.ToLowerInvariant()}.png" );

                salvage.ObserveOnboardCargo( now.AddSeconds( 1 ), 1 );
                Layout( view, 1100 );
                Assert.AreEqual( Visibility.Collapsed, badge.Visibility );
                Assert.AreEqual( salvage.localizedProgress, ((TextBlock)progressColumn.GetCellContent( salvage )).Text );
                Assert.AreEqual( MissionStatus.Active, salvage.statusDef );
                Render( view, $"mission-phase4d-{theme.ToLowerInvariant()}-cargo-loss.png" );

                salvage.ObserveOnboardCargo( now.AddSeconds( 2 ), 2 );
                Layout( view, 1100 );
                Assert.AreEqual( Visibility.Visible, badge.Visibility );
                Assert.AreEqual( "100%", ((TextBlock)progressColumn.GetCellContent( salvage )).Text );
                Layout( view, 360 );
                var scrollViewer = Descendants<ScrollViewer>( grid ).First();
                Assert.AreEqual( ScrollBarVisibility.Auto, scrollViewer.HorizontalScrollBarVisibility );
                scrollViewer.ScrollToRightEnd();
                Layout( view, 360 );
                var statusContent = Descendants<StackPanel>( statusColumn.GetCellContent( goal ) ).Single();
                var requiredStatusWidth = statusContent.Children.OfType<TextBlock>().Sum( block => block.DesiredSize.Width ) +
                                          statusContent.Margin.Left + statusContent.Margin.Right + 2;
                Assert.IsGreaterThanOrEqualTo( requiredStatusWidth,
                    statusColumn.ActualWidth, "Narrow layouts must not clip lifecycle status or its claimable badge." );
                Assert.IsGreaterThanOrEqualTo( progressColumn.GetCellContent( delivery ).DesiredSize.Width,
                    progressColumn.ActualWidth, "Narrow layouts must not clip partial progress." );
                var goalProgress = (TextBlock)progressColumn.GetCellContent( goal );
                Assert.AreEqual( TextWrapping.Wrap, goalProgress.TextWrapping );
                Assert.AreEqual( goalProgress.Text, goalProgress.ToolTip );
                Render( view, $"mission-phase4d-{theme.ToLowerInvariant()}-narrow.png" );
            }
        }

        private static void Layout ( FrameworkElement view, int width )
        {
            view.Width = width;
            view.Height = 420;
            view.Measure( new Size( width, 420 ) );
            view.Arrange( new Rect( 0, 0, width, 420 ) );
            view.UpdateLayout();
            // DataGrid defers column measurements beyond the binding pass, including in offscreen layouts.
            view.Dispatcher.Invoke( () => { }, DispatcherPriority.ContextIdle );
            view.UpdateLayout();
        }

        private void Render ( FrameworkElement view, string filename )
        {
            var bitmap = new RenderTargetBitmap( (int)view.ActualWidth, (int)view.ActualHeight, 96, 96, PixelFormats.Pbgra32 );
            bitmap.Render( view );
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add( BitmapFrame.Create( bitmap ) );
            var directory = Path.Combine( AppContext.BaseDirectory, "MissionMonitorLayout" );
            Directory.CreateDirectory( directory );
            var path = Path.Combine( directory, filename );
            using ( var stream = File.Create( path ) ) { encoder.Save( stream ); }
            TestContext.AddResultFile( path );
        }

        private static IEnumerable<T> Descendants<T> ( DependencyObject root ) where T : DependencyObject
        {
            if ( root is null ) { yield break; }
            if ( root is T match ) { yield return match; }
            for ( var i = 0; i < VisualTreeHelper.GetChildrenCount( root ); i++ )
            {
                foreach ( var descendant in Descendants<T>( VisualTreeHelper.GetChild( root, i ) ) ) { yield return descendant; }
            }
        }
    }
}
