using AForge.Genetic;
using GeneticMIDI.Generators;
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
        int currentTime = 0;

        DispatcherTimer songUpdateTimer;

        public MainWindow()
        {
            InitializeComponent();


            this.songModel = new PlotModel { Title = "Song" };

            // this.songModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "Ellie(x)"));
            this.midiPlot.Model = songModel;

            SongPoints = new List<ScatterPoint>{                               
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

            //SongSeries.FillColor = OxyColors.SteelBlue;
            
            //SongSeries.Color = OxyColors.SteelBlue;
            //SongSeries.MarkerFill = OxyColors.SteelBlue;
            //SongSeries.MarkerType = MarkerType.Circle;

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


            };
            midiPlot.ActualController.UnbindAll();

            songModel.Axes.Add(x_axes);
            songModel.Axes.Add(y_axes);

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

        void songUpdateTimer_Tick(object sender, EventArgs e)
        {


            //songModel.Axes[0].IsPanEnabled = false;



            x_axes.Minimum = prevTime - 800;
            x_axes.Maximum = prevTime + 200;

            double interval = currentTime - prevTime;
            interval /= 10;

            double panStep = x_axes.Transform(-(interval) + x_axes.Offset);
            //x_axes.Pan(panStep);

            this.midiPlot.InvalidatePlot();

            prevTime += interval;


        }

        void player_OnMessageSent(object sender, int key, PlaybackMessage msg)
        {

            currentTime = key / 10;
            SongPoints.Add(new ScatterPoint(currentTime, msg.Pitch % 12, msg.Duration / 2, msg.Pitch / 12));
            //SongConPoints.Add(new DataPoint(currentTime, msg.Pitch % 12));

            if (songModel.Axes.Count > 2)
            {
                songModel.Axes[2].Maximum = 14;
                songModel.Axes[2].Minimum = -2;
            }

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
            PlotMetric(metricPlot7, new PitchDistance(), "Pitch Distance", seq);
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
            
            
            guide = new Composition();
            guide.LoadFromMIDI((guideCombo.SelectedItem as ComboBoxItem).Tag.ToString());

            if(procedureCombo.SelectedIndex == 0)
            {
                //HMM
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


                
                if(fitnessFuncCombo.SelectedIndex == 0)
                    fitness = new GeneticMIDI.FitnessFunctions.CosineSimiliarity(seq, activeMetrics.ToArray());
                if(fitnessFuncCombo.SelectedIndex == 2)
                    fitness = new GeneticMIDI.FitnessFunctions.NCD(new MelodySequence[]{seq});
                if(fitnessFuncCombo.SelectedIndex == 1)
                    fitness = new GeneticMIDI.FitnessFunctions.CrossCorrelation(seq);

                GeneticGenerator gen = new GeneticGenerator(fitness, seq);
                gen.OnPercentage += gen_OnPercentage;

                gen.MaxGenerations = (int)maxGenerationSlider.Value;

                progressGenSlider.Value = 0;

                new Thread(() =>
                    {
                        var notes = gen.Generate();
                        comp = new MelodySequence(notes);
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
    }
}
