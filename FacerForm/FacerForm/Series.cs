namespace FacerForm
{
    public class Series
    {
        public string type;
        public Photo[] photos;

        public Series(string type, Photo[] photos)
        {
            this.type = type;
            this.photos = photos;
        }

        public Series()
        {
        }
    }
}