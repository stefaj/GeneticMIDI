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

        Composition generated;

        double prevTime = 0;
        double currentTime = 0;

        DispatcherTimer songUpdateTimer;

        IPlaybackGenerator lastGen = null;

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

            player = new MusicPlayer();
            player.OnMessageSent += player_OnMessageSent;

            //Load files
            SetupCategories();

            songUpdateTimer = new DispatcherTimer();
            songUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            songUpdateTimer.Tick += songUpdateTimer_Tick;
            songUpdateTimer.Start();

        }

        private void SetupPlayPlot(Composition comp)
        {
            playPanel.Children.Clear();
            foreach(var t in comp.Tracks)
            {
                MelodySequence seq = t.GetMainSequence() as MelodySequence;
                DotNetMusic.WPF.MusicSheet sheet = new DotNetMusic.WPF.MusicSheet();
                sheet.Height = 150;
                sheet.SetNotes(seq.ToArray());
                sheet.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;


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



           // x_axes.Minimum = prevTime - 16;
           // x_axes.Maximum = prevTime + 4;

            double interval = currentTime - prevTime;
            interval /= 10;

           // double panStep = x_axes.Transform(-(interval) + x_axes.Offset);
            //x_axes.Pan(panStep);

            //this.midiPlot.InvalidatePlot();

            prevTime += interval;


        }

        int last_index = 0;
        void player_OnMessageSent(object sender, int key, PlaybackMessage msg)
        {

            currentTime = key / 1000.0f;

            double duration = Math.Log(msg.Duration + 1, 2) * 2;

            var tag = msg.Tag as Tuple<byte, Note>;
            if(tag != null)
            {
                GeneticMIDI.Representation.Track track = null;
                int j = 0;
                foreach (var t in generated.Tracks)
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
                for(int i = last_index; i < notes.Length; i++)
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
                }
            }
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

            if (generated == null)
                return;
            currentTime = 0;
            last_index = 0;

            prevTime = 0;
            player.Play(generated);
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

                last_index = 0;
                generated = Composition.LoadFromMIDI(dlg.FileName);
               // generateSongPlotFull(midiPlotFull.Model, generated.GeneratePlaybackInfo());
                itemsBox.Items.Clear();

                foreach(var track in generated.Tracks)
                {
                    
                    AddTrackPlot(track.GetMainSequence() as MelodySequence, track.Instrument);
                }

                SetupPlayPlot(generated);

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
            sheet.Height = 150;
            sheet.SetNotes(seq.ToArray());
            sheet.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Content = sheet;
            listBoxItem.Tag = seq;
            listBoxItem.MouseDoubleClick += listBoxItem_MouseDoubleClick;
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

        private void itemsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }
    }
}
