﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Image = System.Drawing.Image;
using ToolTip = System.Windows.Forms.ToolTip;

namespace My_paint
{
    
    public partial class Form1 : Form
    {
        class DraggedFragment
        {
            public Rectangle SourceRect;
            public Point Location;

            public Rectangle Rect
            {
                get { return new Rectangle(Location, SourceRect.Size); }
            }

            public void Fix(Image image)
            {
                using (var clone = (Image)image.Clone())
                using (var gr = Graphics.FromImage(image))
                {
                    //рисуем вырезанное белое место
                    gr.SetClip(SourceRect);
                    gr.Clear(Color.White);

                    //рисуем сдвинутый фрагмент
                    gr.SetClip(Rect);
                    gr.DrawImage(clone, Location.X - SourceRect.X, Location.Y - SourceRect.Y);
                    
                }
            }

        }

        Point p1, p2;
        private string userText;
        static Color c1 = Color.Crimson, c2 = Color.White, zal;
        Boolean risch, chcol, fail;
        Point oldMouse;
        int x2, y2, x3, y3, x4, y4;
        static Point[] gpoint;
        private Point mousePos1;
        private Point mousePos2;
        private DraggedFragment draggedFragment;
        static Pen cont = new Pen(col, 2);
        public static Bitmap image;
        static Color col = Color.Black, ncol = Color.Black, fill_col=Color.White, border;
        static Pen pen = new Pen(col, 1);
        int pen_size = 1;
        int x1, y1;
        public static Bitmap nbmp, bitmap, bmp, colbitmap;
        int intr;
        private int textSize = 16;
        public static Stack<Bitmap> buf = new Stack<Bitmap>();
        static Stack<Bitmap> buf2 = new Stack<Bitmap>();
        Graphics g;
        Point[] resize_point = new Point[8];
        bool[] resize=new bool[8];
        public static UInt32[,] pixel;
        public static string full_name_of_image = "\0";
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;

            Bitmap btm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = btm;
            buf.Push(btm);
            intr = 1;

            Form1_Load(null, null);

        }


        private void bufer()
        {
            buf.Push(new Bitmap(bmp));
        }

