namespace TK158.Model
{
    class SettingsModel
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public byte Frequency { get; set; }
        public byte BitsCount { get; set; }
        public byte WordLength { get; set; }
        public bool IsInfiniteFT { get; set; }
        public bool IsInfiniteSending { get; set; }
    }

    public enum Frequency
    {
        _128kHz = 0x3F,
        _256kHz = 0x1F,
        _512kHz = 0x0F,
        _1024kHz = 0x07, 
    }

    public enum WordLength
    {
        _16Bit = 0x10,
        _32Bit = 0x20,
    }

    public enum CheckMode
    {
        PassingInformation = 0,
        OutputInformation = 1,
        OtherTests = 2,
        Test = 3,
    }
}
