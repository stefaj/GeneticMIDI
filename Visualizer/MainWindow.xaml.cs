using AForge.Genetic;
using GeneticMIDI;
using GeneticMIDI.FitnessFunctions;
using GeneticMIDI.Generators;
using GeneticMIDI.Generators.CompositionGenerator;
using GeneticMIDI.Generators.NoteGenerators;
using GeneticMIDI.Metrics;
using GeneticMIDI.Metrics.Frequency;
using GeneticMIDI.Representation;
using MahApps.Metro.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Visualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public PlotModel songModel { get; private set; }
        IList<ScatterPoint> SongPoints;
        IList<DataPoint> SongConPoints;
        ScatterSeries SongSeries;
        LineSeries SongConSeries;
        LinearAxis x_axes;
        LinearColorAxis y_axes;

        MusicPlayer player;

        Composition generated;

        double prevTime = 0;
        double currentTime = 0;

        DispatcherTimer songUpdateTimer;

        IPlaybackGenerator lastGen = null;

        NCD fitness = null;

        Databank databank;

        CompositionCategory selectedCategory;

        private Composition ActiveComposition
        {
            get
            {
                return generated;
            }
        }

        public MainWindow()
        {
            InitializeComponent();


            setupMidiPlot();
            setupMidiPlotFull();

            player = new MusicPlayer();
            player.OnMessageSent += player_OnMessageSent;

            //Load files
            SetupCategories();

            songUpdateTimer = new DispatcherTimer();
            songUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            songUpdateTimer.Tick += songUpdateTimer_Tick;
            songUpdateTimer.Start();

        }

        void SetupCategories()
        {
            databank = new Databank(Visualizer.Properties.Settings.Default.LibraryPath);
            foreach(var k in Databank.defaultPaths.Keys)
            {
                ComboBoxItem cb = new ComboBoxItem();
                cb.Content = k;
                cb.Tag = k;
                guideCombo.Items.Add(cb);
            }

        }

        void setupMidiPlot()
        {
            this.songModel = new PlotModel { Title = "Song" };

            // this.songModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "Ellie(x)"));
            this.midiPlot.Model = songModel;

            SongPoints = new List<ScatterPoint>
            {
            };
            SongConPoints = new List<DataPoint>();

            SongSeries = new ScatterSeries();
            SongSeries.ItemsSource = SongPoints;
            SongSeries.MarkerType = MarkerType.Circle;
            SongSeries.MarkerSize = 0.1;

            SongConSeries = new LineSeries();
            SongConSeries.LineStyle = LineStyle.Dash;
            SongConSeries.Color = OxyColors.SteelBlue;
            SongConSeries.StrokeThickness = 0.3;
            SongConSeries.ItemsSource = SongConPoints;
            SongConSeries.Smooth = true;
            SongConSeries.CanTrackerInterpolatePoints = true;

            this.midiPlot.Model.Series.Add(SongConSeries);
            this.midiPlot.Model.Series.Add(SongSeries);

            x_axes = new OxyPlot.Axes.LinearAxis()
            {
                Key = "XAxis",
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                //AbsoluteMaximum = 1800,
                //AbsoluteMinimum = -1,
                //MinorStep = 10,
            };
            y_axes = new LinearColorAxis()
            {
                Key = "YAxis",
                Position = OxyPlot.Axes.AxisPosition.Left,
                Palette = OxyPalettes.Hue(12),
                Minimum = 0,
                Maximum = 12,
                IsAxisVisible = false


            };
            midiPlot.ActualController.UnbindAll();

            songModel.Axes.Add(x_axes);
            songModel.Axes.Add(y_axes);
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

        void setupMidiPlotFull()
        {
            setupTrackPlot(this.midiPlotFull);

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

        void songUpdateTimer_Tick(object sender, EventArgs e)
        {


            //songModel.Axes[0].IsPanEnabled = false;



            x_axes.Minimum = prevTime - 16;
            x_axes.Maximum = prevTime + 4;

            double interval = currentTime - prevTime;
            interval /= 10;

            double panStep = x_axes.Transform(-(interval) + x_axes.Offset);
            //x_axes.Pan(panStep);

            this.midiPlot.InvalidatePlot();

            prevTime += interval;


        }

        void player_OnMessageSent(object sender, int key, PlaybackMessage msg)
        {

            currentTime = key / 1000.0f;

            double duration = Math.Log(msg.Duration + 1, 2) * 2;
             
            SongPoints.Add(new ScatterPoint(currentTime, msg.Pitch % 12, duration, msg.Pitch / 12));
            //SongConPoints.Add(new DataPoint(currentTime, msg.Pitch % 12));

            if (songModel.Axes.Count > 2)
            {
                songModel.Axes[2].Maximum = 14;
                songModel.Axes[2].Minimum = -2;
                songModel.Axes[2].IsAxisVisible = false;
            }

            progressSongSlider.Dispatcher.Invoke(() =>
            {

                if (progressSongSlider.IsMouseCaptured || progressSongSlider.IsMouseOver)
                    return;
                
                progressSongSlider.Maximum = player.MaxKey;
                progressSongSlider.Value = key; 
            }
            );
            
        }

        void SetupColumnPlot(OxyPlot.Wpf.PlotView plot, string title)
        {
            plot.Model = new PlotModel { Title = title };

            var series = new ColumnSeries();
            series.FillColor = OxyColors.SteelBlue;

            plot.Model.Series.Add(series);

            var xaxis = new OxyPlot.Axes.CategoryAxis()
            {
                Key = "XAxis",
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                IsAxisVisible = false
            };
            var yaxis = new LinearAxis()
            {
                Key = "YAxis",
                Position = OxyPlot.Axes.AxisPosition.Left,
            };

            plot.Model.Axes.Add(xaxis);
            plot.Model.Axes.Add(yaxis);
            plot.ActualController.UnbindAll();
        }

        void SetupLinePlot(OxyPlot.Wpf.PlotView plot, string title)
        {
            plot.Model = new PlotModel { Title = title };

            var series = new LineSeries();
            series.Color = OxyColors.SteelBlue;

            series.ItemsSource = new List<DataPoint>(); 

            plot.Model.Series.Add(series);

            var xaxis = new OxyPlot.Axes.LinearAxis()
            {
                Key = "XAxis",
                Position = OxyPlot.Axes.AxisPosition.Bottom,
            };
            var yaxis = new LinearAxis()
            {
                Key = "YAxis",
                Position = OxyPlot.Axes.AxisPosition.Left,
            };

            plot.Model.Axes.Add(xaxis);
            plot.Model.Axes.Add(yaxis);
            plot.ActualController.UnbindAll();
        }

        void PlotMetric(OxyPlot.Wpf.PlotView plot, GeneticMIDI.Metrics.IMetric metric, string title, MelodySequence seq)
        {
            var dic = metric.Generate(seq.ToArray());

            SetupColumnPlot(plot, title);

            foreach(var k in dic.Keys)
            {
                (plot.Model.Series[0] as ColumnSeries).Items.Add(new ColumnItem(dic[k]));
            }
        }

        void Play()
        {
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // var seq = guide.GetLongestTrack().GetMainSequence() as MelodySequence;
             //GenerateMetrics(seq);
        }





        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            player.Stop();
            // Play button

            // if (comp == null)
            //     return;

            if (generated == null)
                return;
            currentTime = 0;
            SongPoints.Clear();
            prevTime = 0;
            player.Play(generated);
            generateSongPlotFull(midiPlotFull.Model, generated.GeneratePlaybackInfo());

            songUpdateTimer.Stop();
            songUpdateTimer.Start();


        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void guideCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCategory = databank.Load((guideCombo.SelectedItem as ComboBoxItem).Tag.ToString());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Stop
            player.Stop();
        }

        private void playCompRad_Checked(object sender, RoutedEventArgs e)
        {
            if (generated == null)
                return;
            generateSongPlotFull(midiPlotFull.Model, generated.GeneratePlaybackInfo());
        }

        private void randomizeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (lastGen == null)
                return;

            // Randomize
            generated = new Composition();

            Composition best = generated;


                var mel = lastGen.Next() as MelodySequence;
                if (mel != null)
                {
                    GeneticMIDI.Representation.Track track = new GeneticMIDI.Representation.Track(PatchNames.Acoustic_Grand, 1);
                    track.AddSequence(mel);
                    generated.Add(track);
                }
                else
                {
                    generated = lastGen.Next() as Composition;
                }

           

            generateSongPlotFull(midiPlotFull.Model, generated.GeneratePlaybackInfo());

        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            player.Stop();
            Thread.Sleep(100);
        }

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            player.Seek((int)progressSongSlider.Value);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
  
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".mid";
            dlg.Filter = "MIDI Files (*.mid)|*.mid";
            dlg.Title = "Load MIDI file";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                generated = Composition.LoadFromMIDI(dlg.FileName);
                generateSongPlotFull(midiPlotFull.Model, generated.GeneratePlaybackInfo());
                itemsBox.Items.Clear();

                foreach(var track in generated.Tracks)
                {
                    
                    AddTrackPlot(track.GetMainSequence() as MelodySequence, track.Instrument);
                }

            }


        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveComposition == null)
                return;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.DefaultExt = ".mid";
            dlg.Filter = "MIDI Files (*.mid)|*.mid";
            dlg.Title = "Load MIDI file";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                ActiveComposition.WriteToMidi(dlg.FileName);
            }

        }

        private void availableInstrBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
   
            
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            btn_Click(sender, e);
        }

        private void AddTrackPlot(MelodySequence seq, PatchNames instr)
        {



            /*<Grid Height="107">
                                <oxy:Plot Margin="0,0,82,10"></oxy:Plot>
                                <Button HorizontalAlignment="Right" Width="48" Height="48" Margin="0,32,0,0" VerticalAlignment="Top" Click="Button_Click_4">
                                    <Image Width="16" Height="16" Source="Resources/glyphicons-208-remove-2.png"></Image>
                                </Button>
                            </Grid>*/
            ListBoxItem listBoxItem = new ListBoxItem();
            OxyPlot.Wpf.PlotView plotView = new OxyPlot.Wpf.PlotView();
            plotView.Margin = new Thickness(0, 0, 82, 10);
            Button btn = new Button(); btn.Width = 48; btn.Height = 48; btn.Margin = new Thickness(0, 32, 0, 0); btn.HorizontalAlignment = System.Windows.HorizontalAlignment.Right; btn.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            btn.Click += btn_Click;
            setupTrackPlot(plotView);
            generateSongPlotFull(plotView.Model, seq.GeneratePlaybackInfo(0));

            Grid grid = new Grid();
            grid.Height = 128;
            grid.Children.Add(plotView); grid.Children.Add(btn);
            listBoxItem.Content = grid;
            listBoxItem.Tag = seq;
            listBoxItem.MouseDoubleClick += listBoxItem_MouseDoubleClick;
            btn.Tag = listBoxItem;

            itemsBox.Items.Add(listBoxItem);

        }

        void listBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MelodySequence seq = (sender as ListBoxItem).Tag as MelodySequence;
            if (seq != null)
            {
                MetricWindow metric = new MetricWindow(seq);
                metric.Title = "Metrics " + seq.ToString();
                metric.Show();
            }
        }

        void btn_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = (sender as Button).Tag as ListBoxItem;

            try
            {
                int index = itemsBox.Items.IndexOf(item);
                generated.Tracks.RemoveAt(index);
            }
            catch
            {

            }
            itemsBox.Items.Remove(item);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TrackGenerator g = new TrackGenerator(selectedCategory, generated);
            g.ShowDialog();

            if (g.GeneratedSequence != null)
            {
                AddTrackPlot(g.GeneratedSequence, g.Instrument);
                if (generated == null)
                    generated = new Composition();


                byte channel = 1;
                if (generated != null)
                    channel = (byte)(generated.Tracks.Count + 1);
                var track = new GeneticMIDI.Representation.Track(g.Instrument, channel);
                track.AddSequence(g.GeneratedSequence);

                generated.Tracks.Add(track);
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            itemsBox.Items.Clear();
            generated = new Composition();
        }
    }
}