        Rectangle GetRect(Point p1, Point p2)
        {
            var x1 = Math.Min(p1.X, p2.X);
            var x2 = Math.Max(p1.X, p2.X);
            var y1 = Math.Min(p1.Y, p2.Y);
            var y2 = Math.Max(p1.Y, p2.Y);
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            col = Color.FromArgb(col.A, trackBar1.Value, col.G, col.B);
            label7.BackColor = col;
        }
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            col = Color.FromArgb(col.A, col.R, trackBar2.Value, col.B);
            label7.BackColor = col;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            col = Color.FromArgb(col.A, col.R, col.G, trackBar3.Value);
            label7.BackColor = col;
        }
        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            col = Color.FromArgb(trackBar4.Value, col.R, col.G, col.B);
            label7.BackColor = col;
        }
        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            int bright = (ncol.R + ncol.B + ncol.G) / 3;
            int delta = bright - trackBar5.Value;
            int r, g, b;
            r = ncol.R - delta;
            g = ncol.G - delta;
            b = ncol.B - delta;
            if (r > 255)
                r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;
            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;
            col = Color.FromArgb(col.A, r, g, b);
            label7.BackColor = col;
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            pen_size = trackBar6.Value;
        }

        private void сщхранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            open();
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save_new();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Кнопка "Назад"
            if (buf.Count > 1)
            {
                buf2.Push(buf.Pop()); 
                pictureBox1.Image = (Bitmap)buf.Peek().Clone(); 
                panel1.Controls.Remove(panel1.Controls[panel1.Controls.Count - 1]); 
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            // Кнопка "Вперед"
            if (buf2.Count > 0)
            {
                var newImage = (Bitmap)buf2.Pop().Clone(); 
                buf.Push(newImage); 

                pictureBox1.Image = (Bitmap)newImage.Clone();

                PictureBox pic = new PictureBox
                {
                    Location = new Point(10, panel1.Controls.Count * 66),
                    Width = 65,
                    Height = 42,
                    Image = resizeImage(newImage, new Size(65, 42))
                };

                panel1.Controls.Add(pic);
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            intr = 1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            intr = 2;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            intr = 3;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            intr = 4;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            intr = 6;
            border = col;
        }
        private void button9_Click(object sender, EventArgs e)
        {
            intr = 7;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBox3.Font = fontDialog1.Font;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            userText = textBox3.Text;
            intr = 11;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////
        /// палитра
        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            chcol = true;
            colbitmap = new Bitmap(pictureBox2.Image);
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            chcol = false;
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            float stretch_x = colbitmap.Width / (float)pictureBox2.Width;
            float stretch_y = colbitmap.Height / (float)pictureBox2.Height;
            col = colbitmap.GetPixel((int)(e.X * stretch_x), (int)(e.Y * stretch_y));
            trackBar1.Value = col.R;
            trackBar2.Value = col.G;
            trackBar3.Value = col.B;
            trackBar4.Value = col.A;
            trackBar5.Value = (col.R + col.B + col.G) / 3;
            ncol = col;
            label7.BackColor = col;
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (chcol)
            {
                if ((e.X >= 0) && (e.X <= pictureBox2.Width) && (e.Y >= 0) && (e.Y <= pictureBox2.Width))
                {
                    float stretch_x = colbitmap.Width / (float)pictureBox2.Width;
                    float stretch_y = colbitmap.Height / (float)pictureBox2.Height;
                    col = colbitmap.GetPixel((int)(e.X * stretch_x), (int)(e.Y * stretch_y));
                    trackBar1.Value = col.R;
                    trackBar2.Value = col.G;
                    trackBar3.Value = col.B;
                    trackBar4.Value = col.A;
                    trackBar5.Value = (col.R + col.B + col.G) / 3;
                }
            }
            ncol = col;
            label7.BackColor = col;
        }
        private void чернобелыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bmp = (Bitmap)pictureBox1.Image.Clone();
            for (int i = 0; i<bmp.Width; ++i)
            {
                for (int j=0; j<bmp.Height;++j)
                {
                    UInt32 pixel = (UInt32)(bmp.GetPixel(i, j).ToArgb());
                    float R = (float)((pixel & 0x00FF0000) >> 16); 
                    float G = (float)((pixel & 0x0000FF00) >> 8); 
                    float B = (float)(pixel & 0x000000FF);
                                                          
                    R = G = B = (R + G + B) / 3.0f;
                    UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);
                    bmp.SetPixel(i, j, Color.FromArgb((int)newPixel));
                }
            }
            pictureBox1.Image = (Bitmap)bmp.Clone();
            buf.Push((Bitmap)pictureBox1.Image.Clone());
            ToBufer(buf.Count);
        }
        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bmp = (Bitmap)pictureBox1.Image.Clone();
            for (int i = 0; i < bmp.Width; ++i)
            {
                for (int j = 0; j < bmp.Height; ++j)
                {
                    UInt32 pixel = (UInt32)(bmp.GetPixel(i, j).ToArgb());
                    float R = (float)((pixel & 0x00FF0000) >> 16);
                    float G = (float)((pixel & 0x0000FF00) >> 8);  
                    float B = (float)(pixel & 0x000000FF);     
                    int grayScale = (int)((R * .3) + (G * .59) + (B * .11));

                    Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);

                    R = newColor.R * 1;
                    G = newColor.G * 0.95f;
                    B = newColor.B * 0.82f;

                    UInt32 newPixel = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);
                    bmp.SetPixel(i, j, Color.FromArgb((int)newPixel));
                }
            }
            pictureBox1.Image = (Bitmap)bmp.Clone();
            buf.Push((Bitmap)pictureBox1.Image.Clone());
            ToBufer(buf.Count);
        }
        private void каналыToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Form2 ColorBalanceForm = new Form2(this);
            ColorBalanceForm.ShowDialog();
        }
        private void яркостьКонтрастностьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 BrightnessForm = new Form3(this);
            BrightnessForm.ShowDialog();
        }
        private void повышениеРезкостиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (full_name_of_image != "\0")
            {
                pixel = Filter.matrix_filtration(image.Width, image.Height, pixel, Filter.N1, Filter.sharpness);
                FromPixelToBitmap();
                FromBitmapToScreen();
                pictureBox1.Image = (Bitmap)image.Clone();
                buf.Push((Bitmap)pictureBox1.Image.Clone());
                ToBufer(buf.Count);
            }
        }
        private void размытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (full_name_of_image != "\0")
            {
                pixel = Filter.matrix_filtration(image.Width, image.Height, pixel, Filter.N2, Filter.blur);
                FromPixelToBitmap();
                FromBitmapToScreen();
                pictureBox1.Image = (Bitmap)image.Clone();
                buf.Push((Bitmap)pictureBox1.Image.Clone());
                ToBufer(buf.Count);
            }
        }
        public static void FromPixelToBitmap()
        {
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                    image.SetPixel(x, y, Color.FromArgb((int)pixel[y, x]));
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            
            if (draggedFragment != null)
            {
                e.Graphics.SetClip(draggedFragment.SourceRect);
                e.Graphics.Clear(Color.White);

                e.Graphics.SetClip(draggedFragment.Rect);
                e.Graphics.DrawImage(pictureBox1.Image, draggedFragment.Location.X - draggedFragment.SourceRect.X, draggedFragment.Location.Y - draggedFragment.SourceRect.Y);

                e.Graphics.ResetClip();
                ControlPaint.DrawFocusRectangle(e.Graphics, draggedFragment.Rect);
            }
            else
            {
                if (mousePos1 != mousePos2)
                    ControlPaint.DrawFocusRectangle(e.Graphics, GetRect(mousePos1, mousePos2));
            }
            
        }
        /////////////////////////////////////////////////////////////////////////////////////////////
        /// события на холсте
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //координаты
            this.Cursor= System.Windows.Forms.Cursors.Default;
            if ((e.X >= 0) && (e.X <= pictureBox1.Width))
                textBox1.Text = e.X.ToString();
            if ((e.Y >= 0) && (e.Y <= pictureBox1.Width))
                textBox2.Text = e.Y.ToString();
            if (risch)
            {
                switch (intr) {
                    case 1:
                        draw_pen(x2, y2, x1, y1, e.X, e.Y);
                        break;
                    case 2:
                        draw_regtangle(oldMouse.X, oldMouse.Y, e.X, e.Y);
                        break;
                    case 3:
                        draw_ellipse(oldMouse.X, oldMouse.Y, e.X, e.Y);
                        break;
                    case 4:
                        if (e.Button == MouseButtons.Left)
                        {
                            if (draggedFragment != null)
                            {
                                //сдвигаем фрагмент
                                draggedFragment.Location.Offset(e.Location.X - mousePos2.X, e.Location.Y - mousePos2.Y);
                                mousePos1 = e.Location;
                            }
                            //сдвигаем выделенную область
                            mousePos2 = e.Location;
                            pictureBox1.Invalidate();
                        }
                        else
                        {
                            mousePos1 = mousePos2 = e.Location;
                        }
                        
                        break;
                    case 7:
                        draw_polygon(oldMouse.X, oldMouse.Y, e.X, e.Y);
                        break;
                    case 11:
                        if (!string.IsNullOrEmpty(userText)) 
                        {
                            draw_text(userText, e.X, e.Y); 
                        }
                        break;

                }
                x2 = x1;
                y2 = y1;
                x1 = e.X;
                y1 = e.Y;
            }
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            buf2.Clear();
            oldMouse = new Point(e.X, e.Y);

            risch = true;
            x1 = e.X;
            y1 = e.Y;
            x2 = x1;
            y2 = y1;
            bitmap = new Bitmap(pictureBox1.Image);
            bmp = (Bitmap)bitmap.Clone();
            if (intr==4)
            {
                if (draggedFragment != null && !draggedFragment.Rect.Contains(e.Location))
                {
                    draggedFragment = null;
                    pictureBox1.Invalidate();
                    
                }
            }
            else if (intr==6)
            {
                risch = false;
                g= Graphics.FromImage(bmp);
                colbitmap= new Bitmap(pictureBox1.Image);
                float stretch_x = colbitmap.Width / (float)pictureBox1.Width;
                float stretch_y = colbitmap.Height / (float)pictureBox1.Height;
                fill_col = colbitmap.GetPixel((int)(e.X * stretch_x), (int)(e.Y * stretch_y));
                bmp = filling(bitmap, e.X, e.Y, col);
                pictureBox1.Image=(Bitmap)bmp.Clone();
                
            }

        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (intr == 4)
            {
                if (mousePos1 != mousePos2)
                {
                    var rect = GetRect(mousePos1, mousePos2);
                    draggedFragment = new DraggedFragment() { SourceRect = rect, Location = rect.Location };
                }
                else
                {
                    if (draggedFragment != null)
                    {
                        draggedFragment.Fix(pictureBox1.Image);
                        draggedFragment = null;
                        mousePos1 = mousePos2 = e.Location;
                    }
                }
                pictureBox1.Invalidate();
                risch = false;
                buf.Push((Bitmap)pictureBox1.Image.Clone());
                ToBufer(buf.Count);
            }
            else { 
                pictureBox1.Image = (Bitmap)bmp.Clone();
                risch = false;
                buf.Push((Bitmap)pictureBox1.Image.Clone());
                ToBufer(buf.Count);
            }
        }
        /////////////////////////////////////////////////////////////////////////////////////////
        /// действия
        void draw_pen(int x0, int y0, int x1, int y1, int x2, int y2)
        {
            Point[] point = new Point[3];
            point[0] = new Point(x0, y0);
            point[1] = new Point(x1, y1);
            point[2] = new Point(x2, y2);
            Graphics graph = Graphics.FromImage(bmp);
            Pen pen = new Pen(col, pen_size);
            graph.DrawLine(pen, point[0], point[1]);
            graph.DrawEllipse(pen, point[2].X, point[2].Y, pen_size, pen_size );
            pictureBox1.Image = bmp;
        }
        void draw_regtangle(int x1, int y1, int x2, int y2)
        {
            int tmp;
            bmp = (Bitmap)bitmap.Clone();
            Graphics graph = Graphics.FromImage(bmp);
            Pen pen = new Pen(col, pen_size);
            if (x2 < x1)
            {
                tmp = x1;
                x1 = x2;
                x2 = tmp;
                x2 -= x1;
            }
            else
            {
                x2 -= x1;
            }
            if (y2 < y1)
            {
                tmp = y1;
                y1 = y2;
                y2 = tmp;
                y2 -= y1;
            }
            else
            {
                y2 -= y1;
            }
            graph.DrawRectangle(pen, x1, y1, x2, y2);
            pictureBox1.Image = bmp;
        }
        void draw_ellipse(int x1, int y1, int x2, int y2)
        {
            int tmp;
            bmp = (Bitmap)bitmap.Clone();
            Graphics graph = Graphics.FromImage(bmp);
            Pen pen = new Pen(col, pen_size);
            if (x2 < x1)
            {
                tmp = x1;
                x1 = x2;
                x2 = tmp;
                x2 -= x1;
            }
            else
            {
                x2 -= x1;
            }
            if (y2 < y1)
            {
                tmp = y1;
                y1 = y2;
                y2 = tmp;
                y2 -= y1;
            }
            else
            {
                y2 -= y1;
            }
            graph.DrawEllipse(pen, x1, y1, x2, y2);
            pictureBox1.Image = bmp;
        }
        void draw_polygon(int x1, int y1, int x2, int y2)
        {
            bmp = (Bitmap)bitmap.Clone();
            Graphics graph = Graphics.FromImage(bmp);
            Pen pen = new Pen(col, pen_size);

            if (x2 < x1)
            {
                int tmp = x1;
                x1 = x2;
                x2 = tmp;
            }

            if (y2 < y1)
            {
                int tmp = y1;
                y1 = y2;
                y2 = tmp;
            }

            int x3 = x1 + (x2 - x1) / 2; 
            int y3 = y1 - (y2 - y1); 

            Point[] trianglePoints = new Point[]
            {
            new Point(x1, y2),
            new Point(x2, y2), 
            new Point(x3, y3) 
            };

            graph.DrawPolygon(pen, trianglePoints);

            pictureBox1.Image = bmp;
        }
        void draw_text(string text, int x, int y)
        {
            bmp = (Bitmap)bitmap.Clone(); 
            Graphics graph = Graphics.FromImage(bmp);
            Brush brush = new SolidBrush(col);
            graph.DrawString(text, textBox3.Font, brush, x, y); 
            pictureBox1.Image = bmp; 
        }
        private Bitmap filling(Bitmap sourceImage, int x, int y, Color fillColor)
        {
            Bitmap image = (Bitmap)sourceImage.Clone();
            Stack<Point> points = new Stack<Point>();
            points.Push(new Point(x, y));

            if (x < 0 || x >= image.Width || y < 0 || y >= image.Height)
                return image;

            Color targetColor = image.GetPixel(x, y);

            if (targetColor.ToArgb() == fillColor.ToArgb())
                return image;

            while (points.Count > 0)
            {
                Point currentPoint = points.Pop();

                int px = currentPoint.X;
                int py = currentPoint.Y;

                if (px < 0 || px >= image.Width || py < 0 || py >= image.Height)
                    continue;

                Color currentColor = image.GetPixel(px, py);

                if (currentColor.ToArgb() != targetColor.ToArgb())
                    continue;

                image.SetPixel(px, py, fillColor);

                // Добавляем соседние точки в стек
                points.Push(new Point(px, py + 1)); 
                points.Push(new Point(px + 1, py)); 
                points.Push(new Point(px, py - 1)); 
                points.Push(new Point(px - 1, py)); 
            }

            return image;
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// работа с файлами
        public void open()
        {
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
            if (open_dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    full_name_of_image = open_dialog.FileName;
                    image = new Bitmap(open_dialog.FileName);
                    pictureBox1.Image = image;
                    pictureBox1.Invalidate(); 
                    pixel = new UInt32[image.Height, image.Width];
                    for (int y = 0; y < image.Height; y++)
                        for (int x = 0; x < image.Width; x++)
                            pixel[y, x] = (UInt32)(image.GetPixel(x, y).ToArgb());
                    float f_x = image.Width / (float)pictureBox1.Width;
                    float f_y = image.Height / (float)pictureBox1.Height;

                }
                catch
                {
                    full_name_of_image = "\0";
                    DialogResult rezult = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                pictureBox1.Image = (Bitmap)image.Clone();
                buf.Push((Bitmap)pictureBox1.Image.Clone());
                ToBufer(buf.Count);

            }
        }
        public void save()
        {
            if (pictureBox1.Image != null)
            {
                SaveFileDialog save_as= new SaveFileDialog();
                save_as.Title = "Сохранить как...";
                save_as.Filter= "BMP Files (*.bmp)|*.bmp|" +
                 "JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                 "GIF Files (*.gif)|*.gif|" +
                 "PNG Files (*.png)|*.png|" +
                 "All Files (*.*)|*.*";
                save_as.ShowHelp = true;
                if (save_as.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBox1.Image.Save(save_as.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }
        }
        public void save_new()
        {
            if (pictureBox1.Image != null)
            {
                try
                {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    string filePath = Path.Combine(desktopPath, "SavedImage.png");

                    pictureBox1.Image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                    MessageBox.Show("Изображение сохранено на рабочий стол: " + filePath,
                                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Невозможно сохранить изображение: " + ex.Message,
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Нет изображения для сохранения.",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        bool inPoint(MouseEventArgs e, Point p) 
        {
            if (Math.Abs(e.X - p.X) < 4 && Math.Abs(e.Y - p.Y) < 4)
                return true;
            else
            {   
                return false;
            }
        }
        public void ToBufer(int i)
        {
            PictureBox pic = new PictureBox();
            pic.Location = new Point(10, (i - 1) * 66 - 65);
            pic.Width = 65;
            pic.Height = 42;
            pic.Image = resizeImage((Bitmap)buf.Peek().Clone(), new Size(65, 42));
            panel1.VerticalScroll.Value = 0;
            panel1.Controls.Add(pic);

        }

        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        public void FromBitmapToScreen()
        {
            pictureBox1.Image = image;
            bmp = (Bitmap)image.Clone();
            buf.Push((Bitmap)pictureBox1.Image.Clone());
            ToBufer(buf.Count);
        }

        public static void FromOnePixelToBitmap(int x, int y, UInt32 pixel)
        {
            image.SetPixel(y, x, Color.FromArgb((int)pixel));
        }

        private ToolTip toolTip1;


        private void Form1_Load(object sender, EventArgs e)
        {
            toolTip1 = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                ShowAlways = true
            };

            toolTip1.SetToolTip(button3, "Рисование");
            toolTip1.SetToolTip(button4, "Нарисовать четырехугольник");
            toolTip1.SetToolTip(button5, "Нарисовать окружность");
            toolTip1.SetToolTip(button9, "Нарисовать треугольник");
            toolTip1.SetToolTip(button8, "Заливка");
            toolTip1.SetToolTip(button11, "Применить");
        }

    }

}
