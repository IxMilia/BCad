using System.Threading.Tasks;

namespace IxMilia.BCad.Commands
{
    internal class DebugDumpCommand : ICadCommand
    {
        public Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            workspace.OutputService.WriteLine("input service entries:");
            var entries = workspace.DebugService.GetLog();
            foreach (var entry in entries)
            {
                workspace.OutputService.WriteLine("  " + entry.ToString());
            }

            workspace.OutputService.WriteLine("drawing structure:");
            foreach (var layer in workspace.Drawing.Layers.GetValues())
            {
                workspace.OutputService.WriteLine("  layer={0}", layer.Name);
                foreach (var entity in layer.GetEntities())
                {
                    workspace.OutputService.WriteLine("    entity id={0}, detail={1}", entity.Id, entity);
                }
            }

            return Task.FromResult(true);
        }
    }
}
