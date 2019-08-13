using Sitecore.Configuration;
using Sitecore.Controls;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XamlSharp.Xaml;
using System;
using System.Web.UI.WebControls;

namespace Sitecore.Shell.Applications.Media.ImageProperties
{
    /// <summary>Represents a GridDesignerPage.</summary>
    public class ImagePropertiesPage : DialogPage
    {
        /// <summary>The alt.</summary>
        protected TextBox Alt;

        /// <summary>The aspect.</summary>
        protected Sitecore.Web.UI.HtmlControls.Checkbox Aspect;

        /// <summary>The h space.</summary>
        protected TextBox HSpace;

        /// <summary>The height edit.</summary>
        protected TextBox HeightEdit;

        /// <summary>The original size.</summary>
        protected Sitecore.Web.UI.HtmlControls.Literal OriginalSize;

        /// <summary>The original text.</summary>
        protected TextBox OriginalText;

        /// <summary>The size warning.</summary>
        protected Border SizeWarning;

        /// <summary>The v space.</summary>
        protected TextBox VSpace;

        /// <summary>The width edit.</summary>
        protected TextBox WidthEdit;

        /// <summary>
        /// Gets or sets the height of the image.
        /// </summary>
        /// <value>The height of the image.</value>
        public int ImageHeight
        {
            get
            {
                return (int)this.ViewState["ImageHeight"];
            }
            set
            {
                this.ViewState["ImageHeight"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of the image.
        /// </summary>
        /// <value>The width of the image.</value>
        public int ImageWidth
        {
            get
            {
                return (int)this.ViewState["ImageWidth"];
            }
            set
            {
                this.ViewState["ImageWidth"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the XML value.
        /// </summary>
        /// <value>The XML value.</value>
        /// <contract>
        ///   <requires name="value" condition="not null" />
        ///   <ensures condition="nullable" />
        /// </contract>
        private XmlValue XmlValue
        {
            get
            {
                return new XmlValue(StringUtil.GetString(this.ViewState["XmlValue"]), "image");
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this.ViewState["XmlValue"] = value.ToString();
            }
        }

        /// <summary>Changes the height.</summary>
        protected void ChangeHeight()
        {
            if (this.ImageHeight == 0)
            {
                return;
            }
            int num = MainUtil.GetInt(this.HeightEdit.Text, 0);
            if (num > 0)
            {
                if (num > 8192)
                {
                    num = 8192;
                    this.HeightEdit.Text = "8192";
                    SheerResponse.SetAttribute(this.HeightEdit.ClientID, "value", this.HeightEdit.Text);
                }
                if (this.Aspect.Checked)
                {

                    this.WidthEdit.Text = ((int)((float)num / (float)this.ImageHeight * (float)this.ImageWidth)).ToString();
                    SheerResponse.SetAttribute(this.WidthEdit.ClientID, "value", this.WidthEdit.Text);
                }
            }
            SheerResponse.SetReturnValue(true);
        }

        /// <summary>Changes the width.</summary>
        protected void ChangeWidth()
        {
            if (this.ImageWidth == 0)
            {
                return;
            }
            int num = MainUtil.GetInt(this.WidthEdit.Text, 0);
            if (num > 0)
            {
                if (num > 8192)
                {
                    num = 8192;
                    this.WidthEdit.Text = "8192";
                    SheerResponse.SetAttribute(this.WidthEdit.ClientID, "value", this.WidthEdit.Text);
                }
                if (this.Aspect.Checked)
                {
                    this.HeightEdit.Text = ((int)((float)num / (float)this.ImageWidth * (float)this.ImageHeight)).ToString();
                    SheerResponse.SetAttribute(this.HeightEdit.ClientID, "value", this.HeightEdit.Text);
                }
            }
            SheerResponse.SetReturnValue(true);
        }

        /// <summary>Handles a click on the OK button.</summary>
        /// <remarks>When the user clicks OK, the dialog is closed by calling
        /// the <see cref="M:Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow">CloseWindow</see> method.</remarks>
        protected override void OK_Click()
        {
            XmlValue expr_06 = this.XmlValue;
            Assert.IsNotNull(expr_06, "XmlValue");
            expr_06.SetAttribute("alt", this.Alt.Text);
            expr_06.SetAttribute("height", this.HeightEdit.Text);
            expr_06.SetAttribute("width", this.WidthEdit.Text);
            expr_06.SetAttribute("hspace", this.HSpace.Text);
            expr_06.SetAttribute("vspace", this.VSpace.Text);
            SheerResponse.SetDialogValue(expr_06.ToString());
            base.OK_Click();
        }

        /// <summary>Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.</summary>
        /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (XamlControl.AjaxScriptManager.IsEvent)
            {
                return;
            }
            this.ImageWidth = 0;
            this.ImageHeight = 0;
            ItemUri itemUri = ItemUri.ParseQueryString();
            if (itemUri == null)
            {
                return;
            }
            Item item = Database.GetItem(itemUri);
            if (item == null)
            {
                return;
            }
            string text = item["Dimensions"];
            if (!string.IsNullOrEmpty(text))
            {
                int num = text.IndexOf('x');
                if (num >= 0)
                {
                    this.ImageWidth = MainUtil.GetInt(StringUtil.Left(text, num).Trim(), 0);
                    this.ImageHeight = MainUtil.GetInt(StringUtil.Mid(text, num + 1).Trim(), 0);
                }
            }
            if (this.ImageWidth <= 0 || this.ImageHeight <= 0)
            {
                this.Aspect.Checked = false;
                this.Aspect.Disabled = true;
            }
            else
            {
                this.Aspect.Checked = true;
            }
            if (this.ImageWidth > 0)
            {
                this.OriginalSize.Text = Translate.Text("Original Dimensions: {0} x {1}", new object[]
                {
                    this.ImageWidth,
                    this.ImageHeight
                });
            }
            if (MainUtil.GetLong(item["Size"], 0L) >= Settings.Media.MaxSizeInMemory)
            {
                this.HeightEdit.Enabled = false;
                this.WidthEdit.Enabled = false;
                this.Aspect.Disabled = true;
            }
            else
            {
                this.SizeWarning.Visible = false;
            }
            this.OriginalText.Text = StringUtil.GetString(new string[]
            {
                item["Alt"],
                Translate.Text("[none]")
            });
            UrlHandle expr_198 = UrlHandle.Get();
            XmlValue xmlValue = new XmlValue(expr_198["xmlvalue"], "image");
            this.XmlValue = xmlValue;
            this.Alt.Text = xmlValue.GetAttribute("alt");
            this.HeightEdit.Text = xmlValue.GetAttribute("height");
            this.WidthEdit.Text = xmlValue.GetAttribute("width");
            this.HSpace.Text = xmlValue.GetAttribute("hspace");
            this.VSpace.Text = xmlValue.GetAttribute("vspace");
            if (MainUtil.GetBool(expr_198["disableheight"], false))
            {
                this.HeightEdit.Enabled = false;
                this.Aspect.Checked = false;
                this.Aspect.Disabled = true;
            }
            if (MainUtil.GetBool(expr_198["disablewidth"], false))
            {
                this.WidthEdit.Enabled = false;
                this.Aspect.Checked = false;
                this.Aspect.Disabled = true;
            }
        }
    }
}