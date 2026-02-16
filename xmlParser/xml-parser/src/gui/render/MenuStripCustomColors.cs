using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XmlParser.src;

namespace XmlParser
{
    internal class MenuStripCustomColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Constants.colorScheme.Hover;
        public override Color MenuItemBorder => Constants.colorScheme.Secondary;
        public override Color MenuItemSelectedGradientBegin => Constants.colorScheme.Hover;
        public override Color MenuItemSelectedGradientEnd => Constants.colorScheme.Hover;
        public override Color MenuItemPressedGradientBegin => Constants.colorScheme.Hover;
        public override Color MenuItemPressedGradientEnd => Constants.colorScheme.Hover;
    }
}
