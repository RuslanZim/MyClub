using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClub
{
    public class Event
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Location { get; set; }
        public int? ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
        public int? SectionId { get; set; }
        public string SectionName { get; set; }  // если нужно фильтровать по секции
        public string Sport { get; set; }  // если нужно фильтровать по виду спорта


    }
}