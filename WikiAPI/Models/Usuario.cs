using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiAPI.Models
{
    public class Usuario
    {
        public int id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string senhaHash { get; set; }
        public string Perfil { get; set; }
        // Se é leitor ou editor.
    }
}