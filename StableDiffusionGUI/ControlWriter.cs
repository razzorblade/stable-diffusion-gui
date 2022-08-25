using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace StableDiffusionGUI
{
    public class ControlWriter : TextWriter
    {
        TextBox _output = null;
        private readonly Dispatcher dispatcher;

        public ControlWriter(TextBox output, System.Windows.Threading.Dispatcher dispatcher)
        {
            _output = output;
            this.dispatcher = dispatcher;
        }

        public override void Write(char value)
        {
            base.Write(value);

            dispatcher.Invoke(() =>
            {
                _output.AppendText(value.ToString()); // When character data is written, append it to the text box.
                _output.ScrollToEnd();
            });
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
