namespace FacerForm
{
    public class Pixel
    {
        public MyColor newColor;
        public MyColor oldColor;
        public int x;
        public int y;

        public Pixel(MyColor newColor, MyColor oldColor, int x, int y)
        {
            this.newColor = newColor;
            this.oldColor = oldColor;
            this.x = x;
            this.y = y;
        }

        public Pixel()
        {
        }
    }
}