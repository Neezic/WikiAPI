using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string senhaHash { get; set; }
        public required string Perfil { get; set; }
        // Se é leitor ou editor.
    }
}