using PowerArgs;
using System;

namespace Automatica.Core.CLI.Args
{
    internal class WorkingDirectoryArguments
    {
        private string _workingDirectory;

        public WorkingDirectoryArguments()
        {
            WorkingDirectory = Environment.CurrentDirectory;
        }

        [ArgDescription("The working directory, if not set the current working directory will be used")]
        public string WorkingDirectory {
            get
            {
                if(string.IsNullOrEmpty(_workingDirectory))
                {
                    _workingDirectory = Environment.CurrentDirectory;
                }
                return _workingDirectory;
            }
            set => _workingDirectory = value; }
    }
}
