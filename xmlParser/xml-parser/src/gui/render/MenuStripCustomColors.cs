using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xml_parser
{
    internal class MenuStripCustomColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected => ColorScheme.colorScheme.Hover;
        public override Color MenuItemBorder => ColorScheme.colorScheme.Secondary;
        public override Color MenuItemSelectedGradientBegin => ColorScheme.colorScheme.Hover;
        public override Color MenuItemSelectedGradientEnd => ColorScheme.colorScheme.Hover;
        public override Color MenuItemPressedGradientBegin => ColorScheme.colorScheme.Hover;
        public override Color MenuItemPressedGradientEnd => ColorScheme.colorScheme.Hover;
    }
}
