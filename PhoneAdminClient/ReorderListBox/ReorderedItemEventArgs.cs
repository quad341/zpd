using System;

namespace PhoneAdminClient.ReorderListBox
{
    public class ReorderedItemEventArgs : EventArgs
    {
        public object ReorderedItem { get; set; }
        public int DestinationIndex { get; set; }
    }
}