﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class ObtenerDetalleOpinion
    {
        public int Id { get; set; }

        public string Comentarios { get; set; }

        public ArchivosModel[] Archivos { get; set; }

        public ObtenerDetalleOpinionReceptor Receptor { get; set; }
    }
}
