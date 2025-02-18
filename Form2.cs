﻿using System;
using System.Windows.Forms;

namespace My_paint
{
    public partial class Form2 : Form 
    { 

         Form1 OwnerForm;
        
        public Form2(Form1 ownerForm)
        {
            this.OwnerForm = ownerForm;
            InitializeComponent();
            this.button1.Click += new System.EventHandler(this.button_Click);
            this.button2.Click += new System.EventHandler(this.button_Click);
            this.button3.Click += new System.EventHandler(this.button_Click);
            this.FormClosing += new FormClosingEventHandler(Form2Closing);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

            if (Form1.full_name_of_image != "\0")
            {
                UInt32 p;
                for (int i = 0; i < Form1.image.Height; i++)
                    for (int j = 0; j < Form1.image.Width; j++)
                    {
                        p = ColorBalance.ColorBalance_R(Form1.pixel[i, j], trackBar1.Value, trackBar1.Maximum);
                        Form1.FromOnePixelToBitmap(i, j, p);
                    }

                FromBitmapToScreen();
            }
        }

        //сохранение изменений яркости или контрастности
        private void button_Click(object sender, EventArgs e)
        {
            if (Form1.full_name_of_image != "\0")
            {
                for (int i = 0; i < Form1.image.Height; i++)
                    for (int j = 0; j < Form1.image.Width; j++)
                        Form1.pixel[i, j] = (UInt32)(Form1.image.GetPixel(j, i).ToArgb());
                trackBar1.Value = 0;
                trackBar2.Value = 0;
                trackBar3.Value = 0;
            }
        }

        private void Form2Closing(object sender, System.EventArgs e)
        {
            if (Form1.full_name_of_image != "\0")
            {
                Form1.FromPixelToBitmap();
                FromBitmapToScreen();
            }
        }

        void FromBitmapToScreen()
        {
            OwnerForm.FromBitmapToScreen();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if (Form1.full_name_of_image != "\0")
            {
                UInt32 p;
                for (int i = 0; i < Form1.image.Height; i++)
                    for (int j = 0; j < Form1.image.Width; j++)
                    {
                        p = ColorBalance.ColorBalance_G(Form1.pixel[i, j], trackBar2.Value, trackBar2.Maximum);
                        Form1.FromOnePixelToBitmap(i, j, p);
                    }

                FromBitmapToScreen();
            }
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {

            if (Form1.full_name_of_image != "\0")
            {
                UInt32 p;
                for (int i = 0; i < Form1.image.Height; i++)
                    for (int j = 0; j < Form1.image.Width; j++)
                    {
                        p = ColorBalance.ColorBalance_B(Form1.pixel[i, j], trackBar3.Value, trackBar3.Maximum);
                        Form1.FromOnePixelToBitmap(i, j, p);
                    }

                FromBitmapToScreen();
            }
        }

    }
    
}
