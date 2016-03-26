using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace FacerForm
{
    public class Photo
    {
        public Photo(string name, int width,int height, float changed, Pixel[] pixels)
        {
            this.name = name;
            this.width = height;
            this.changed = changed;
            this.pixels = pixels;
        }

        public string name;
        public int width;
        public int height;
        public float changed;
        public Pixel[] pixels;

        public Photo()
        {
        }
    }
}