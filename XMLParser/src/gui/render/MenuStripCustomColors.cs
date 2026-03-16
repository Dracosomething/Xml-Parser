using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XmlParser.src;

namespace XmlParser.src.gui.render
{
    internal class MenuStripCustomColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Utils.colorScheme.Hover;
        public override Color MenuItemBorder => Utils.colorScheme.Secondary;
        public override Color MenuItemSelectedGradientBegin => Utils.colorScheme.Hover;
        public override Color MenuItemSelectedGradientEnd => Utils.colorScheme.Hover;
        public override Color MenuItemPressedGradientBegin => Utils.colorScheme.Hover;
        public override Color MenuItemPressedGradientEnd => Utils.colorScheme.Hover;
    }
}
