namespace OpenSvip.GUI
{
    public class ToastActionButton
    {
        public string Content { get; set; } = "Button";
        
        public string Action { get; set; } = "Action";

        public ToastActionButton()
        {
            
        }

        public ToastActionButton(string content, string action)
        {
            this.Content = content;
            this.Action = action;
        }
    }
}