using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Application.DTOs
{
    public class EmpresaNovedadDto
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int AgrupacionId { get; set; }
        public bool Visible { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public AgrupacionBasicDto? Agrupacion { get; set; }
    }

    public class AgrupacionBasicDto
    {
        public int Id { get; set; }
        public int Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activa { get; set; }
        public int Tipo { get; set; }
        public int EmpresaPrincipalId { get; set; }
    }

    public class AgrupacionWithNovedadStatusDto
    {
        public int Id { get; set; }
        public int Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activa { get; set; }
        public bool IsNovedad { get; set; }
        public int EmpresaPrincipalId { get; set; }
    }

    public class SetNovedadesRequestDto
    {
        public List<int> AgrupacionIds { get; set; } = new List<int>();
    }
}