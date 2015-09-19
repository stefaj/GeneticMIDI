using GeneticMIDI.Representation;
using MahApps.Metro.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Visualizer
{
    /// <summary>
    /// Interaction logic for TrackSelector.xaml
    /// </summary>
    public partial class TrackSelector : MetroWindow
    {
        public MelodySequence SelectedSequence;
        public PatchNames SelectedInstrument;

        Composition composition;
        public TrackSelector(Composition comp)
        {
            this.composition = comp;
            InitializeComponent();

            foreach(var track in comp.Tracks)
            {
                AddTrackPlot(track);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void AddTrackPlot(Track track)
        {
            MelodySequence seq = track.GetMainSequence() as MelodySequence;

            ListBoxItem listBoxItem = new ListBoxItem();
            OxyPlot.Wpf.PlotView plotView = new OxyPlot.Wpf.PlotView();
            plotView.Margin = new Thickness(0, 0, 82, 10);
            setupTrackPlot(plotView);
            generateSongPlotFull(plotView.Model, seq.GeneratePlaybackInfo(0));

            Grid grid = new Grid();
            grid.Height = 128;
            grid.Children.Add(plotView); 
            listBoxItem.Content = grid;
            listBoxItem.Tag = track;
            listBoxItem.MouseDoubleClick += listBoxItem_MouseDoubleClick;

            itemsBox.Items.Add(listBoxItem);

        }

        void listBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Track track = (sender as ListBoxItem).Tag as Track;
            this.SelectedSequence = track.GetMainSequence() as MelodySequence;
            this.SelectedInstrument = track.Instrument;
            this.DialogResult = true;
            this.Close();
        }

        void generateSongPlotFull(PlotModel model, PlaybackInfo info)
        {
            var songpoints = (model.Series[0] as ScatterSeries).ItemsSource as List<ScatterPoint>;
            songpoints.Clear();
            model.InvalidatePlot(true);

            var keys = new int[info.Messages.Keys.Count];
            int i = 0;
            foreach (var k in info.Messages.Keys)
            {
                keys[i++] = k;
            }

            for (i = 0; i < keys.Length - 1; i++)
            {

                foreach (PlaybackMessage message in info.Messages[keys[i]])
                {
                    if (message.Message == PlaybackMessage.PlaybackMessageType.Start)
                    {
                        double duration = Math.Log(message.Duration + 1, 2) * 2;
                        songpoints.Add(new ScatterPoint(keys[i], message.Pitch % 12, duration, message.Pitch / 12));
                    }
                }
            }



            if (model.Axes.Count > 2)
            {
                model.Axes[2].IsAxisVisible = false;
                model.Axes[2].Maximum = 14;
                model.Axes[2].Minimum = -2;
            }


            model.InvalidatePlot(true);
        }

        void setupTrackPlot(OxyPlot.Wpf.PlotView plot)
        {
            var plotModel = new PlotModel { };

            plot.Model = plotModel;

            var songPointsFull = new List<ScatterPoint>();

            var songSeries = new ScatterSeries();
            songSeries.ItemsSource = songPointsFull;
            songSeries.MarkerType = MarkerType.Square;
            songSeries.MarkerSize = 0.1;

            plotModel.Series.Add(songSeries);

            var x_axis = new OxyPlot.Axes.LinearAxis()
            {
                Key = "XAxis",
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                IsAxisVisible = false,
                //AbsoluteMaximum = 1800,
                //AbsoluteMinimum = -1,
                //MinorStep = 10,
            };
            var y_axis = new LinearColorAxis()
            {
                Key = "YAxis",
                Position = OxyPlot.Axes.AxisPosition.Left,
                Palette = OxyPalettes.Hue(12),
                Minimum = 0,
                Maximum = 12,
                IsAxisVisible = false


            };
            plot.ActualController.UnbindAll();
            plotModel.IsLegendVisible = false;


            plotModel.Axes.Add(x_axis);
            plotModel.Axes.Add(y_axis);
        }
    }
}
