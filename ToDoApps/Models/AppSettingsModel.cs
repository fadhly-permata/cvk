namespace ToDoApps.Models
{
    public class AppSettingsModel
    {
        public Constring Constring { get; set; }
        public string JWTSecret { get; set; }
        public int TokenExpiresInMinutes { get; set; }
    }

    public class Constring
    {
        public string Main { get; set; }
    }
}