using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ScreenLampWPF.Helper
{
    public class ColorPalette
    {
        static private BrushConverter bc = new BrushConverter();

        public Brush OK { get; } = (Brush)bc.ConvertFrom("#74DA72");
        public Brush EDITED { get; } = (Brush)bc.ConvertFrom("#DA9E72");
        public Brush BASIC { get; } = (Brush)bc.ConvertFrom("#FFFFFF");
        public Brush FALSE { get; } = (Brush)bc.ConvertFrom("#DA7272");
    }
}
