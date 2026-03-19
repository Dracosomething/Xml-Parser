namespace XmlParser.src.gui.render
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
