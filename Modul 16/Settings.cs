using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Serialization;
using System.Xml;

namespace FManagerApp
{
    [DataContract(Name = "Settings")]
    public class Settings
    {
        [DataMember(Name = "FontSize")]
        public short FontSize
        { get;  private set; }

        [DataMember(Name = "FontColor")]
        public Color FontColor
        { get; private set; }

        public Settings() { }

        public Settings(short fontSize, Color fontColor)
        {
            FontSize = fontSize;
            FontColor = fontColor;
        }
    }
}
