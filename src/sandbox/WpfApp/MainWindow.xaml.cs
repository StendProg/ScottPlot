using ScottPlot.Plottable;
using System.Windows;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WpfPlot1.plt.Title("Plot 1");
            WpfPlot1.plt.PlotScatter(ScottPlot.DataGen.Consecutive(50_000), ScottPlot.DataGen.Sin(50_000));
            WpfPlot1.OverlayPlottables.Add(new HSpan() { X1 = 10, X2 = 25, DragEnabled = true });

            WpfPlot2.plt.Title("Plot 2");
            WpfPlot2.plt.PlotScatter(ScottPlot.DataGen.Consecutive(50_000), ScottPlot.DataGen.Sin(50_000));
            WpfPlot2.plt.PlotHSpan(10, 25, draggable: true);
            //WpfPlot2.plt.PlotSignal(ScottPlot.DataGen.Cos(5_000_000));
        }
    }
}
