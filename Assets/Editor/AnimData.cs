using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

[XmlRoot("AnimData")]
public class AnimData
{
    [XmlElement("ShadowSize")]
    public int ShadowSize { get; set; }

    [XmlArray("Anims")]
    [XmlArrayItem("Anim")]
    public List<AnimInfo> Anims { get; set; }
}

public class AnimInfo
{
    public int Index { get; set; }

    public string Name { get; set; }
}

public class AnimCopyOf : AnimInfo
{
    public string CopyOf { get; set; }
}

public class Anim : AnimInfo
{
    public int FrameWidth { get; set; }

    public int FrameHeight { get; set; }

    public int? RushFrame { get; set; }

    public int? HitFrame { get; set; }

    public int? ReturnFrame { get; set; }

    public List<int> Durations { get; set; }
}