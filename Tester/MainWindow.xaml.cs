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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudioWrapper;

namespace Tester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SongManager manager = new SongManager();

        string song1 = @"D:\Libraries\Musik\God Help The Girl\God Help The Girl\02-God help the girl.mp3";
        string song2 = @"D:\Libraries\Musik\God Help The Girl\God Help The Girl\13-I'll have to dance with Cassie.mp3";

        string sound1 = @"D:\Libraries\Dokument\Visual Studio 2013\Projects\LekmattanGUI\LekmattanGUI\music\swedish\DANCE_2\1_TRUMMOR\Trummor2.wav";
        string sound2 = @"D:\Libraries\Dokument\Visual Studio 2013\Projects\LekmattanGUI\LekmattanGUI\music\swedish\DANCE_2\1_TRUMMOR\Trummor3.wav";
        string sound3 = @"D:\Libraries\Dokument\Visual Studio 2013\Projects\LekmattanGUI\LekmattanGUI\music\swedish\DANCE_2\1_TRUMMOR\Trummor0.wav";
        string sound4 = @"D:\Libraries\Dokument\Visual Studio 2013\Projects\LekmattanGUI\LekmattanGUI\music\swedish\DANCE_2\1_TRUMMOR\Trummor1.wav";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            manager.PlayAll();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            manager.PauseAll();
        }

        private void Resume_Click(object sender, RoutedEventArgs e)
        {
            manager.ResumeAll();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            manager.StopAll();
        }

        private void Dispose_Click(object sender, RoutedEventArgs e)
        {
            manager.Dispose();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            manager.AddSong(new LoopedSong(sound2));
            manager.AddSong(new ChainedSong(new Song[2] { new Song(sound3), new Song(song1) }));
        }

    }
}
