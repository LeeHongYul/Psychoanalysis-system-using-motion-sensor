using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

namespace KinectStreams
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        Mode _mode = Mode.Color;

        IList<Body> _bodies;

        bool _displayBody = false;

        private KinectSensor _sensor = null;
        private CoordinateMapper coordinateMapper = null;
        private MultiSourceFrameReader _reader = null;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            //clock_txtbox.Text = DateTime.Now.ToString();

            DispatcherTimer timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromMilliseconds(0.01);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            this._sensor = KinectSensor.GetDefault();
            this.coordinateMapper = this._sensor.CoordinateMapper;
            FrameDescription colorFrameDescription = this._sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            FrameDescription frameDescription = this._sensor.DepthFrameSource.FrameDescription;

            this._reader = this._sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.Body);
            this._reader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;

        }

        #endregion

        #region Event handlers
        private void timer_Tick(object sender, EventArgs e)
        {
            clock_txtbox.Text = DateTime.Now.ToString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
         

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Infrared)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                float nbody = 0;
                                // Draw skeleton.
                                if (_displayBody)
                                {
                                    canvas.DrawSkeleton(body);

                                    //saveCoordinates
                                    string filePath = @"C:\Users\82102\Desktop\111\KinectStreams\KinectStreams\KinectStreams\KinectStreams\bin\Debug";
                                    string _filename = string.Format("{0}{1}", filePath, nbody, ".txt");

                                    StreamWriter coordinatesStream = new StreamWriter(_filename);

                                    foreach (Joint joint in body.Joints.Values)
                                    {
                                        coordinatesStream.WriteLine(joint.JointType);
                                        coordinatesStream.WriteLine(" x = " + joint.Position.X);
                                        coordinatesStream.WriteLine(" y = " + joint.Position.Y);
                                        
                                    }
                                    coordinatesStream.Close();
                                }
                                nbody++;
                            }
                        }
                    }
                }
            }
        }
        /*private void saveCoordinates(Body body)
        {
            float nbody = 0;
            string filePath = @"d:C:\Users\82102\Desktop\111\KinectStreams\KinectStreams\KinectStreams\KinectStreams\bin\Debug";
            string _filename = string.Format("{0}{1}", filePath, nbody, ".txt");

            StreamWriter coordinatesStream = new StreamWriter(_filename);
            foreach (Joint joint in body.Joints.Values)
            {
                coordinatesStream.WriteLine(joint.JointType);
                coordinatesStream.WriteLine(" x = " + joint.Position.X);
                coordinatesStream.WriteLine(" y = " + joint.Position.Y);

            }
            coordinatesStream.Close();
            nbody++;
        }
        */

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Depth;
        }

        private void Infrared_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Infrared;
        }

        private void Body_Click(object sender, RoutedEventArgs e)
        {
            _displayBody = !_displayBody;

            string text = "   ==================== 스켈레톤 좌표 측정중입니다. ====================";
            output_txtbox.Text = text;

            /*
            string filePath = @"C:\Users\82102\Desktop\ppppp";
            string _filename = string.Format("{0}{1}", filePath, nbody, ".txt");
            
           Console.WriteLine(textValue);
            

            */
        }

        private void Valuee_Click(object sender, RoutedEventArgs e)
        {
      
            for (int i = 0; i <= 1; i++)
            {

                string AnkleLeftt = File.ReadAllText("AnkleLeft" + i + ".txt");
                string ShoulderLeftt = File.ReadAllText("ShoulderLeft" + i + ".txt");

                output_txtbox.Text = output_txtbox.Text + "\n" + AnkleLeftt + "\n" + ShoulderLeftt +"\n";
               

            }
            
        }
        /*
         * 
         * 
         * string etest = File.ReadAllText("AnkleRight" + i + ".txt");
        string etest = File.ReadAllText("ShoulderLeft" + i + ".txt");
        string etest = File.ReadAllText("ShoulderRight" + i + ".txt");
        string etest = File.ReadAllText("hipLeft" + i + ".txt");
        string etest = File.ReadAllText("hipRight" + i + ".txt");
        string etest = File.ReadAllText("FootLeft" + i + ".txt");
        string etest = File.ReadAllText("FootRight" + i + ".txt");
        string etest = File.ReadAllText("Neck" + i + ".txt");
        string etest = File.ReadAllText("handLeft" + i + ".txt");
        string etest = File.ReadAllText("handRight" + i + ".txt");
        string etest = File.ReadAllText("elbowLeftRight" + i + ".txt");
        string etest = File.ReadAllText("elbowLeft" + i + ".txt");
        */



        private void Valuee1_Click(object sender, RoutedEventArgs e)
        {

            double[,] arrA = new double[1, 2];
            double[,] arrS = new double[1, 2];
            for (int i = 0; i <= 1; i++)
            {
                using (TextReader reader = File.OpenText("AnkleLeft" + i +".txt"))
                {
                    string ALz = reader.ReadLine();
                    double ALx = double.Parse(reader.ReadLine());
                    double ALy = double.Parse(reader.ReadLine());



                    arrA[0, 0] = ALx;
                    arrA[0, 1] = ALy;
                }

                using (TextReader reader = File.OpenText("ShoulderLeft" + i + ".txt"))
                {
                    string Sz = reader.ReadLine();
                    double Sx = double.Parse(reader.ReadLine());
                    double Sy = double.Parse(reader.ReadLine());


                    arrS[0, 0] = Sx;
                    arrS[0, 1] = Sy;


                }
                if ((arrA[0, 0] - arrS[0, 0]) - (arrA[0, 1] - arrS[0, 1]) > 5)
                {
                    output_txtbox2.Text = Convert.ToString(output_txtbox2.Text + "\n" + arrA[0, 0] + "\n" + arrA[0, 1] + "\n" + arrS[0, 0] + "\n" + arrS[0, 1]);
                }
                else output_txtbox2.Text = "Smaller thatn 5";
                }







        }

            /*output_txtbox2.Text = x + "\n" + y;
                if(arr[0, 0]== arr[0, 1])
              {
                  output_txtbox2.Text = "x,y좌표 같다";
              }else output_txtbox2.Text = Convert.ToString(arr[0, 0] + "\n" + arr[0, 1]+"\n"+"x, y 좌표 같지않다");*/
            /*string path = @"C:\Users\82102\Desktop\111\KinectStreams\KinectStreams\KinectStreams\KinectStreams\bin\Debug\Ankel0.txt";
            string[] lines = File.ReadAllLines(path);
            public string ExtractOpenFileName(string path)
            {
                List<string> open_fileList = new List<string>();
                using (StreamReader sr = new StreamReader(path))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        open_fileList.Add(line.Split('"')[3]);//"로 문자열 나누기
                        if (open_fileList.Count == 1)
                        {
                            break;
                        }
                    }
                }
                string open_file = open_fileList[0];

                return open_file;
            }
            



*/
        

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _displayBody = !_displayBody;

            string text = "   ==================== 스켈레톤 좌표 측정중입니다. ====================";
            string text2 = "   ==================== 좌표 저장이 완료되었습니다. ====================\n" +
                           "   ====================       좌표 저장 경로 : (Debug File)   ====================";

            output_txtbox.Text = text + "\n" + text2;



        }



        /* private void Exit_Click(object sender, RoutedEventArgs e)
         {
             Environment.Exit(0);
         }
        */
        #endregion


    }

    public enum Mode
    {
        Color,
        Depth,
        Infrared
    }
}
/*class Program { 
          static void Main(string[] args) 
          { 
              string str; Console.Write("AnkleLeft0_1102202121212.txt"); 
              string filename = Console.ReadLine(); 
              StringReader sr = new StringReader(filename); 
              str = sr.ReadLine(); string[] data = str.Split(new char[] { ':' }); 
              int count = int.Parse(data[1]); Console.WriteLine("---------------------------"); 

              for (int i = 0; i < count; i++) 
              { str = sr.ReadLine(); 
                  string[] data2 = str.Split(new char[] { ':' });
                  Console.WriteLine("{0} {1} {2} {3} {4}", data2[0], data2[1], data2[2], data2[3], data2[4]);
              } 
              Console.WriteLine("---------------------"); sr.Close(); }
      }

  StreamReader rd = new StreamReader("test.txt");
            for(int i= 0; i < 100; i++)
            {
                string stest= rd.ReadLine();
                
            }
            rd.Close();
string str;
            Console.Write("Ankle1.txt");
            string filename = Console.ReadLine();
            StringReader sr = new StringReader(filename); 
            str = sr.ReadLine(); 
            string[] data = str.Split(new char[] { ':' }); 
            int count = int.Parse(data[1]); 
            Console.WriteLine("---------------------------"); 
           
            for (int i = 0; i < count; i++) 
            { 
                str = sr.ReadLine(); 
                string[] data2 = str.Split(new char[] { ':' }); 
                Console.WriteLine("{0} {1} {2} {3} {4}", data2[0], data2[1], data2[2], data2[3], data2[4]); 
            }
            Console.WriteLine("---------------------"); 
            sr.Close();
 
 
 */