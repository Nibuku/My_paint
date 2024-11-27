using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Image = System.Drawing.Image;

namespace My_paint
{
    


    public partial class Form1 : Form
    {
        class DraggedFragment
        {
            public Rectangle SourceRect;//прямоугольник фрагмента в исходном изображении
            public Point Location;//положение сдвинутого фрагмента

            //прямоугольник сдвинутого фрагмента
            public Rectangle Rect
            {
                get { return new Rectangle(Location, SourceRect.Size); }
            }

            //фиксация изменений в исх изображении
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

        static Color c1 = Color.Crimson, c2 = Color.White, zal;
        Boolean risch, chcol;
        Point oldMouse;
        int x2, y2, x3, y3, x4, y4;
        static Point[] gpoint;
        private Point mousePos1;
        private Point mousePos2;
        private DraggedFragment draggedFragment;
        static Pen cont = new Pen(col, 2);

        static Color col = Color.Black, ncol = Color.Black;
        static Pen pen = new Pen(col, 1);
        int pen_size = 1;
        int x1, y1;
        static Bitmap nbmp, bitmap, bmp, colbitmap;
        int intr;
        static Stack<Bitmap> buf = new Stack<Bitmap>();
        static Stack<Bitmap> buf2 = new Stack<Bitmap>();

        Point[] resize_point = new Point[8];
        bool[] resize=new bool[8];
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;

            Bitmap btm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = btm;
            buf.Push(btm);
            intr = 1;
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

        private void сщхранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            open();
        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// буффер 
        private void button1_Click(object sender, EventArgs e)
        {
            if (buf.Count >1)
            {
                buf2.Push(buf.Pop());
                pictureBox1.Image = (Bitmap)buf.Pop().Clone();
                panel1.Controls.Remove(panel1.Controls[panel1.Controls.Count - 1]);
            }
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            pen_size=trackBar6.Value;
        }


        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            col = Color.FromArgb(col.A, col.R, col.G, trackBar3.Value);
            label7.BackColor = col;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (buf2.Count >0)
            {
                buf.Push(buf2.Pop());
                pictureBox1.Image= (Bitmap)buf.Pop().Clone();
                ToBufer(buf.Count);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        /// палитра
        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            chcol = true;
            colbitmap = new Bitmap(pictureBox2.Image);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            intr= 1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            intr = 2;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            intr = 3;
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            chcol = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            intr = 4;
        }

