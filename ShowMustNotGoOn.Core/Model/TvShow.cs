﻿namespace ShowMustNotGoOn.Core.Model
{
    public sealed class TvShow
    {
        public int Id { get; set; }
        public int MyShowsId { get; set; }
        public string Title { get; set; }
        public string TitleOriginal { get; set; }
        public string Description { get; set; }
        public int TotalSeasons { get; set; }
        public string Status { get; set; }
        public string Image { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
