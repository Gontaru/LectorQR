using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LectorQR
{
    public class MaquinaLinea
    {
        internal static VentanaTeclados numberpad2;

        public static bool ModoTeclado { get; internal set; }
        public static bool Password { get; internal set; }
        public static bool StatusTeclado { get; internal set; }
        public static string Teclado { get; internal set; }
        public static bool TecladoAbierto { get; internal set; }
        public static int TipoTeclado { get; internal set; }
    }
}
