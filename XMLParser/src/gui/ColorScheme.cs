using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlParser
{
    internal class ColorScheme
    {
        private bool isDarkMode = true;

        // dark mode colors
        private readonly Color primaryDark = Color.FromArgb(34, 34, 34);
        private readonly Color secondryDark = Color.FromArgb(19, 19, 19);
        private readonly Color accentDark = Color.FromArgb(48, 48, 48);
        private readonly Color transparentDark = Color.FromArgb(25, 85, 85, 85);
        private readonly Color hoverDark = Color.FromArgb(56, 56, 56);

        // light mode colors
        private readonly Color primaryLight = Color.FromArgb(221, 221, 221);
        private readonly Color secondaryLight = Color.FromArgb(236, 236, 236);
        private readonly Color accentLight = Color.FromArgb(217, 217, 217);
        private readonly Color transparentLight = Color.FromArgb(25, 200, 200, 200);
        private readonly Color hoverLight = Color.FromArgb(244, 244, 244);

        // current set colors
        public Color Primary { get { return isDarkMode ? primaryDark : primaryLight; } }
        public Color Secondary { get { return isDarkMode ? secondryDark : secondaryLight; } }
        public Color Accent { get { return isDarkMode ? accentDark : accentLight; } }
        public Color Transparent { get { return isDarkMode ? transparentDark : transparentLight; } }
        public Color Hover { get { return isDarkMode ? hoverDark : hoverLight; } }
        public Color Text { get { return isDarkMode ? Color.White : Color.Black; } }

        public void ChangeDarkMode() { isDarkMode = !isDarkMode; }
    }
}
