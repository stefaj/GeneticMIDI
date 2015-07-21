using AForge.Genetic;
using GeneticMIDI.Generators;
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        MelodySequence comp;

        Composition guide;

        double prevTime = 0;
        double currentTime = 0;

        DispatcherTimer songUpdateTimer;

        INoteGenerator lastGen = null;

        public MainWindow()
        {
            InitializeComponent();


            setupMidiPlot();
            setupMidiPlotFull();

            player = new MusicPlayer();
            player.OnMessageSent += player_OnMessageSent;

            guide = new Composition();
            guide.LoadFromMIDI("ff7tifa.mid");

            //Load files
            foreach(string file in System.IO.Directory.GetFiles("test/"))
            {
                if (System.IO.Path.GetExtension(file) != ".mid")
                    continue;
                var c = new ComboBoxItem();
                c.Content = System.IO.Path.GetFileNameWithoutExtension(file);
                c.Tag = file;
                guideCombo.Items.Add(c);
            }


            songUpdateTimer = new DispatcherTimer();
            songUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            songUpdateTimer.Tick += songUpdateTimer_Tick;
            songUpdateTimer.Start();

            SetupLinePlot(fitnessPlot, "Average Fitness");
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

        void setupMidiPlotFull()
        {
            var fullPlotModel = new PlotModel { };

            // this.songModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "Ellie(x)"));
            this.midiPlotFull.Model = fullPlotModel;

            var songPointsFull = new List<ScatterPoint>();

            var songSeries = new ScatterSeries();
            songSeries.ItemsSource = songPointsFull;
            songSeries.MarkerType = MarkerType.Diamond;
            songSeries.MarkerSize = 0.1;

            fullPlotModel.Series.Add(songSeries);

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
            midiPlotFull.ActualController.UnbindAll();
            fullPlotModel.IsLegendVisible = false;


            fullPlotModel.Axes.Add(x_axis);
            fullPlotModel.Axes.Add(y_axis);

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
                    if(message.Message == PlaybackMessage.PlaybackMessageType.Start)
                        songpoints.Add(new ScatterPoint(keys[i], message.Pitch % 12, message.Duration / 2, message.Pitch / 12));
                }
            }



            if (model.Axes.Count > 2)
            {
                model.Axes[2].IsAxisVisible = false;
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
            SongPoints.Add(new ScatterPoint(currentTime, msg.Pitch % 12, msg.Duration / 2, msg.Pitch / 12));
            //SongConPoints.Add(new DataPoint(currentTime, msg.Pitch % 12));

            if (songModel.Axes.Count > 2)
            {
                songModel.Axes[2].Maximum = 14;
                songModel.Axes[2].Minimum = -2;
                songModel.Axes[2].IsAxisVisible = false;
            }

            progressSongSlider.Dispatcher.Invoke(() =>
            {
                if (!progressSongSlider.IsMouseCaptured && !progressSongSlider.IsMouseDirectlyOver )
                {
                    progressSongSlider.Maximum = player.MaxKey;
                    progressSongSlider.Value = key;
                } 
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

            //new Thread(() => { player.Play(comp); }).Start();

            //var info = guide.GeneratePlaybackInfo();


            /*
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
                    if(message.Message == PlaybackMessage.PlaybackMessageType.Start)
                    {
                        if (message.Velocity <= 0)
                            continue;
                        SongPoints.Add(new ScatterPoint(keys[i]/100, message.Pitch%12, message.Duration, message.Pitch / 12));
                        //SongSeries.Items.Add(new ColumnItem(message.Pitch % 12));
                    }

                }
                int sleep_dur = keys[i + 1] - keys[i];
            }*/

            //var seq = guide.GetLongestTrack().GetMainSequence() as MelodySequence;
            //GenerateMetrics(seq);

        }

        void GenerateMetrics(MelodySequence seq)
        {
            PlotMetric(metricPlot1, new ChromaticTone(), "Chromatic Tone", seq);
            PlotMetric(metricPlot2, new ChromaticToneDistance(), "Chromatic Tone Distance", seq);
            PlotMetric(metricPlot3, new ChromaticToneDuration(), "Chromatic Tone Duration", seq);
            PlotMetric(metricPlot4, new MelodicBigram(), "Melodic Bigram", seq);
            PlotMetric(metricPlot5, new MelodicInterval(), "Melodic Interval", seq);
            PlotMetric(metricPlot6, new Pitch(), "Pitch", seq);
            PlotMetric(metricPlot7, new Rhythm(), "Rhythm", seq);
            PlotMetric(metricPlot8, new RhythmicBigram(), "Rhythmic Bigram", seq);
            PlotMetric(metricPlot9, new RhythmicInterval(), "Rhythmic Interval", seq);

            metricPlot1.InvalidatePlot(true);
            metricPlot2.InvalidatePlot(true);
            metricPlot3.InvalidatePlot(true);
            metricPlot4.InvalidatePlot(true);
            metricPlot5.InvalidatePlot(true);
            metricPlot6.InvalidatePlot(true);
            metricPlot7.InvalidatePlot(true);
            metricPlot8.InvalidatePlot(true);
            metricPlot9.InvalidatePlot(true);

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
             var seq = guide.GetLongestTrack().GetMainSequence() as MelodySequence;
             GenerateMetrics(seq);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(procedureCombo.SelectedIndex < 0)
                return;
            if(guideCombo.SelectedIndex < 0)
                return;

            progressGenSlider.Value = 0;
            
            guide = new Composition();
            guide.LoadFromMIDI((guideCombo.SelectedItem as ComboBoxItem).Tag.ToString());

            if(procedureCombo.SelectedIndex == 0)
            {
                //HMM
                MelodySequence seq = guide.GetLongestTrack().GetMainSequence() as MelodySequence;
                MarkovChainGenerator gen = new MarkovChainGenerator();
                gen.AddMelody(seq);
                //gen.AddMelody("test/bach/bwv651.mid");

                gen.OnPercentage += gen_OnPercentage;

                lastGen = gen;
                //gen.MaxGenerations = (int)maxGenerationSlider.Value*10;

                new Thread(() =>
                {
                    var notes = gen.Generate();
                    comp = new MelodySequence(notes);
                    progressGenSlider.Dispatcher.Invoke(() =>
                        {
                            progressGenSlider.Value = 100;
                        });
                }).Start();
                
            }
            if(procedureCombo.SelectedIndex == 1)
            {
                
                (fitnessPlot.Model.Series[0] as LineSeries).Points.Clear();
                fitnessPlot.Model.Series[0].Unselect();
                fitnessPlot.Model.Series.Clear();
                SetupLinePlot(fitnessPlot, "Average Fitness");
                fitnessPlot.ResetAllAxes();
                fitnessPlot.InvalidatePlot();                

                //GA    
                IFitnessFunction fitness = null;

                MelodySequence seq = guide.GetLongestTrack().GetMainSequence() as MelodySequence;
       
                //Options
                List<IMetric> activeMetrics = new List<IMetric>();
                if (metricChromaticTone.IsChecked == true)
                    activeMetrics.Add(new ChromaticTone());
                if (metricChromaticToneDistance.IsChecked == true)
                    activeMetrics.Add(new ChromaticToneDistance());
                if (metricChromaticToneDuration.IsChecked == true)
                    activeMetrics.Add(new ChromaticToneDuration());
                if (metricMelodicBigram.IsChecked == true)
                    activeMetrics.Add(new MelodicBigram());
                if (metricMelodicInterval.IsChecked == true)
                    activeMetrics.Add(new MelodicInterval());
                if (metricPitch.IsChecked == true)
                    activeMetrics.Add(new Pitch());
                if (metricPitchDistance.IsChecked == true)
                    activeMetrics.Add(new PitchDistance());
                if (metricRhythm.IsChecked == true)
                    activeMetrics.Add(new Rhythm());
                if (metricRhythmicBigram.IsChecked == true)
                    activeMetrics.Add(new RhythmicBigram());
                if (metricRhythmicInterval.IsChecked == true)
                    activeMetrics.Add(new RhythmicInterval());


                                    //<ComboBoxItem>Cosine Similarity</ComboBoxItem>
                                    //<ComboBoxItem>Euclidian Similarity</ComboBoxItem>
                                    //<ComboBoxItem>Pearson Correlation</ComboBoxItem>
                                    //<ComboBoxItem>Cross Correlation</ComboBoxItem>
                                    //<ComboBoxItem>Normalized Compression Distance</ComboBoxItem>

                if(fitnessFuncCombo.SelectedIndex == 0)
                    fitness = new GeneticMIDI.FitnessFunctions.MetricSimilarity(seq, activeMetrics.ToArray(), GeneticMIDI.FitnessFunctions.SimilarityType.Cosine);
                if (fitnessFuncCombo.SelectedIndex == 1)
                    fitness = new GeneticMIDI.FitnessFunctions.MetricSimilarity(seq, activeMetrics.ToArray(), GeneticMIDI.FitnessFunctions.SimilarityType.Euclidian);
                if (fitnessFuncCombo.SelectedIndex == 2)
                    fitness = new GeneticMIDI.FitnessFunctions.MetricSimilarity(seq, activeMetrics.ToArray(), GeneticMIDI.FitnessFunctions.SimilarityType.Pearson);
                if (fitnessFuncCombo.SelectedIndex == 3)
                    fitness = new GeneticMIDI.FitnessFunctions.CrossCorrelation(seq);
                if(fitnessFuncCombo.SelectedIndex == 4)
                    fitness = new GeneticMIDI.FitnessFunctions.NCD(new MelodySequence[]{seq});


                var gen = new GeneticGenerator(fitness, seq);
                gen.OnPercentage += gen_OnPercentage;

                gen.MaxGenerations = (int)maxGenerationSlider.Value;
                lastGen = gen;

                new Thread(() =>
                    {
                        var notes = gen.Generate();
                        comp = new MelodySequence(notes);
                        progressGenSlider.Dispatcher.Invoke(() =>
                        {
                            progressGenSlider.Value = 100;
                        });
                    }).Start();
            }
        }

        void gen_OnPercentage(object sender, int percentage, double fitness)
        {
            progressGenSlider.Dispatcher.Invoke(() =>
                {
                    progressGenSlider.Value += 1;
                });

            fitnessPlot.Dispatcher.Invoke(() =>
                {
                    ((fitnessPlot.Model.Series[0] as LineSeries).ItemsSource as List<DataPoint>).Add(new DataPoint(percentage, fitness));
                    fitnessPlot.InvalidatePlot();
                });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            player.Stop();
            // Play button

           // if (comp == null)
           //     return;
            if (playCompRad.IsChecked == true)
            {
                if (comp == null)
                    return;
                currentTime = 0;
                SongPoints.Clear();
                prevTime = 0;
                player.Play(comp);
            }
            if(playGuideRad.IsChecked == true)
            {
                if (guide == null)
                    return;
                currentTime = 0;
                SongPoints.Clear();
                prevTime = 0;
                player.Play(guide);
            }
            songUpdateTimer.Stop();
            songUpdateTimer.Start();
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            if (comp == null)
                return;
            GenerateMetrics(comp);
        }

        private void guideCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            guide = new Composition();
            guide.LoadFromMIDI((guideCombo.SelectedItem as ComboBoxItem).Tag.ToString());
        }

        private void procedureCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabGA == null)
                return;
            if (optionsTab == null)
                return;

            if (procedureCombo.SelectedIndex != 1)
            {
                tabGA.IsEnabled = false;
                tabGA.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                tabGA.IsEnabled = true;
                tabGA.Visibility = System.Windows.Visibility.Visible;
            }
            optionsTab.SelectedIndex = procedureCombo.SelectedIndex;

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // Stop
            player.Stop();
        }

        private void playGuideRad_Checked(object sender, RoutedEventArgs e)
        {
            if(guide == null)
                return;
            generateSongPlotFull(midiPlotFull.Model, guide.GeneratePlaybackInfo());
        }

        private void playCompRad_Checked(object sender, RoutedEventArgs e)
        {
            if (comp == null)
                return;
            generateSongPlotFull(midiPlotFull.Model, comp.GeneratePlaybackInfo(1));
        }

        private void randomizeBtn_Click(object sender, RoutedEventArgs e)
        {
            // Randomize

            if (lastGen == null)
                return;
            comp = new MelodySequence(lastGen.Next());

            generateSongPlotFull(midiPlotFull.Model, comp.GeneratePlaybackInfo(1));

            playCompRad.IsChecked = true;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            player.Stop();
            Thread.Sleep(100);
        }
    }
}
