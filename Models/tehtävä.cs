﻿


namespace Kipa_plus.Models
{
    public class Tehtävä
    {
        public int Id { get; set; }
        public int SarjaId { get; set; }
        public int KisaId { get; set; }
        public int RastiId { get; set; }
        public string Nimi { get; set; }
        public bool Tarkistettu { get; set; } //ota ehk pois
        public string? TehtavaJson { get; set; }

    }
}
