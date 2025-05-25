using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiAPI.Models
{
    public class Artigo
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }

        public int? UsuarioId { get; set; } // Id do usuario que criou/editou
        public Usuario Usuario { get; set; }
        

    }
}