        private void panel2_Resize(object sender, EventArgs e)
        {
            pictureBox1.Left = (panel2.Width - pictureBox1.Width) / 2;
            pictureBox1.Top = (panel2.Height - pictureBox1.Height) / 2;
            if (intr == 5)
                draw_resize();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            intr = 5;
            draw_resize();
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

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
                pictureBox1.Left = (panel2.Width - pictureBox1.Width) / 2;
                pictureBox1.Top = (panel2.Height - pictureBox1.Height) / 2;
            if ( intr==5)
                draw_resize();

        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {   if (intr == 5)
            {
                    if (inPoint(e, resize_point[0]) || inPoint(e, resize_point[7]))
                    {
                        this.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
                    }
                    else if (inPoint(e, resize_point[2]) || inPoint(e, resize_point[5]))
                    {
                    this.Cursor = System.Windows.Forms.Cursors.SizeNESW;
                    }
                    else if (inPoint(e, resize_point[1]) || inPoint(e, resize_point[6]))
                    {
                    this.Cursor = System.Windows.Forms.Cursors.SizeWE;
                    }
                    else if (inPoint(e, resize_point[3]) || inPoint(e, resize_point[4]))
                    {
                    this.Cursor = System.Windows.Forms.Cursors.SizeNS;
                    resize[4] = true;
                    }
                    else
                    {
                    this.Cursor=System.Windows.Forms.Cursors.Default;
                     }

            }
            if (resize[4] == true)
                holst_resize(e);
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

        private void button8_Click(object sender, EventArgs e)
        {
            intr = 6;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //если есть сдвигаемый фрагмент
            if (draggedFragment != null)
            {
                //рисуем вырезанное белое место
                e.Graphics.SetClip(draggedFragment.SourceRect);
                e.Graphics.Clear(Color.White);

                //рисуем сдвинутый фрагмент
                e.Graphics.SetClip(draggedFragment.Rect);
                e.Graphics.DrawImage(pictureBox1.Image, draggedFragment.Location.X - draggedFragment.SourceRect.X, draggedFragment.Location.Y - draggedFragment.SourceRect.Y);

                //рисуем рамку
                e.Graphics.ResetClip();
                ControlPaint.DrawFocusRectangle(e.Graphics, draggedFragment.Rect);
            }
            else
            {
                //если выделена область
                if (mousePos1 != mousePos2)
                    ControlPaint.DrawFocusRectangle(e.Graphics, GetRect(mousePos1, mousePos2));//рисуем рамку
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
                            //юзер тянет фрагмент?
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
                //юзер кликнул мышью мимо фрагмента?
                if (draggedFragment != null && !draggedFragment.Rect.Contains(e.Location))
                {
                    //уничтожаем фрагмент
                    draggedFragment = null;
                    pictureBox1.Invalidate();
                }
            }

        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (intr == 4)
            {
                //пользователь выделил фрагмент и отпустил мышь?
                if (mousePos1 != mousePos2)
                {
                    //создаем DraggedFragment
                    var rect = GetRect(mousePos1, mousePos2);
                    draggedFragment = new DraggedFragment() { SourceRect = rect, Location = rect.Location };
                }
                else
                {
                    //пользователь сдвинул фрагмент и отпутил мышь?
                    if (draggedFragment != null)
                    {
                        //фиксируем изменения в исходном изображении
                        draggedFragment.Fix(pictureBox1.Image);
                        //уничтожаем фрагмент
                        draggedFragment = null;
                        mousePos1 = mousePos2 = e.Location;
                    }
                }
                pictureBox1.Invalidate();
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

        void select(int x1, int y1, int x2, int y2)
        {
            if (risch)
            {
                int tmp;
                bmp = (Bitmap)bitmap.Clone();
                Graphics graph = Graphics.FromImage(bmp);
                Pen pen = new Pen(Color.Blue, 1);
                pen.DashStyle = DashStyle.Dash;
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
            else
            {
                pictureBox1.Image=bitmap;
            }
        }

        void draw_resize()
        {
            Graphics graph = panel2.CreateGraphics();
            graph.Clear(panel2.BackColor);
            int x1, x2, x3, y1, y2, y3, x, y;
            x=pictureBox1.Location.X-2;
            y = pictureBox1.Location.Y-2;
            x1 = x; 
            y1 = y;
            x2 = pictureBox1.Width / 2+x;
            y2 = pictureBox1.Height/2+y;
            x3 = pictureBox1.Width+x;
            y3 = pictureBox1.Height+y;
            resize_point[0] =new Point(x1, y1);
            resize_point[1] = new Point(x1, y2);
            resize_point[2] = new Point(x1, y3);
            resize_point[3] = new Point(x2, y1);
            resize_point[4] = new Point(x2, y3);
            resize_point[5] = new Point(x3, y1);
            resize_point[6] = new Point(x3, y2);
            resize_point[7] = new Point(x3, y3);

            Pen pen = new Pen(Color.Gray, 3);
            graph.DrawRectangle(pen, x1, y1, 3, 3);
            graph.DrawRectangle(pen, x1, y2, 3, 3);
            graph.DrawRectangle(pen, x1, y3, 3, 3);
            graph.DrawRectangle(pen, x2, y1, 3, 3);
            graph.DrawRectangle(pen, x2, y3, 3, 3);
            graph.DrawRectangle(pen, x3, y1, 3, 3);
            graph.DrawRectangle(pen, x3, y2, 3, 3);
            graph.DrawRectangle(pen, x3, y3, 3, 3);
            //pictureBox1.Image = bmp;
        }

        void holst_resize(MouseEventArgs e)
        {
            pictureBox1.Width=pictureBox1.Width+e.X-pictureBox1.Location.X;
        }

        /////////////////////////////////////////////////////////////////////////////////////
        /// работа с файлами

        public void open()
        { OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Открыть файл";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true; 
            ofd.Filter= "BMP Files (*.bmp)|*.bmp|" +
                 "JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                 "GIF Files (*.gif)|*.gif|" +
                 "PNG Files (*.png)|*.png|" +
                 "All Files (*.*)|*.*";
            ofd.ShowHelp= true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox1.Image = new Bitmap(ofd.FileName);
                }
                catch
                {
                    MessageBox.Show("Невозможно открыть изображение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        bool inPoint(MouseEventArgs e, Point p) 
        {
            if (Math.Abs(e.X - p.X) < 4 && Math.Abs(e.Y - p.Y) < 4)
                return true;
            else
            {   
                return false;
            }
        }

        public Point[] bezier(Point[] point)
        {
            Point[] t = new Point[2];
            t[0]= new Point(0,0);
            t[1]= new Point(0,0);
            t[0].X = (point[0].X + point[1].X) / 2;
            t[0].Y= (point[0].Y + point[1].Y) / 2;
            t[1].X = (point[0].X + point[1].X) / 2;
            t[1].Y = (point[0].Y + point[1].Y) / 2;
            return t;
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
    }


}
