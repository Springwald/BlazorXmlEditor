using de.springwald.xml.editor.nativeplatform.gfx;
using System;

namespace de.springwald.xml.editor.editor.xmlelements.StandardNode
{
    public class StandardNodeDimensionsAndColor
    {
        public int AttributeInnerMarginY { get; private set; }
        public int AttributeHeight { get; private set; }
        public int CornerRadius { get; private set; }
        public int AttributeMarginY { get; private set; }
        public int InnerMarginX { get; private set; }
        public Color BackgroundColor { get; }

        private EditorConfig config;
        private int lastNodeNameHeight = 0;
        private int lastAttributeHeight = 0;
        

        public StandardNodeDimensionsAndColor(EditorConfig config, Color backgroundColor)
        {
            this.config = config;
            this.BackgroundColor = backgroundColor;
        }

        public void Update()
        {
            if (this.config.FontNodeName.Height == this.lastNodeNameHeight && this.config.FontNodeAttribute.Height == this.lastAttributeHeight) return;

            this.lastNodeNameHeight = this.config.FontNodeName.Height;
            this.lastAttributeHeight = this.config.FontNodeAttribute.Height;

            this.AttributeInnerMarginY = Math.Max(1, (this.config.FontNodeName.Height - this.config.FontNodeAttribute.Height) / 2);
            this.AttributeHeight = this.config.FontNodeAttribute.Height + AttributeInnerMarginY * 2;
            this.CornerRadius = (this.config.TagHeight - this.AttributeHeight - AttributeInnerMarginY) / 2;
            this.AttributeMarginY = (this.config.TagHeight - this.AttributeHeight - AttributeInnerMarginY) / 2;
            this.InnerMarginX = this.config.FontNodeName.Height / 2;

           
        }


    }
}
