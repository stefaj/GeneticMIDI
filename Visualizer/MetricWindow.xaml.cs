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
    /// Interaction logic for MetricWindow.xaml
    /// </summary>
    public partial class MetricWindow : MetroWindow
    {
        public MetricWindow(MelodySequence seq)
        {
            InitializeComponent();

            GenerateMetrics(seq);
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

        void PlotMetric(OxyPlot.Wpf.PlotView plot, GeneticMIDI.Metrics.IMetric metric, string title, MelodySequence seq)
        {
            var dic = metric.Generate(seq.ToArray());

            SetupColumnPlot(plot, title);

            foreach (var k in dic.Keys)
            {
                (plot.Model.Series[0] as ColumnSeries).Items.Add(new ColumnItem(dic[k]));
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
    }
}
