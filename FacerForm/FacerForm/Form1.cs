using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

namespace FacerForm
{
    public partial class Form1 : Form
    {
        private float randFact=0.5f;

        private readonly string eyes;
        private readonly string mouth;
        private readonly string nose;

        private PartType partType;
        private Image origFile;
        private readonly Random rnd;


        public enum PartType
        {
            Nose = 1,
            Eyses = 2,
            Mouth = 3,
            All = 4
        }

        public Form1()
        {
            eyes = Application.StartupPath + "\\Haar\\Eyes.xml";
            nose = Application.StartupPath + "\\Haar\\Nariz.xml";
            mouth = Application.StartupPath + "\\Haar\\Mouth.xml";

            rnd = new Random();

            InitializeComponent();

            if (comboBox1 != null)
            {
                comboBox1.DataSource = Enum.GetNames(typeof (PartType));
            }


            Enum.TryParse(comboBox1.SelectedValue.ToString(), out partType);

            label1.Text = trackBar1.Value.ToString() + "%";
        }
        [DllImport("FaceRecognitionDll.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_CheckPhoto@4", ExactSpelling = true)]
        static extern int CheckPhoto(string path);


        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "bmp files (*.bmp)|*.bmp";
                dlg.Filter = "jpg files (*.jpg)|*.jpg";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // pictureBox2 = new PictureBox();

                    Image image = Image.FromFile(dlg.FileName);
                    pictureBox2.Image = Bitmap.FromFile(dlg.FileName); //image;
                    pictureBox2.Height = image.Height;
                    pictureBox2.Width = image.Width;
                    origFile = image;
                }
            }
        }

        Rectangle Process(string path,Bitmap bm)
        {
            var _cascadeClassifier =
                new CascadeClassifier(path);

            var image = new Image<Bgr, byte>(bm);
            var imageOutput = new Image<Bgr, byte>(bm);
            // var grayframe = image.Convert<Gray, byte>();
            var faces = _cascadeClassifier.DetectMultiScale(image, 2, 10, Size.Empty);
            //the actual face detection happens here
            foreach (var face in faces)
            {
                imageOutput.Draw(faces[0], new Bgr(), 3);
                //the detected face(s) is highlighted here using a box that is drawn around it/them
            }
            pictureBox2.Image = new Bitmap(imageOutput.ToBitmap());
            if (faces.Length > 0)
                return faces[0];

            return new Rectangle(0,0,0,0);
        }

        void Search()
        {
            switch (partType)
            {
                case PartType.Nose:
                    Process(nose,(Bitmap)origFile);
                    break;
                case PartType.Eyses:
                    Process(eyes, (Bitmap)origFile);
                    break;
                case PartType.Mouth:
                    Process(mouth, (Bitmap)origFile);
                    break;
                case PartType.All:
                    Process(nose, (Bitmap)origFile);
                    Process(eyes, (Bitmap)origFile);
                    Process(mouth, (Bitmap)origFile);
                    break;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Enum.TryParse<PartType>(comboBox1.SelectedValue.ToString(), out partType);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                // Set the file dialog to filter for graphics files.
                dlg.Filter =
                    "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|" +
                    "All files (*.*)|*.*";

                // Allow the user to select multiple images.
                dlg.Multiselect = true;
                dlg.Title = "My Image Browser";
                DialogResult dr = dlg.ShowDialog();

                Series[] series = new Series[3];
                Photo[] nosePhotos = new Photo[dlg.FileNames.Length];
                Photo[] mouthPhotos = new Photo[dlg.FileNames.Length];
                Photo[] eysPhotos = new Photo[dlg.FileNames.Length];

               series[0]= CreateSeries(dlg, nose, ref series, ref nosePhotos);
                series[1] = CreateSeries(dlg, mouth, ref series, ref mouthPhotos);
                series[2] = CreateSeries(dlg, eyes, ref series, ref eysPhotos);

                MulitSeries multiSeries = new MulitSeries(series);

                XmlSerialization(multiSeries);
            }
        }

        private static void XmlSerialization(MulitSeries multiSeries)
        {
            System.Xml.Serialization.XmlSerializer writer =
                 new System.Xml.Serialization.XmlSerializer(typeof(MulitSeries));

            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "//SerializationOverview.xml";
            System.IO.FileStream outputFile = System.IO.File.Create(path);

            writer.Serialize(outputFile, multiSeries);
            outputFile.Close();
        }

        private Series CreateSeries(OpenFileDialog dlg,string type,ref Series[] series,ref Photo[] photos)
        {
            int i = 0;
            foreach (var file in dlg.FileNames)
            {
                photos[i] = RandPhotos(file, type);
                i++;
            }

            return new Series(Path.GetFileName(type), photos);
        }

        private Photo RandPhotos(string file,string type)
        {
            randFact = 0.0005f;//rnd.Next(0, 20))/100.0f;

            Bitmap bmp =(Bitmap)Image.FromFile(file);
            Photo photo = new Photo(Path.GetFileName(file),bmp.Width, bmp.Height,(int)(randFact*100), RandPixels(Process(type,bmp), ref bmp));
            bmp.Save("new_"+ Path.GetFileName(type)+"_" + Path.GetFileName(file));
            return photo;
        }

        private Pixel [] RandPixels(Rectangle rect,ref Bitmap bmp)
        {

            int size = (int)((float)bmp.Width*(float)bmp.Height*randFact);
            Pixel[] pixels=new Pixel[size];

            int x = rect.X, y = rect.Y;

            for (int i = 0; i < size; i++)
            {
                
                while (Array.Find(pixels, p => p!=null&&p.x==x&&p.y==y)!=null)
                {
                    x = rnd.Next(rect.X, rect.X+ rect.Width);
                    y = rnd.Next(rect.Y, rect.Y+ rect.Height);
                    Console.WriteLine(i);
                }
                pixels[i] = RandChangePixel(x,y,ref bmp);

            }
            

            return pixels;

        }

        private Pixel RandChangePixel(int x, int y,ref Bitmap bmp )
        {
           
            Color color = bmp.GetPixel(x, y);
            MyColor oldColor=new MyColor (color.R,color.G,color.B);

            
            int month = rnd.Next(1, 13);

            MyColor newColor = new MyColor(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
            bmp.SetPixel(x, y,Color.FromArgb(newColor.Red, newColor.Green, newColor.Blue,0));

            return new Pixel(newColor,oldColor,x,y);

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            //  int a = 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            randFact = ((float) trackBar1.Value)/100.0f;
            label1.Text = trackBar1.Value.ToString()+"%";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                // Set the file dialog to filter for graphics files.
                dlg.Filter =
                    "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|" +
                    "All files (*.*)|*.*";

                // Allow the user to select multiple images.
                dlg.Multiselect = true;
                dlg.Title = "My Image Browser";
                DialogResult dr = dlg.ShowDialog();

                foreach (var file in dlg.FileNames)
                {
                    var returned = 0;
                    try
                    {
                        returned = CheckPhoto(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("!Błąd - Biblioteka zgłosiła wyjątek: {0}", ex.ToString());
                    }
                    Console.WriteLine(returned);
                }
            }

            
        }
    }
}