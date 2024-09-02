using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using HandAnalyzer.Structures;
using NetManager;
using System.IO;

namespace HandAnalyzer
{
    public partial class Form1 : Form
    {
        List<String> titles = new List<String>();

        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        int redThreshold = 150;
        double blueThreshold = 150;
        double[] maxMean = new double[29];
        int bucketsCounter = 0;
        int accBucketsCount = 0;
        int analise_window = 0;
        int dec = 0;
        bool abscorrFlag = false;
        //double[] maxV = new double[29];
        //double[] minV = new double[29];
        bool testFlag = false;
        double minV = 99999999999;
        double maxV = -99999999999;
        int floor = 0;


        bool timerFlag = false;
        double valThreshold = 2;
        double[] currentCorrValue = new double[29];
        double[] currentAbsValue = new double[29];
        Dictionary<FortuneSite, String> blobsDictionary = new Dictionary<FortuneSite, String>();
        Dictionary<String, List<double>>  meanDictionary = new Dictionary<String, List<double>>();
        Dictionary<String, List<double>> currentDictionary = new Dictionary<String, List<double>>();
        double valBlueThreshold = 2;
        public Form1()
        {
            InitializeComponent();
        }

        private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (videoSource.IsRunning)
            {
                blobsDictionary.Clear();//очистка канала-точки
                var bitmap = (Bitmap)eventArgs.Frame.Clone();
                var blueBitmap = (Bitmap)eventArgs.Frame.Clone();
                var copy = (Bitmap)eventArgs.Frame.Clone();
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                BitmapData bitmapDataB = blueBitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, blueBitmap.PixelFormat);
                int w = bitmap.Width;
                int h = bitmap.Height;
                //System.Threading.Tasks.Parallel.For(0, bitmap.Height, y =>
                for(int y = 0; y<bitmap.Height; y++)
                {
                    unsafe
                    {
                        byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                        byte* rowB = (byte*)bitmapDataB.Scan0 + (y * bitmapDataB.Stride);

                        for (int x = 0; x < w; x++)
                        {
                            byte b = row[x * 3];
                            byte g = row[x * 3 + 1];
                            byte r = row[x * 3 + 2];

                            if (!(r > redThreshold && r > g * valThreshold && r > b * valThreshold))
                            {
                                row[x * 3] = 0;
                                row[x * 3 + 1] = 0;
                                row[x * 3 + 2] = 0;
                            }
                            else
                            {
                                row[x * 3] = 255;
                                row[x * 3 + 1] = 255;
                                row[x * 3 + 2] = 255;
                            }

                            if (!(b > blueThreshold && b > g * valBlueThreshold && b > r * valBlueThreshold))
                            {
                                rowB[x * 3] = 0;
                                rowB[x * 3 + 1] = 0;
                                rowB[x * 3 + 2] = 0;
                            }
                            else
                            {
                                rowB[x * 3] = 255;
                                rowB[x * 3 + 1] = 255;
                                rowB[x * 3 + 2] = 255;
                            }
                        }
                    }
                }
                bitmap.UnlockBits(bitmapData);
                blueBitmap.UnlockBits(bitmapDataB);

                Grayscale grayscaleFilter = new Grayscale(1, 1, 1);
                Bitmap grayImage = grayscaleFilter.Apply(bitmap);
                Bitmap grayBlueImage = grayscaleFilter.Apply(blueBitmap);
                // Создание экземпляра BlobCounter
                BlobCounter blobCounter = new BlobCounter();

                // Выделение параметров для поиска
                blobCounter.MinWidth = blobCounter.MinHeight = 15; // Минимальные размеры окружности
                blobCounter.MaxWidth = blobCounter.MaxHeight = 1000; // Максимальные размеры окружности
                blobCounter.FilterBlobs = true; // Применить фильтрацию по размеру

                // Применение BlobCounter к изображению
                blobCounter.ProcessImage(grayImage);

