﻿namespace Kipa_plus.Models
{
    public class Vartio
    {
        public int? Id { get; set; }
        public string Nimi { get; set; }
        public int? Numero { get; set; }
        public int? SarjaId { get; set; }
        public int? KisaId { get; set; }
        public string Lippukunta { get; set; }
        public int tilanne { get; set; } //0 = kisassa, 1 = Keskeytetty, 2 = ulkopuolella
    }
}
