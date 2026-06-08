namespace IaAgent.Application.DTOs;

public class PipelineOptions
{
    public List<string> CategoryIds { get; set; } = new()
    {
        "MLC1000",  // Electrónica, Audio y Video
        "MLC1051",  // Computación
        "MLC1071",  // Teléfonos y Telefonía
        "MLC1039",  // Cámaras y Accesorios
        "MLC1276",  // Hogar, Muebles y Jardín
        "MLC1168",  // Deportes y Fitness
        "MLC1246",  // Belleza y Cuidado Personal
        "MLC1132",  // Ropa y Accesorios
        "MLC1367",  // Juegos y Juguetes
        "MLC3025",  // Bebés
        "MLC1648",  // Herramientas
        "MLC1514",  // Accesorios para Vehículos
        "MLC1612",  // Mascotas
        "MLC1182"   // Industrias y Oficinas
    };
    public int MaxOpportunitiesPerRun { get; set; } = 10;
    public decimal MinNetMarginPct { get; set; } = 25m;
    public int RunIntervalHours { get; set; } = 6;
}
