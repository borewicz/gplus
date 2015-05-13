using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace gPlus.Classes
{
    public enum CommandType
    {
        CIRCLE = 0,
        SQUARE = 1,
        USER = 2
    }

    public class Command
    {
        public string title { get; set; }
        public string arg { get; set; }
        public CommandType type { get; set; }

        public Command(string title, string arg, CommandType type)
        {
            this.title = title;
            this.arg = arg;
            this.type = type;
        }
    }

    //public static class Commands
    //{
    //    private ObservableCollection<Command> _commands = new ObservableCollection<Command>();
    //    public ObservableCollection<Command> commands { get { return _commands; } }
    //}
}
