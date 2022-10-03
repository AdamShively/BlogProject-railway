using System.ComponentModel;

namespace BlogProject.Enums
{
    public enum ModerationReason
    {
        [Description("Hate Speech")]
        HateSpeech,
        [Description("Inappropriate Language")]
        Language,
        [Description("Political")]
        Political,
        [Description("Misinformation")]
        Misinformation,
        [Description("Drugs and/or Alcohol Reference")]
        Drugs,
        [Description("Threatening Speech")]
        Threatening,
        [Description("Shaming")]
        Shaming,
        [Description("Cyber Bullying")]
        Bullying,
        [Description("Sexual Content")]
        Sexual,
        [Description("Unrelated Topic")]
        OffTopic
    }
}
