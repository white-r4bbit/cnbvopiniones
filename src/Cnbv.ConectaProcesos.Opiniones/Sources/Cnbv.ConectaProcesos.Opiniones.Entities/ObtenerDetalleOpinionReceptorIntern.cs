﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cnbv.ConectaProcesos.Opiniones.Entities
{
    public class ObtenerDetalleOpinionReceptorIntern
    {
        public string Comentarios { get; set; }

        public DateTime? FechaRespuesta { get; set; }

        public List<ArchivosModel> Archivos {get; set;}


    }
}
