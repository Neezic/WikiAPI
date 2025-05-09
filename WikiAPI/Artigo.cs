using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiAPI
{
    public class Artigo
    {
        public int Id { get; set;}
        public string Titulo {get; set; }
        public string Conteudo {get; set;}
        public DateTime DataCriacao {get; set;} = DateTime.UtcNow;
        public DateTime? DataAtualizacao {get; set; }

    }
}