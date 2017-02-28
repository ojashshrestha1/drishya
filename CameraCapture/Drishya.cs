/*=================================================
 * This code is brought to you by Mahvish
 * visit http://fewtutorials.bravesites.com/ for more
 * tutorials on EmguCV and C#
 * **************************************************
 *        PLEASE DO NOT REMOVE THIS NOTE!
 * **************************************************
 * ================================================== */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using System.IO;

namespace LiveFaceDetection
{
    public partial class CameraCapture : Form
    {
        //******************************************************************************
        //                          GLOBAL VARIABLES
        //******************************************************************************
        private Capture capture;        //takes images from camera as image frames

        Image<Bgr, Byte> ImageFrame;    //The Global "Input Image"

        //============For Face detector(Haar classifier)===========================
        private HaarCascade haar;            //the viola-jones classifier (detector)

        //instead of using the Default values of the parameters
        //in call to DetectHaarCascade(),Lets make the values "variable"
        private int WindowsSize = 25;
        private Double ScaleIncreaseRate = 1.1;
        private int MinNeighbors = 3;

        Bitmap[] ExtFaces;
        int faceNo = 0;
        //******************************************************************************
        //******************************************************************************


        public CameraCapture()
        {
            InitializeComponent();
        }

        private void CameraCapture_Load(object sender, EventArgs e)
        {
            // adjust path to find your xml at loading
            haar = new HaarCascade("haarcascade_frontalface_alt_tree.xml");

        }
        ////////////////////////////////////////////////////////////////////////////////////////
        //               FUNCTIONS USED TO TAKE INPUT IMAGE FROM CAMERA
        ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Connects to the selected camera attached to the system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbCamIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Set the camera number to the one selected via combo box
            int CamNumber = -1;
            CamNumber = int.Parse(cbCamIndex.Text);

            //Start the selected camera
            #region if capture is not created, create it now
            if (capture == null)
            {
                try
                {
                    capture = new Capture(CamNumber);
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }
            #endregion

            //Start showing the stream from camera
            btnStart_Click(sender, e);
            btnStart.Enabled = true;
        }

        /// <summary>
        /// Starts live video streaming, Pauses it to detect faces, Resumes it 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {

            if (capture != null)
            {
                if (btnStart.Text == "Detect Face")
                {  //if camera is getting frames then pause the capture and set button Text to
                    // "Resume" for resuming capture
                    btnStart.Text = "Resume Live Video"; //

                    //Pause the live streaming video
                    Application.Idle -= ProcessFrame;

                    //Call face detection
                    DetectFaces();
                }
                else
                {
                    //if camera is NOT getting frames then start the capture and set button
                    // Text to "Pause" for pausing capture
                    btnStart.Text = "Detect Face";
                    Application.Idle += ProcessFrame;
                }
            }
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            //fetch the frame captured by web camera
            ImageFrame = capture.QueryFrame();

            //show the image in the EmguCV ImageBox
            CamImageBox.Image = ImageFrame;
        }

        /// <summary>
        /// Disconnects from the camera
        /// </summary>
        private void ReleaseData()
        {
            if (capture != null)
                capture.Dispose();
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //               FUNCTIONS USED TO TAKE INPUT IMAGE FROM FOLDER
        ////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Loads an image from Hard disk and detects faces from it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Image InputImg = Image.FromFile(openFileDialog.FileName);
                ImageFrame = new Image<Bgr, byte>(new Bitmap(InputImg));
                CamImageBox.Image = ImageFrame;
                DetectFaces();
            }

        }

        ////////////////////////////////////////////////////////////////////////////////////////
        //               FUNCTIONS USED TO DETECT FACES IN INPUT IMAGE
        ////////////////////////////////////////////////////////////////////////////////////////
        private void DetectFaces()
        {
            Image<Gray, byte> grayframe = ImageFrame.Convert<Gray, byte>();

            //Assign user-defined Values to parameter variables:
            MinNeighbors = int.Parse(comboBoxMinNeigh.Text);  // the 3rd parameter
            WindowsSize = int.Parse(textBoxWinSiz.Text);   // the 5th parameter
            ScaleIncreaseRate = Double.Parse(comboBoxScIncRte.Text); //the 2nd parameter

            //detect faces from the gray-scale image and store into an array of type 'var',i.e 'MCvAvgComp[]'
            var faces = grayframe.DetectHaarCascade(haar, ScaleIncreaseRate, MinNeighbors,
                                    HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                                    new Size(WindowsSize, WindowsSize))[0];
            if (faces.Length > 0)
            {
                MessageBox.Show("Total Faces Detected: " + faces.Length.ToString());
                Bitmap BmpInput = grayframe.ToBitmap();
                Bitmap ExtractedFace;   //empty
                Graphics FaceCanvas;
                ExtFaces = new Bitmap[faces.Length];
                faceNo = 0;
                
                //draw a green rectangle on each detected face in image
                foreach (var face in faces)
                {
                    ImageFrame.Draw(face.rect, new Bgr(Color.Green), 3);

                    //set the size of the empty box(ExtractedFace) which will later contain the detected face
                    ExtractedFace = new Bitmap(face.rect.Width, face.rect.Height);

                    //set empty image as FaceCanvas, for painting
                    FaceCanvas = Graphics.FromImage(ExtractedFace);

                    FaceCanvas.DrawImage(BmpInput, 0, 0, face.rect, GraphicsUnit.Pixel);

                    ExtFaces[faceNo] = ExtractedFace;
                    faceNo++;
                }                        
                                                
                pbExtractedFaces.Image = ExtFaces[0];
                
                //Display the detected faces in imagebox
                CamImageBox.Image = ImageFrame;

                btnNext.Enabled = true;
                btnPrev.Enabled = true;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (faceNo < ExtFaces.Length - 1)
            {
                faceNo++;
                pbExtractedFaces.Image = ExtFaces[faceNo];
            }
            else
                MessageBox.Show("Last image!");
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (faceNo > 0)
            {
                faceNo--;
                pbExtractedFaces.Image = ExtFaces[faceNo];
            }
            else
                MessageBox.Show("1st image!");
        }

        
    }
    
}    
        

