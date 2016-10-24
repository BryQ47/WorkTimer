namespace WorkTimer.Models
{
    class Alert
    {
        public enum MessageType { NORMAL, URGENT };     // Message type

        public int Period { get; private set; }         // Repeat time
        public bool Repeat { get; private set; }        // Sets if message will be displayed more than once
        public string Text { get; private set; }        // Message text
        public MessageType Type { get; private set; }   // Type

        public Alert(int period, bool repeat, string text, MessageType type)
        {
            Period = period;
            Repeat = repeat;
            Text = text;
            Type = type;
        }
    }
}
