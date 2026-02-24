using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XmlParser.src.gui.render
{
    internal class MenuStripCustomRenderer : ToolStripProfessionalRenderer
    {
        public MenuStripCustomRenderer() : base(new MenuStripCustomColors()) { }
    }
}