                // Получение списка найденных окружностей
                Blob[] blobs = blobCounter.GetObjectsInformation();
                blobCounter.ProcessImage(grayBlueImage);
                Blob[] blueBlobs = blobCounter.GetObjectsInformation();
                List<FortuneSite> fs = new List<FortuneSite>();

                int titlesCounter = 0;
                Graphics mainImage = Graphics.FromImage(copy);
                Bitmap croppedImage = new Bitmap(pictureBox2.Width, pictureBox2.Height);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    Pen pen = new Pen(Color.Red, 2); // Цвет и толщина обводки окружности

                    foreach (Blob blob in blobs)
                    {
                        
                        float centerX = blob.CenterOfGravity.X;
                        float centerY = blob.CenterOfGravity.Y;
                        float radius = (10); // Используем Fullness для приближенного вычисления радиуса
                        if (titlesCounter < titles.Count)
                        {
                            mainImage.DrawString(titles[titlesCounter], new Font("Times New Roman", 30.0f), new SolidBrush(Color.Purple), new System.Drawing.Point((int)(centerX - 30), (int)(centerY - 30)));
                        }
                        graphics.DrawEllipse(pen, centerX - radius, centerY - radius, 2 * radius, 2 * radius);
                        if (0 < centerX && centerX < bitmap.Width && 0 < centerY && centerY < bitmap.Height) {
                            var fSite = new FortuneSite(centerX - radius, centerY - radius);
                            fs.Add(fSite);
                            if (titlesCounter < titles.Count)
                                blobsDictionary.Add(fSite, titles[titlesCounter]);  
                        }
                        titlesCounter++;
                    }

                    pen.Dispose();
                }
                floor = fs.Count();
                Border border = null;
                using (Graphics graphics = Graphics.FromImage(blueBitmap))
                {
                    Pen pen = new Pen(Color.Red, 2); // Цвет и толщина обводки окружности

                    foreach (Blob blob in blueBlobs)
                    {
                        float centerX = blob.CenterOfGravity.X;
                        float centerY = blob.CenterOfGravity.Y;
                        float radius = (10); // Используем Fullness для приближенного вычисления радиуса

                        graphics.DrawEllipse(pen, centerX - radius, centerY - radius, 2 * radius, 2 * radius);
                        //fs.Add(new FortuneSite(centerX - radius, centerY - radius));
                    }

                    pen.Dispose();
                }
                if (blueBlobs.Length >= 4)
                {
                    List<System.Drawing.Point> borderP = new List<System.Drawing.Point>();
                    for (int i = 0; i < blueBlobs.Length; i++)
                    {
                        borderP.Add(new System.Drawing.Point((int)blueBlobs[i].CenterOfGravity.X, (int)blueBlobs[i].CenterOfGravity.Y));
                    }

                    border = new Border(borderP);
                    var borders = border.GetEdges();
                    using (Graphics graphics = Graphics.FromImage(copy))
                    {
                        Pen pen = new Pen(Color.Blue, 2); // Цвет и толщина обводки окружности

                        for (int i = 0; i < borders.Count && i<4; i++)
                        {
                            graphics.DrawEllipse(pen, blueBlobs[i].CenterOfGravity.X - 10, blueBlobs[i].CenterOfGravity.Y - 10, 2 * 10, 2 * 10);
                            graphics.DrawLine(pen, borders[i].start, borders[i].end);
                        }

                        pen.Dispose();
                    }
                }

                var a = FortunesAlgorithm.Run(fs, 0, 0, w, h);

