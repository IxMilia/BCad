using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IxMilia.BCad.Collections;
using IxMilia.BCad.Commands;
using IxMilia.BCad.Entities;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Services;

namespace IxMilia.BCad
{
    public abstract class WorkspaceBase : IWorkspace
    {
        private RubberBandGenerator rubberBandGenerator;
        private readonly Regex settingsPattern = new Regex(@"^/p:([a-zA-Z\.]+)=(.*)$");

        public WorkspaceBase()
        {
            Drawing = new Drawing();
            DrawingPlane = new Plane(Point.Origin, Vector.ZAxis);
            ActiveViewPort = ViewPort.CreateDefaultViewPort();
            SelectedEntities = new ObservableHashSet<Entity>();
            ViewControl = null;
            RubberBandGenerator = null;
        }

        #region Events

        public event CommandExecutingEventHandler CommandExecuting;

        protected virtual void OnCommandExecuting(CadCommandExecutingEventArgs e)
        {
            var executing = CommandExecuting;
            if (executing != null)
                executing(this, e);
        }

        public event CommandExecutedEventHandler CommandExecuted;

        protected virtual void OnCommandExecuted(CadCommandExecutedEventArgs e)
        {
            var executed = CommandExecuted;
            if (executed != null)
                executed(this, e);
        }

        public event EventHandler RubberBandGeneratorChanged;

        protected virtual void OnRubberBandGeneratorChanged(EventArgs e)
        {
            var changed = RubberBandGeneratorChanged;
            if (changed != null)
                changed(this, e);
        }

        #endregion

        #region Properties

        public bool IsDirty { get; private set; }

        public Drawing Drawing { get; private set; }

        public Plane DrawingPlane { get; private set; }

        public ViewPort ActiveViewPort { get; private set; }

        public IViewControl ViewControl { get; private set; }

        public ObservableHashSet<Entity> SelectedEntities { get; private set; }

        public RubberBandGenerator RubberBandGenerator
        {
            get { return rubberBandGenerator; }
            set
            {
                if (rubberBandGenerator == value)
                    return;
                rubberBandGenerator = value;
                OnRubberBandGeneratorChanged(new EventArgs());
            }
        }

        public bool IsDrawing { get { return RubberBandGenerator != null; } }

        public bool IsCommandExecuting { get; private set; }

        #endregion

        #region Imports

