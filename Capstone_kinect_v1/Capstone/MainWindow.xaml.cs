using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;



namespace Capstone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Member Variables
        private KinectSensor _KinectDevice;
        private readonly Brush[] _BodyBrushes;
        private Body[] _FrameBody;
        #endregion Member Variables


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            _BodyBrushes = new Brush[] { Brushes.Black, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, Brushes.Purple, Brushes.Pink };

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }
        #endregion Constructor


        #region Methods
        private void KinectSensors_StatusChanged(object sender, IsAvailableChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Initializing:
                case KinectStatus.Connected:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.KinectDevice = e.Sensor;
                    break;
                case KinectStatus.Disconnected:
                    //TODO: Give the user feedback to plug-in a Kinect device.                    
                    this.KinectDevice = null;
                    break;
                default:
                    //TODO: Show an error state
                    break;
            }
        }


        // Listing 4-2
        private void KinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (BodyFrameSource frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Polyline figure;
                    Brush userBrush;
                    Body body;

                    LayoutRoot.Children.Clear();
                    frame.CopyFrameDataToArray(this._FrameBody);

                    Body[] dataSet2 = new Body[this._FrameBody.Length];
                    frame.CopyFrameDataToArray(dataSet2);


                    for (int i = 0; i < this._FrameBody.Length; i++)
                    {
                        body = this._FrameBody[i];

                        if (body.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            userBrush = this._SkeletonBrushes[i % this._SkeletonBrushes.Length];

                            //Draw head and torso
                            figure = CreateFigure(body, userBrush, new[] { JointType.Head, JointType.Neck, JointType.ShoulderLeft, JointType.SpineShoulder,
                                                                                JointType.ShoulderRight, JointType.SpineMid, JointType.SpineBase                                                                              });
                            LayoutRoot.Children.Add(figure);


                            figure = CreateFigure(body, userBrush, new[] { JointType.HipLeft, JointType.HipRight });
                            LayoutRoot.Children.Add(figure);
                            //Debug.WriteLine("------------------------------" + skeleton.Joints[joints[0]]);

                            //Draw left leg
                            figure = CreateFigure(body, userBrush, new[] { JointType.SpineBase, JointType.HipLeft, JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                            LayoutRoot.Children.Add(figure);

                            //Draw right leg
                            figure = CreateFigure(body, userBrush, new[] { JointType.SpineBase, JointType.HipRight, JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                            LayoutRoot.Children.Add(figure);

                            //Draw left arm
                            figure = CreateFigure(body, userBrush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft, JointType.HandLeft });
                            LayoutRoot.Children.Add(figure);

                            //Draw right arm
                            figure = CreateFigure(body, userBrush, new[] { JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight });
                            LayoutRoot.Children.Add(figure);

                            saveCoordinates(body);
                        }
                    }
                }
            }
        }


        // Listing 4-3
        private Polyline CreateFigure(Body body, Brush brush, JointType[] joints)
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness = 8;
            figure.Stroke = brush;

            for (int i = 0; i < joints.Length; i++)
            {
                figure.Points.Add(GetJointPoint(body.Joints[JointType.joints[i]]));
            }

            return figure;
        }


        private Point GetJointPoint(Joint joint)
        {
            DepthImagePoint point = this.KinectDevice.MapSkeletonPointToDepth(joint.Position, this.KinectDevice.DepthStream.Format);
            point.X *= (int)this.LayoutRoot.ActualWidth / KinectDevice.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / KinectDevice.DepthStream.FrameHeight;
            return new Point(point.X, point.Y);
        }



        #endregion Methods


        #region Properties
        public KinectSensor KinectDevice
        {
            get { return this._KinectDevice; }
            set
            {
                if (this._KinectDevice != value)
                {
                    //Uninitialize
                    if (this._KinectDevice != null)
                    {
                        this._KinectDevice.Close();
                        this._KinectDevice.SkeletonFrameReady -= KinectDevice_SkeletonFrameReady;
                        this._KinectDevice.SkeletonStream.Disable();
                        this._FrameSkeletons = null;
                    }

                    this._KinectDevice = value;

                    //Initialize
                    if (this._KinectDevice != null)
                    {
                        if (this._KinectDevice.Status == KinectStatus.Connected)
                        {
                            this._KinectDevice.SkeletonStream.Enable();
                            this._FrameSkeletons = new Skeleton[this._KinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                            this.KinectDevice.SkeletonFrameReady += KinectDevice_SkeletonFrameReady;
                            this._KinectDevice.Open();
                        }
                    }
                }
            }
        }
        #endregion Properties

        private void saveCoordinates(Body body)
        {
            string filePath = @"d:\vids\";
            string _fileName = string.Format("{0}{1}{2}", filePath, DateTime.Now.ToString("MMddyyyyHmmss"), ".txt");

            StreamWriter coordinatesStream = new StreamWriter(_fileName);

            foreach (Joint joint in body.Joints)
            {
                coordinatesStream.WriteLine(joint.JointType + ", " + joint.TrackingState + ", " + joint.Position.X + ", " + joint.Position.Y + ", " + joint.Position.Z);

            }
            coordinatesStream.Close();

        }

        //public void saveVideo()
        //{
        //    string filePath = timeStamp + ".txt";

        //    StreamWriter cooStream = new StreamWriter(filePath, false);

        //    IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

        //    Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

        //    foreach (JointType jointType in joints.Keys)
        //    {
        //        ColorSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToColorSpace(joints[jointType].Position);

        //        cooStream.WriteLine(joints[jointType].JointType + " " + joints[jointType].TrackingState + " " + joints[jointType].Position.X + " " + joints[jointType].Position.Y + " " + joints[jointType].Position.Z + " " + depthSpacePoint.X + " " + depthSpacePoint.Y);
        //    }

        //    string wrtLineData = "LeftHand " + body.HandLeftState + " RightHand " + body.HandRightState;

        //    cooStream.WriteLine(wrtLineData);
        //    cooStream.Close();
        //}



    }
}
