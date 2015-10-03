/*
WorkTimer
Autor: Marcin Bryk

Wiadomość okresowa
Agreguje dane związane w wyświetlanymi okresowo komunikatami
*/

namespace WorkTimer
{
    public class PeriodicMessage
    {
        public enum MessageType { NORMAL, URGENT };     // Typ wiadomości: zwykła/pilna

        public int Period { get; private set; }         // Okres powtarzania (w minutach)
        public bool Repeat { get; private set; }        // Czy wiadomość ma być powtarzana
        public string Text { get; private set; }        // Tekst komunikatu
        public MessageType Type { get; private set; }   // Typ wiadomości
        public int Count { get; set; }                  // Teoretyczna liczba wyświetleń komunikatu związanego z wiadomością

        public PeriodicMessage(int period, bool repeat, string text, MessageType type)
        {
            Period = period;
            Repeat = repeat;
            Text = text;
            Type = type;
            Count = 0;
        }
    }
}
