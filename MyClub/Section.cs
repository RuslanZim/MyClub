using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClub
{
    public class Section
    {
        public int SectionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? TrainerId { get; set; }
        public string Sport { get; set; }
        public string TrainerName { get; set; } // для отображения ФИО тренера
        public byte[] TrainerPhoto { get; set; }
    }
}