        [ImportMany]
        public IEnumerable<IWorkspaceService> Services { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<ICadCommand, CadCommandMetadata>> Commands { get; set; }

        #endregion

        #region IWorkspace implementation

        public TService GetService<TService>() where TService : class, IWorkspaceService
        {
            return Services.OfType<TService>().SingleOrDefault();
        }

        // well-known services
        private IDebugService _debugServiceCache;
        public IDebugService DebugService => CacheService<IDebugService>(ref _debugServiceCache);

        private IDialogService _dialogServiceCache;
        public IDialogService DialogService => CacheService<IDialogService>(ref _dialogServiceCache);

        private IInputService _inputServiceCache;
        public IInputService InputService => CacheService<IInputService>(ref _inputServiceCache);

        private IOutputService _outputServiceCache;
        public IOutputService OutputService => CacheService<IOutputService>(ref _outputServiceCache);

        private IReaderWriterService _readerWriterServiceCache;
        public IReaderWriterService ReaderWriterService => CacheService<IReaderWriterService>(ref _readerWriterServiceCache);

        private ISettingsService _settingsService;
        public ISettingsService SettingsService => CacheService<ISettingsService>(ref _settingsService);

        private IUndoRedoService _undoRedoServiceCache;
        public IUndoRedoService UndoRedoService => CacheService<IUndoRedoService>(ref _undoRedoServiceCache);

        private TService CacheService<TService>(ref TService backingStore) where TService : class, IWorkspaceService
        {
            if (backingStore == null)
            {
                backingStore = GetService<TService>();
            }

            return backingStore;
        }

        public async Task Initialize(params string[] args)
        {
            string fileName = null;
            foreach (var arg in args)
            {
                var match = settingsPattern.Match(arg);
                if (match.Success)
                {
                    var settingName = match.Groups[1].Value;
                    var settingValue = match.Groups[2].Value;
                    SettingsService.SetValue(settingName, settingValue);
                }
                else
                {
                    // try to match a file to open
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        throw new NotSupportedException("More than one file specified on the command line.");
                    }
                    else
                    {
                        fileName = arg;
                    }
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                Update(isDirty: false); // don't let the following command prompt for unsaved changes
                await ExecuteCommand("File.Open", fileName);
            }
        }

        public virtual void Update(
            Optional<Drawing> drawing = default(Optional<Drawing>),
            Optional<Plane> drawingPlane = default(Optional<Plane>),
            Optional<ViewPort> activeViewPort = default(Optional<ViewPort>),
            Optional<IViewControl> viewControl = default(Optional<IViewControl>),
            bool isDirty = true)
        {
            var e = new WorkspaceChangeEventArgs(
                drawing.HasValue,
                drawingPlane.HasValue,
                activeViewPort.HasValue,
                viewControl.HasValue,
                this.IsDirty != isDirty);

            OnWorkspaceChanging(e);
            if (drawing.HasValue)
                this.Drawing = drawing.Value;
            if (drawingPlane.HasValue)
                this.DrawingPlane = drawingPlane.Value;
            if (activeViewPort.HasValue)
                this.ActiveViewPort = activeViewPort.Value;
            if (viewControl.HasValue)
                this.ViewControl = viewControl.Value;
            this.IsDirty = isDirty;
            OnWorkspaceChanged(e);
        }

        public event WorkspaceChangingEventHandler WorkspaceChanging;

        protected void OnWorkspaceChanging(WorkspaceChangeEventArgs e)
        {
            var handler = WorkspaceChanging;
            if (handler != null)
                handler(this, e);
        }

        public event WorkspaceChangedEventHandler WorkspaceChanged;

        protected void OnWorkspaceChanged(WorkspaceChangeEventArgs e)
        {
            var handler = WorkspaceChanged;
            if (handler != null)
                handler(this, e);
        }

        private async Task<bool> Execute(Tuple<ICadCommand, string> commandPair, object arg)
        {
            var command = commandPair.Item1;
            var display = commandPair.Item2;
            var outputService = GetService<IOutputService>();
            OnCommandExecuting(new CadCommandExecutingEventArgs(command));
            outputService.WriteLine(display);
            bool result;
            try
            {
                result = await command.Execute(this, arg);
            }
            catch (Exception ex)
            {
                outputService.WriteLine("Error: {0} - {1}", ex.GetType().ToString(), ex.Message);
                result = false;
            }

            RubberBandGenerator = null;
            OnCommandExecuted(new CadCommandExecutedEventArgs(command));
            return result;
        }

        public async Task<bool> ExecuteCommand(string commandName, object arg)
        {
            if (commandName == null && lastCommand == null)
            {
                return false;
            }

            lock (executeGate)
            {
                if (IsCommandExecuting)
                    return false;
                IsCommandExecuting = true;
            }

            commandName = commandName ?? lastCommand;
            var debugService = GetService<IDebugService>();
            var outputService = GetService<IOutputService>();
            debugService.Add(new WorkspaceLogEntry(string.Format("execute {0}", commandName)));
            var commandPair = GetCommand(commandName);
            if (commandPair == null)
            {
                outputService.WriteLine("Command {0} not found", commandName);
                IsCommandExecuting = false;
                return false;
            }

            var selectedStart = SelectedEntities;
            var result = await Execute(commandPair, arg);
            lastCommand = commandName;
            lock (executeGate)
            {
                IsCommandExecuting = false;
                SelectedEntities = selectedStart;
            }

            return result;
        }

        public bool CommandExists(string commandName)
        {
            return GetCommand(commandName) != null;
        }

        public bool CanExecute()
        {
            return !this.IsCommandExecuting;
        }

        public abstract Task<UnsavedChangesResult> PromptForUnsavedChanges();

        #endregion

        #region Privates

        private string lastCommand = null;
        private object executeGate = new object();

        public virtual Tuple<ICadCommand, string> GetCommand(string commandName)
        {
            var candidateCommands =
                from c in Commands
                let data = c.Metadata
                where string.Compare(data.Name, commandName, StringComparison.OrdinalIgnoreCase) == 0
                   || data.CommandAliases.Any(alias => string.Compare(alias, commandName, StringComparison.OrdinalIgnoreCase) == 0)
                select c;
            var command = candidateCommands.FirstOrDefault();
            if (candidateCommands.Count() > 1)
            {
                throw new InvalidOperationException($"Ambiguous command name '{commandName}'.  Possibilities: {string.Join(", ", candidateCommands.Select(c => c.Metadata.Name))}");
            }

            return command == null ? null : Tuple.Create(command.Value, command.Metadata.DisplayName);
        }

        #endregion
    }
}
