using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

namespace MedianCut
{
    public class mpix
    {
        public int R; 
        public int G; 
        public int B;
        public int x;
        public int y;

        public mpix(System.Windows.Media.Color c, int x, int y)
        {

            this.R = c.R;
            this.G = c.G;
            this.B = c.B;
            this.x = x;
            this.y = y;
        }
    }

    public class cube
    {
        public List<mpix> my_cube = new List<mpix>();
        public Tuple<int, int> axe_max;

        public cube(List<mpix> my_cube)
        {
            this.my_cube = my_cube;
        }

        public Tuple<int, int> c_max_min_axe(List<mpix> my_list)
        {
            int maxr = 0;
            int maxg = 0;
            int maxb = 0;
            int minr = 255;
            int ming = 255;
            int minb = 255;

            foreach (mpix c in my_list)
            {
                if (maxr < c.R) { maxr = c.R; }
                if (maxg < c.G) { maxg = c.G; }
                if (maxb < c.B) { maxb = c.B; }

                if (minr > c.R) { minr = c.R; }
                if (ming > c.G) { ming = c.G; }
                if (minb > c.B) { minb = c.B; }
            }

            int[] max_min = new int[3];
            max_min[0] = maxr - minr;
            max_min[1] = maxg - ming;
            max_min[2] = maxb - minb;

            List<int> levels = max_min.AsEnumerable().ToList();
            int max_color = levels.Max();

            if (max_min[0] == max_color) { return Tuple.Create(0, max_color); }
            if (max_min[1] == max_color) { return Tuple.Create(1, max_color); }
            if (max_min[2] == max_color) { return Tuple.Create(2, max_color); }
            else
            {
                return Tuple.Create(-1, -1);
            }
        }

        public void order_my_list(List<mpix> color, int axe)
        {
            List<mpix> orderl = new List<mpix>();

            if (axe == 0) { orderl.AddRange(color.OrderBy(o => o.R).ToList()); }
            if (axe == 1) { orderl.AddRange(color.OrderBy(o => o.G).ToList()); }
            if (axe == 2) { orderl.AddRange(color.OrderBy(o => o.B).ToList()); }

            this.my_cube.Clear();
            this.my_cube.AddRange(orderl);

        }

        public Tuple<List<mpix>, List<mpix>> divide_list(List<mpix> main_list)
        {
            List<mpix> l1 = new List<mpix>();
            List<mpix> l2 = new List<mpix>();

            int end1 = (main_list.Count) / 2;
            int start2 = end1;
            int end2 = main_list.Count - start2;

            l1.AddRange(main_list.GetRange(0, end1));
            l2.AddRange(main_list.GetRange(start2, end2));
            return Tuple.Create(l1, l2);

        }

        public List<mpix> mean_of_list(List<mpix> main_list)
        {
            int meanr = 0;
            int meang = 0;
            int meanb = 0;

            foreach (mpix c in main_list)
            {
                meanr = meanr + c.R;
                meang = meang + c.G;
                meanb = meanb + c.B;
            }

            foreach (mpix c in main_list)
            {
                c.R = meanr / main_list.Count;
                c.G = meang / main_list.Count;
                c.B = meanb / main_list.Count;
            }

            return main_list;
        }

        public Tuple<List<mpix>, List<mpix>> run_mc()
        {
            this.axe_max = c_max_min_axe(this.my_cube);

            this.order_my_list(this.my_cube, this.axe_max.Item1);

            List<mpix> l1 = new List<mpix>();
            List<mpix> l2 = new List<mpix>();

            l1.AddRange(this.divide_list(this.my_cube).Item1);
            l2.AddRange(this.divide_list(this.my_cube).Item2);

            return Tuple.Create(l1, l2);
        }
    }


    public class mc
    {
        public BitmapSource img; 
        public int nb_color;
        public List<cube> list_of_cube = new List<cube>();
        public int nb_of_distanc_val;

        public mc(BitmapSource img,int nb_colors) { this.img = img; this.nb_color = nb_colors; }

        public void create_main_list()
        { 
            List<mpix> img_data = new List<mpix>();

            for (int x = 0; x < this.img.Width; x++)
            {
                for (int y = 0; y < this.img.Height; y++)
                {
                    int bytesPerPixel = (img.Format.BitsPerPixel + 7) / 8;
                    int stride = bytesPerPixel * img.PixelWidth;
                    byte[] pixelData = new byte[bytesPerPixel];

                    img.CopyPixels(new Int32Rect(x, y, 1, 1), pixelData, stride, 0);

                    byte red = pixelData[2];
                    byte green = pixelData[1];
                    byte blue = pixelData[0];

                    img_data.Add(new mpix(System.Windows.Media.Color.FromRgb(red, green, blue), x, y));
                }
            }

            this.list_of_cube.Add(new cube(img_data));

            this.nb_of_distanc_val = (from x in img_data select x).Distinct().Count();
        }

        public BitmapSource median_cut()
        {
            WriteableBitmap median_cut_img = new WriteableBitmap(this.img.PixelWidth, this.img.PixelHeight, this.img.DpiX, this.img.DpiY, this.img.Format, this.img.Palette);

            for (int i = 0; i < nb_color; i++)
            {
                if (this.list_of_cube.Count >= nb_color) { break; }

                List<cube> new_list_of_cube = new List<cube>();

                foreach (cube c in this.list_of_cube)
                {
                    Tuple<List<mpix>, List<mpix>> sub_lit = c.run_mc();
                    new_list_of_cube.Add(new cube(sub_lit.Item1));
                    new_list_of_cube.Add(new cube(sub_lit.Item2));
                }

                this.list_of_cube.Clear();
                this.list_of_cube.AddRange(new_list_of_cube);
            }

            foreach (cube c in list_of_cube)
            {
                c.my_cube = c.mean_of_list(c.my_cube);
                foreach (mpix pix in c.my_cube)
                {
                    int stride = (median_cut_img.PixelWidth * median_cut_img.Format.BitsPerPixel + 7) / 8;

                    int offset = stride - (median_cut_img.PixelWidth * 4);

                    median_cut_img.WritePixels(new Int32Rect(pix.x, pix.y, 1, 1), new byte[] { (byte)pix.B, (byte)pix.G, (byte)pix.R, 255 }, 4, 0);
                }
            }

            this.list_of_cube.Clear();

            return median_cut_img;
        }
    }
}
