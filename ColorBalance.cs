using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_paint
{
    internal class ColorBalance
    {
        public static UInt32 ColorBalance_R(UInt32 point, int poz, int lenght)
        {
            int R;
            int G;
            int B;

            int N = (100 / lenght) * poz; //кол-во процентов

            R = (int)(((point & 0x00FF0000) >> 16) + N * 128 / 100);
            G = (int)((point & 0x0000FF00) >> 8);
            B = (int)(point & 0x000000FF);

            if (R < 0) R = 0;
            if (R > 255) R = 255;

            point = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

            return point;
        }

        public static UInt32 ColorBalance_G(UInt32 point, int poz, int lenght)
        {
            int R;
            int G;
            int B;

            int N = (100 / lenght) * poz; 

            R = (int)((point & 0x00FF0000) >> 16);
            G = (int)(((point & 0x0000FF00) >> 8) + N * 128 / 100);
            B = (int)(point & 0x000000FF);

            if (G < 0) G = 0;
            if (G > 255) G = 255;

            point = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

            return point;
        }

        public static UInt32 ColorBalance_B(UInt32 point, int poz, int lenght)
        {
            int R;
            int G;
            int B;

            int N = (100 / lenght) * poz; 

            R = (int)((point & 0x00FF0000) >> 16);
            G = (int)((point & 0x0000FF00) >> 8);
            B = (int)((point & 0x000000FF) + N * 128 / 100);

            if (B < 0) B = 0;
            if (B > 255) B = 255;

            point = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

            return point;
        }

    }
}

