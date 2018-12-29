namespace DataModels
{
    [System.Serializable]
    public class BeatmapInfo
    {
        public string SongName { get; set; }
        public string SongKana { get; set; }
        public string ArtistName { get; set; }
        public string ArtistKana { get; set; }
        public string BackgroundImage { get; set; }
        public string AudioFile { get; set; }
        public string VideoFile { get; set; }
        public Beatmap[] Difficulties { get; set; } = new Beatmap[2];
    }

}
