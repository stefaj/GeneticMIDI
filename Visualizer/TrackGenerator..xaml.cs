using AForge.Genetic;
using GeneticMIDI.Fractal;
using GeneticMIDI.Generators;
using GeneticMIDI.Generators.CompositionGenerator;
using GeneticMIDI.Generators.Sequence;
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
using System.Windows.Shapes;

namespace Visualizer
{
    /// <summary>
    /// Interaction logic for TrackGenerator.xaml
    /// </summary>
    public partial class TrackGenerator : MetroWindow
    {
        public MelodySequence GeneratedSequence;
        public INoteGenerator Generator;
        public PatchNames Instrument;

        CompositionCategory category;
        public TrackGenerator(CompositionCategory category, Composition comp)
        {
            this.category = category;
            InitializeComponent();


            SetupLinePlot(fitnessPlot, "Average Fitness");

            // Load data for accompany instruments
            string path = "save/" + category.CategoryName;
            if(System.IO.Directory.Exists(path))
            {
                var files = System.IO.Directory.GetFiles(path);
                foreach(var file in files)
                {
                    try
                    {
                        string filename = System.IO.Path.GetFileNameWithoutExtension(file);
                        var sub = filename.Substring(6);
                    
                        PatchNames instrument = (PatchNames)(int.Parse(sub));

                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = instrument.ToString();
                        item.Tag = instrument;
                        accompInstruBox.Items.Add(item);
                    }
                    catch
                    {

                    }
                }
            }

            // Load data for accompany tracks
            if(comp != null)
                for(int i = 0; i < comp.Tracks.Count; i++)
                {
                    var track = comp.Tracks[i];

                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = string.Format("Track {0} - {1}", i.ToString(), track.Instrument.ToString());
                    item.Tag = track;

                    accompTrackBox.Items.Add(item);
                }

            randomScale.Items.Clear();
            foreach(var s in Scales.ScaleTypes)
            {
                //ListBoxItem item = new ListBoxItem();
                randomScale.Items.Add(s);
            }

            randomInstrument.Items.Clear();
            var durations = Enum.GetValues(typeof(PatchNames)).Cast<PatchNames>().ToArray();
            foreach (var d in durations)
                randomInstrument.Items.Add(d);
            
            
            
            
            
            StopSpinner();








        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Generate();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Generate()
        {
            int index = optionsTab.SelectedIndex;

            progressGenSlider.Value = 0;

            if (index == 0)
            {

                (fitnessPlot.Model.Series[0] as LineSeries).Points.Clear();
                fitnessPlot.Model.Series[0].Unselect();
                fitnessPlot.Model.Series.Clear();
                SetupLinePlot(fitnessPlot, "Average Fitness");
                fitnessPlot.ResetAllAxes();
                fitnessPlot.InvalidatePlot();

                //GA    
                IFitnessFunction fitness = null;

                MelodySequence seq = category.GetRandomComposition().GetLongestTrack().GetMainSequence() as MelodySequence;

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

                if (fitnessFuncCombo.SelectedIndex == 0)
                    fitness = new GeneticMIDI.FitnessFunctions.MetricSimilarity(seq, activeMetrics.ToArray(), GeneticMIDI.FitnessFunctions.SimilarityType.Cosine);
                if (fitnessFuncCombo.SelectedIndex == 1)
                    fitness = new GeneticMIDI.FitnessFunctions.MetricSimilarity(seq, activeMetrics.ToArray(), GeneticMIDI.FitnessFunctions.SimilarityType.Euclidian);
                if (fitnessFuncCombo.SelectedIndex == 2)
                    fitness = new GeneticMIDI.FitnessFunctions.MetricSimilarity(seq, activeMetrics.ToArray(), GeneticMIDI.FitnessFunctions.SimilarityType.Pearson);
                if (fitnessFuncCombo.SelectedIndex == 3)
                    fitness = new GeneticMIDI.FitnessFunctions.CrossCorrelation(seq);
                if (fitnessFuncCombo.SelectedIndex == 4)
                    fitness = GeneticMIDI.FitnessFunctions.NCD.FromMelodies(category);


                var gen = new GeneticGenerator(fitness, category);
                gen.OnPercentage += gen_OnPercentage;

                gen.MaxGenerations = (int)maxGenerationSlider.Value;
                Generator = gen;

                new Thread(() =>
                {
                    var notes = gen.Generate();

                    Instrument = PatchNames.Acoustic_Grand;

                    var mel = notes;
                    GeneratedSequence = mel;
                    progressGenSlider.Dispatcher.Invoke(() =>
                    {
                        progressGenSlider.Value = 100;
                    });
                }).Start();
            }
            if (index == 1)
            {
                //NCD.MaxTracks = 4;
                //fitness = NCD.FromCompositions(guide);

                InstrumentalGenerator gen = null;
                if (Generator as InstrumentalGenerator == null)
                {
                    gen = new InstrumentalGenerator(category);
                    gen.OnPercentage += gen_OnPercentage;
                    Generator = gen;
                }
                else
                {
                    gen = Generator as InstrumentalGenerator;
                }

                PatchNames instrument = (PatchNames)((instrBox.SelectedItem as ListBoxItem).Tag);
                Instrument = instrument;
                new Thread(() =>
                {
                   
                    if (Generator as InstrumentalGenerator != null)
                    {
                        var g = Generator as InstrumentalGenerator;
                        if (g.IsInitialized)
                        {
                            StartSpinner();
                            GeneratedSequence = gen.GenerateInstrument(instrument);
                            StopSpinner();
                        }
                    }
           
                    progressGenSlider.Dispatcher.Invoke(() =>
                    {
                        progressGenSlider.Value = 100;
                    });

                }).Start();
            }
            if(index == 2)
            {
                if (accompInstruBox.Items.Count == 0 || accompTrackBox.Items.Count == 0)
                    return;
                PatchNames instrument = (PatchNames)(accompInstruBox.SelectedItem);
                Track track = (accompTrackBox.SelectedItem as ListBoxItem).Tag as Track;

                var melSeq = track.GetMainSequence() as MelodySequence;
                Random rnd = new Random();
                if(accompMethoBox.SelectedIndex == 0)
                {
                    AccompanyGeneratorMarkov gen = new AccompanyGeneratorMarkov(category);
                    new Thread(() =>
                        {

                            StartSpinner();
                            GeneratedSequence = gen.Generate(melSeq,rnd.Next());
                            StopSpinner();
                            progressGenSlider.Dispatcher.Invoke(() =>
                            {
                                progressGenSlider.Value = 100;
                            });
                        }).Start();
                }
                else if (accompMethoBox.SelectedIndex == 1)
                {
                    AccompanimentGeneratorANNFF gen = new AccompanimentGeneratorANNFF(category, instrument);
                    
                    gen.SetSequence(melSeq);
                    new Thread(() =>
                        {

                            StartSpinner();
                            gen.Train();
                            GeneratedSequence = gen.Generate();
                            StopSpinner();
                            progressGenSlider.Dispatcher.Invoke(() =>
                                {
                                    progressGenSlider.Value = 100;
                                });

                        }).Start();
                }
                
                Instrument = instrument;
            }
            if(index == 4)
            {
                int centralNotePitch = (int)randomOctave.Value * 12;
                int noteShift = (int)randomPitchVar.Value;
                int minNote = centralNotePitch - noteShift;
                int maxNote = centralNotePitch + noteShift;
                if (minNote <= 0)
                    minNote = 1;
                if (maxNote >= 127)
                    maxNote = 126;
                int durMin = (int)Math.Pow(randomDurationRange.LowerValue, 2);
                int durMax = (int)Math.Pow(randomDurationRange.UpperValue, 2);
                int length = (int)randomLength.Value;

                ScaleType scale = randomScale.SelectedItem as ScaleType;

                var gen = new ReflectingBrownNoteGenerator(new NoteRangeRestrictor(minNote, maxNote, durMin, durMax, scale), new Random(), -2, 2, -1, 1);
                Generator = gen;
                gen.MaxNotes = length;
                Instrument = (PatchNames)randomInstrument.SelectedItem;
                if (Instrument == null)
                    return;

                new Thread(() =>
                {
                    StartSpinner();
                    GeneratedSequence = gen.Generate();
                    StopSpinner();
                    progressGenSlider.Dispatcher.Invoke(() =>
                    {
                        progressGenSlider.Value = 100;
                    });

                }).Start();

                Console.ReadLine();
            }
        }

        void StartSpinner()
        {
            progressRing.Dispatcher.Invoke(() =>
                {
                    progressRing.IsEnabled = true;
                    progressRing.Visibility = System.Windows.Visibility.Visible;
                    progressRing.IsActive = true;
                });
        }

        void StopSpinner()
        {
            progressRing.Dispatcher.Invoke(() =>
            {
                progressRing.IsActive = false;
                progressRing.IsEnabled = false;
                progressRing.Visibility = System.Windows.Visibility.Hidden;
            });
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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (Generator as InstrumentalGenerator != null)
            {
                new Thread(() =>
                {
                    StartSpinner();
                    var g = Generator as InstrumentalGenerator;
                    if (g.IsInitialized)
                    {
                    }
                    else
                    {

                        g.Initialize();
                    }

                    instrBox.Dispatcher.Invoke(() =>
                    {
                        instrBox.Items.Clear();
                        var instrs = g.GetInstruments(7);
                        foreach (var i in instrs)
                        {
                            ListBoxItem boxItem = new ListBoxItem();
                            boxItem.Content = i.ToString();
                            boxItem.Tag = i;
                            instrBox.Items.Add(boxItem);
                        }
                    });
                    StopSpinner(); 
                }).Start();

            }
            else
            {
                Generator = new InstrumentalGenerator(category);
                Button_Click_2(sender, e);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Composition comp;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".mid";
            dlg.Filter = "MIDI Files (*.mid)|*.mid";
            dlg.Title = "Load MIDI file";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                comp = Composition.LoadFromMIDI(dlg.FileName);

                TrackSelector trackSel = new TrackSelector(comp);
                if(trackSel.ShowDialog() == true)
                {
                    this.GeneratedSequence = trackSel.SelectedSequence;
                    this.Instrument = trackSel.SelectedInstrument;
                    this.DialogResult = true;
                    this.Close();
                }
            }

            
        }

        private void accompMethoBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            accompInstruBox.Items.Clear();
            accompInstruBox.Items.Add(PatchNames.Acoustic_Grand);
            accompInstruBox.Items.Add(PatchNames.Orchestral_Strings);
            accompInstruBox.Items.Add(PatchNames.Flute);
            accompInstruBox.Items.Add(PatchNames.Music_Box);
        }
    }


}
