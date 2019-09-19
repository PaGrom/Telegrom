using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbRepository.Entities
{
    public partial class TvShows
    {
        public long Id { get; set; }
        public long MyShowsId { get; set; }
        public string Title { get; set; }
        public string TitleOriginal { get; set; }
        public string Description { get; set; }
        public long? TotalSeasons { get; set; }
        public string Status { get; set; }
    }
}
