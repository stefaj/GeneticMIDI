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

        MusicPlayer player;

        private Composition Generated
        {
            get
            {
                return generator.ActiveComposition;
            }
            set
            {
                generator.ActiveComposition = value;
            }
        }

        CompositionRandomizer generator;


        double prevTime = 0;
        double currentTime = 0;

        DispatcherTimer songUpdateTimer;

        

        Databank databank;

        CompositionCategory selectedCategory;
        

        public MainWindow()
        {
            InitializeComponent();

            player = new MusicPlayer();
            player.OnMessageSent += player_OnMessageSent;

            //Load files
            SetupCategories();

            songUpdateTimer = new DispatcherTimer();
            songUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            songUpdateTimer.Tick += songUpdateTimer_Tick;
            songUpdateTimer.Start();

            generator = new CompositionRandomizer();
            generator.OnCompositionChange += generator_OnCompositionChange;

        }

        // Update stuff when composition changes
        void generator_OnCompositionChange(object sender, EventArgs e)
        {
            SetupPlayPlot(Generated);
            SetupGenerationPlots(Generated);
        }

        private void SetupPlayPlot(Composition comp)
        {
            if (comp == null)
                return;
            playPanel.Children.Clear();
            foreach(var t in comp.Tracks)
            {
                MelodySequence seq = t.GetMainSequence() as MelodySequence;
                DotNetMusic.WPF.MusicSheet sheet = new DotNetMusic.WPF.MusicSheet();
                sheet.Height = 200;
                sheet.SetNotes(seq.ToArray());
                sheet.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; sheet.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                

                playPanel.Children.Add(sheet);            


            }
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

            playScroll.ScrollToHorizontalOffset(playScroll.HorizontalOffset + increment);
            increment *= 0.95;

           // x_axes.Minimum = prevTime - 16;
           // x_axes.Maximum = prevTime + 4;

            double interval = currentTime - prevTime;
            interval /= 10;

           // double panStep = x_axes.Transform(-(interval) + x_axes.Offset);
            //x_axes.Pan(panStep);

            //this.midiPlot.InvalidatePlot();

            prevTime += interval;


        }

        Dictionary<int, int> lastIndex = new Dictionary<int, int>();
        double increment = 0;
        void player_OnMessageSent(object sender, int key, PlaybackMessage msg)
        {

            currentTime = key / 1000.0f;

            var tag = msg.Tag as Tuple<byte, Note>;
            if(tag != null)
            {
                GeneticMIDI.Representation.Track track = null;
                int j = 0;
                foreach (var t in Generated.Tracks)
                {
                    if (t.Channel == tag.Item1)
                    {
                        track = t;
                        break;
                    }
                    j++;
                }
                if (track == null)
                    return;
                var seq = track.GetMainSequence() as MelodySequence;
                var notes = seq.ToArray();
                /*for(int i = last_index; i < notes.Length; i++)
                {
                    if(notes[i] == tag.Item2)
                    {
                        last_index = i;
                        playPanel.Dispatcher.Invoke(() =>
                            {
                                var sheet = (playPanel.Children[j] as DotNetMusic.WPF.MusicSheet);
                                sheet.Dispatcher.Invoke(() =>
                                {
                                    sheet.SetHighlightIndex(i);
                                });
                            });
                        break;
                    }
                }*/
                if (!lastIndex.ContainsKey(j))
                    lastIndex[j] = 0;
                double time = 0;
                for(int i = 0; i < notes.Length; i++)
                {
                    Note n = notes[i];
                    
                    if(Math.Abs(time - currentTime) < 1 && n == tag.Item2 && i>=lastIndex[j])
                    {
                        playPanel.Dispatcher.Invoke(() =>
                        {
                            var sheet = (playPanel.Children[j] as DotNetMusic.WPF.MusicSheet);
                            sheet.Dispatcher.Invoke(() =>
                            {
                                sheet.SetHighlightIndex(i);
                            });
                        });
                        lastIndex[j] = i;
                        break;
                    }
                    else if(n==tag.Item2)
                    {
                        int k = 10;
                    }
                    time += n.RealDuration;
                }

            }

            int maxHighlightedWidth = 0;
            DotNetMusic.WPF.MusicSheet maxHighlight;
            playPanel.Dispatcher.Invoke(() =>
                {
                    foreach (DotNetMusic.WPF.MusicSheet sheet in playPanel.Children)
                    {
                        if (sheet.GetHighlightedWidth() > maxHighlightedWidth)
                        {
                            maxHighlightedWidth = sheet.GetHighlightedWidth();
                            maxHighlight = sheet;
                        }
                    }
                    double dreamOffset = maxHighlightedWidth - this.Width / 2.0;
                    double currentOffset = playScroll.HorizontalOffset;
                    increment = (dreamOffset - currentOffset) / 20.0;
                    
                });
                    
                    // Add new msg
            
            progressSongSlider.Dispatcher.Invoke(() =>
            {

                if (progressSongSlider.IsMouseCaptured || progressSongSlider.IsMouseOver)
                    return;
                
                progressSongSlider.Maximum = player.MaxKey;
                progressSongSlider.Value = key; 
            }
            );
            
        }


        /// <summary>
        /// Play button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            player.Stop();
            // Play button

            // if (comp == null)
            //     return;

            if (Generated == null)
                return;
            currentTime = 0;
            lastIndex = new Dictionary<int, int>();

            prevTime = 0;
            player.Play(Generated);
           // generateSongPlotFull(midiPlotFull.Model, generated.GeneratePlaybackInfo());

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

        private void randomizeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (generator == null)
                return;

            // Randomize
            generator.Next();
          
            //generateSongPlotFull(midiPlotFull.Model, generated.GeneratePlaybackInfo());

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

        /// <summary>
        /// Load button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
  
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".mid";
            dlg.Filter = "MIDI Files (*.mid)|*.mid";
            dlg.Title = "Load MIDI file";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {

                lastIndex = new Dictionary<int, int>();
                Generated = Composition.LoadFromMIDI(dlg.FileName);
               // generateSongPlotFull(midiPlotFull.Model, generated.GeneratePlaybackInfo());
                itemsBox.Items.Clear();

                foreach(var track in Generated.Tracks)
                {
                    
                    AddTrackPlot(track.GetMainSequence() as MelodySequence, track.Instrument);
                }

            }
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Generated == null)
                return;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.DefaultExt = ".mid";
            dlg.Filter = "MIDI Files (*.mid)|*.mid";
            dlg.Title = "Load MIDI file";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                Generated.WriteToMidi(dlg.FileName);
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
          /*  ListBoxItem listBoxItem = new ListBoxItem();
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

            itemsBox.Items.Add(listBoxItem);*/


            DotNetMusic.WPF.MusicSheet sheet = new DotNetMusic.WPF.MusicSheet();
            sheet.Height = 200;
            sheet.SetNotes(seq.ToArray());
            sheet.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = sheet;
            listBoxItem.Tag = seq;
            listBoxItem.MouseDoubleClick += listBoxItem_MouseDoubleClick;
            itemsBox.Items.Add(listBoxItem);

        }

        private void SetupGenerationPlots(Composition comp)
        {
            itemsBox.Items.Clear();
            foreach (var t in comp.Tracks)
                AddTrackPlot(t.GetMainSequence() as MelodySequence, t.Instrument);
        }

        // New metric window
        void listBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MelodySequence seq = (sender as ListBoxItem).Tag as MelodySequence;
            if (seq != null)
            {
                MetricWindow metric = new MetricWindow(seq);
                int index = itemsBox.SelectedIndex;
                if(Generated != null && Generated.Tracks.Count > index && index >= 0)
                {
                    string instrument = Generated.Tracks[index].Instrument.ToString();
                    instrument = instrument.Replace("_", " ");
                    if(Generated.Tracks[index].Instrument == PatchNames.Helicopter)
                        instrument  = "Drums";

                    string str = string.Format("Track {0} - {1}", index, instrument);
                    metric.Title = str;
                    metric.Show();
                }
                
            }
        }

        // Remove a track
        void btn_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = (sender as Button).Tag as ListBoxItem;

            try
            {
                int index = itemsBox.Items.IndexOf(item);
                generator.Remove(index);
            }
            catch
            {

            }
            itemsBox.Items.Remove(item);

            // TODO Remove track plot
        }

        // New track
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TrackGenerator g = new TrackGenerator(selectedCategory, Generated);

            if (g.ShowDialog() == true)
            {

                if (g.GeneratedSequence != null)
                {
                    generator.Add(g.GeneratedSequence, g.Generator);
                }
            }

        }

        // Remove all
        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            itemsBox.Items.Clear();
            generator.Clear();
        }

        private void itemsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        
        //Delete a specific track
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            ListBoxItem item = itemsBox.SelectedItem as ListBoxItem;
            int index = -1;
            try
            {
                index = itemsBox.Items.IndexOf(item);
                generator.Remove(index);
            }
            catch
            {

            }

        }

        // Up octave
        private void MenuItem_ClickUp(object sender, RoutedEventArgs e)
        {

            ListBoxItem item = itemsBox.SelectedItem as ListBoxItem;
            try
            {
                MelodySequence seq = item.Tag as MelodySequence;
                seq.OctaveUp();
            }
            catch
            {

            }

        }

        // Down octave
        private void MenuItem_ClickDown(object sender, RoutedEventArgs e)
        {

            ListBoxItem item = itemsBox.SelectedItem as ListBoxItem;
            try
            {
                MelodySequence seq = item.Tag as MelodySequence;
                seq.OctaveDown();
            }
            catch
            {

            }

        }
    }
}