                using (Graphics graphics = Graphics.FromImage(copy))
                {
                    Random rnd = new Random();
                    Pen pen = new Pen(Color.Green, 2); // Цвет и толщина обводки окружности
                    Pen penR= new Pen(Color.Red, 2);
                    SolidBrush brush = new SolidBrush(Color.FromArgb(90, Color.Red));




                    foreach (VEdge vEdge in a)
                    { 
                        if (border != null)
                        {
                            if (border.GetEdges().Count == 4)
                            {
                                var p1 = new System.Drawing.Point((int)vEdge.Start.X, (int)vEdge.Start.Y);
                                var p2 = new System.Drawing.Point((int)vEdge.End.X, (int)vEdge.End.Y);
                                var c = border.perm(p1, p2);

                                graphics.DrawLine(pen, c[0].X, c[0].Y, c[1].X, c[1].Y);
                            }
                        }
                        //graphics.DrawLine(pen, (float)vEdge.Start.X, (float)vEdge.Start.Y, (float)vEdge.End.X, (float)vEdge.End.Y);
                    }

                    Bitmap beforePolygon = (Bitmap) copy.Clone();
                    
                    foreach (FortuneSite site in fs)
                    {
                        List<System.Drawing.Point> points = new List<System.Drawing.Point>();
                        if (border != null)
                        {
                            if (site.Points.Count > 1 && border.GetEdges().Count == 4)
                            {
                                for (int i = 0; i < site.Points.Count; i++)
                                {
                                    points.Add(new System.Drawing.Point((int)site.Points[i].X, (int)site.Points[i].Y));
                                }
                                if (fs.Count <= titles.Count && timerFlag)
                                {
                                    var title = blobsDictionary[site];
                                    var red = 0;
                                    int green = 255;
                                    if (!abscorrFlag)
                                    {
                                        var coef = Math.Abs(currentCorrValue[titles.IndexOf(title)]);
                                        //coef -= 1;
                                        if (coef < 0 || coef > 1)
                                        {
                                            green = 0;
                                            red = 255;
                                        }
                                        else
                                        {
                                            green = (int)(255 * (coef));
                                            red = (int)(255 * (1 - coef));
                                        }
                                    }
                                    else
                                    {
                                        var coef = currentAbsValue[titles.IndexOf(title)];
                                        var del = 0.1;
                                        if (minV < maxV)
                                        {
                                            del = (coef - minV) / (maxV - minV);
                                        }
                                        else



                                            green = (int)(255 * (del));
                                            red = (int)(255 * (1 - del));
                                            green = (int)(255 * (del));

                                    }
                                    graphics.FillPolygon(new SolidBrush(Color.FromArgb(100, red, green, 0)), points.ToArray());
                                }
                            }
                        }
                    }
                    if (border != null)
                    {
                        var angels = border.getAnglePoints();
                        if (angels.Length >3)
                        {

                            Rectangle rectangle = new Rectangle(System.Drawing.Point.Empty, pictureBox2.ClientSize);

                    
                            GraphicsPath polygonPath = new GraphicsPath();
                            polygonPath.AddPolygon(angels);

                            // Создаем регион на основе Rectangle
                            Region rectangleRegion = new Region(rectangle);

                            // Создаем регион на основе полигона
                            Region polygonRegion = new Region(polygonPath);

                            // Вычитаем полигон из Rectangle
                            rectangleRegion.Exclude(polygonRegion);


                            // Создаем новый битмап, содержащий только область, на которую наложена маска
                            croppedImage = new Bitmap(pictureBox2.Width, pictureBox2.Height);
                            using (Graphics croppedGraphics = Graphics.FromImage(croppedImage))
                            {
                                croppedGraphics.Clip = rectangleRegion;
                                croppedGraphics.DrawImage(beforePolygon, 0, 0);
                            }
                            graphics.DrawImage(croppedImage, 0, 0);
                        }
                    }

                    pen.Dispose();
                }

