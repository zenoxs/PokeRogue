using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public static class AnimDataDeserializer
{
    public static AnimData Deserialize(string xmlContent)
    {
        // Deserialize the root element
        AnimData animData = DeserializeRoot(xmlContent);

        // Extract the Anim elements
        animData.Anims = DeserializeAnims(xmlContent);

        return animData;
    }

    private static AnimData DeserializeRoot(string xmlContent)
    {
        // Deserialize the AnimData root excluding the Anims list
        XmlSerializer rootSerializer = new XmlSerializer(typeof(AnimData), new XmlRootAttribute("AnimData"));
        using StringReader stringReader = new StringReader(xmlContent);
        return (AnimData)rootSerializer.Deserialize(stringReader);
    }

    private static List<AnimInfo> DeserializeAnims(string xmlContent)
    {
        List<AnimInfo> anims = new List<AnimInfo>();

        // Use XmlReader to manually parse the Anims
        using StringReader stringReader = new StringReader(xmlContent);
        using XmlReader xmlReader = XmlReader.Create(stringReader);

        xmlReader.MoveToContent(); // Move to the root element
        if (xmlReader.ReadToDescendant("Anims"))
        {
            if (xmlReader.ReadToDescendant("Anim"))
            {
                do
                {
                    anims.Add(DeserializeAnim(xmlReader.ReadSubtree()));
                }
                while (xmlReader.ReadToNextSibling("Anim"));
            }
        }

        return anims;
    }

    private static AnimInfo DeserializeAnim(XmlReader xmlReader)
    {
        // Move the reader to the content node
        xmlReader.MoveToContent();

        var serializer = new XmlSerializer(typeof(AnimInfoTemp));
        var temp = (AnimInfoTemp)serializer.Deserialize(xmlReader);

        return temp.ToAnimInfo();
    }


    [XmlRoot("Anim")]
    public class AnimInfoTemp
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Index")]
        public int Index { get; set; }

        [XmlElement("CopyOf", IsNullable = true)]
        public string CopyOf { get; set; }

        [XmlElement("FrameWidth", IsNullable = false)]
        public int FrameWidth { get; set; }

        [XmlElement("FrameHeight", IsNullable = false)]
        public int FrameHeight { get; set; }

        [XmlElement("RushFrame", IsNullable = true)]
        public int? RushFrame { get; set; }

        [XmlElement("HitFrame", IsNullable = true)]
        public int? HitFrame { get; set; }

        [XmlElement("ReturnFrame", IsNullable = true)]
        public int? ReturnFrame { get; set; }

        [XmlArray("Durations")]
        [XmlArrayItem("Duration")]
        public List<int> Durations { get; set; }

        public AnimInfo ToAnimInfo()
        {
            if (CopyOf != null)
            {
                return new AnimCopyOf()
                {
                    Name = Name,
                    Index = Index,
                    CopyOf = CopyOf,
                };
            }


            return new Anim()
            {
                Name = Name,
                Index = Index,
                FrameWidth = FrameWidth,
                FrameHeight = FrameHeight,
                RushFrame = RushFrame,
                HitFrame = HitFrame,
                ReturnFrame = ReturnFrame,
                Durations = Durations
            };
        }
    }
}