                    pictureBox1.Invoke(new Action(() => pictureBox1.Image = bitmap));
                    pictureBox2.Invoke(new Action(() => pictureBox2.Image = copy));
                    //pictureBox3.Invoke(new Action(() => pictureBox3.Image = grayBlueImage));//d
                    pictureBox4.Invoke(new Action(() => pictureBox4.Image = blueBitmap));//d
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(videoSource!=null & videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }      
        }

        private void MessageHandler(object sender, EventClientMsgArgs e)
        {
            //if (Thread.CurrentThread.Name == "MainFormThread")
            //{
                Frame f = new Frame(e.Msg);
                var bucket = new double[29, 24];
                var data = f.Data;
                if (timerFlag)
                {

                    accBucketsCount += 24;

                    for (int i = 0; i < 29; i++)
                    {
                        for (int j = 0; j < 24; j++)
                        {
                            bucket[i, j] = Convert.ToDouble(data[i * 24 + j]);
                            if (meanDictionary[titles[i]].Count > currentDictionary[titles[i]].Count)
                                currentDictionary[titles[i]].Add(bucket[i, j]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 29; i++)
                    {
                        for (int j = 0; j < 24; j++)
                        {
                            bucket[i, j] = Convert.ToDouble(data[i * 24 + j]);
                        }
                    }
                }

                if (!timerFlag && timer1.Enabled)
                {
                    for (int i = 0; i < 29; i++)
                    {
                        for (int j = 0; j < 24; j++)
                        {
                            meanDictionary[titles[i]].Add(bucket[i, j]);
                        }
                    }
                }
            //}
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

            reseiveClientControl1.Client.StartClient();
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            for (int i = 0; i < videoDevices.Count; i++)
            {
                comboBox1.Items.Add(videoDevices[i].MonikerString);
            }

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
           
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
          
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            valThreshold = trackBar1.Value * 0.05;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            redThreshold = trackBar2.Value;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            blueThreshold = trackBar3.Value;
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            valBlueThreshold = trackBar4.Value * 0.05;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(abscorrFlag)
            {
                dec = Convert.ToInt32(textBox1.Text);
                analise_window = Convert.ToInt32(textBox2.Text);
                timer1.Interval= analise_window;
            }
            timer1.Start();
            button3.Enabled = true;
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!timerFlag)
            {
                timerFlag = true;
            }
            else
            {
                for (int i = 0; i < 29; i++)
                {
                    if (meanDictionary[titles[i]].Count == currentDictionary[titles[i]].Count)
                    {
                        if (!abscorrFlag)
                        {
                            currentCorrValue[i] = CorrelatorSpirman.Calculate(meanDictionary[titles[i]].ToArray(), currentDictionary[titles[i]].ToArray());
                            currentDictionary[titles[i]].Clear();
                        }
                        else
                        {
                            var sum = Abs_value.calculate_abs(currentDictionary[titles[i]].ToArray(), analise_window/10, dec);                          
                            currentAbsValue[i] = sum;
                            if((testFlag)&&(i<floor))
                            {
                                if (sum > maxV)
                                    maxV = sum;
                                if ((sum < minV)&&(sum!=0))
                                    minV = sum;
                            }
                            currentDictionary[titles[i]].Clear();
                        }
                    }
                }

            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice((string)comboBox1.SelectedItem);
                videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);
                videoSource.Start();
            }
            else
            {
                MessageBox.Show("No video devices found.");
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            abscorrFlag = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.ShowDialog();
                string chan_filename = openFileDialog1.FileName;
                var chan_file = File.ReadAllLines(chan_filename, Encoding.Default);
                foreach (string chan in chan_file)
                    titles.Add(chan);
            }
            catch (Exception ex)
            {

            }

            for (int i = 0; i < 29; i++)
            {
                meanDictionary.Add(titles[i], new List<double>());
                currentDictionary.Add(titles[i], new List<double>());
            }
            button2.Enabled = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            abscorrFlag = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            abscorrFlag = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!testFlag)
            {
                //for (int i = 0; i < minV.Count(); i++)
                //{
                    minV = 99999999999;
                //}
                //minV[28] = 0;
                //for (int i = 0; i < maxV.Count(); i++)
                //{
                    maxV = -9999999999;
                //}
                testFlag = true;
            }
            else
                testFlag = false;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
