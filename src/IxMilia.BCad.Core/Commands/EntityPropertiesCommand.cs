using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.Entities;

namespace IxMilia.BCad.Commands
{
    internal class EntityPropertiesCommand : ICadCommand
    {
        public async Task<bool> Execute(IWorkspace workspace, object arg = null)
        {
            var entity = await workspace.InputService.GetEntity(new UserDirective("Select entity"));
            if (!entity.HasValue || entity.Cancel)
                return false;

            IEnumerable<string> properties;
            switch (entity.Value.Entity.Kind)
            {
                case EntityKind.Aggregate:
                    properties = new[] { "Color", "Location" };
                    break;
                case EntityKind.Arc:
                    properties = new[] { "Center", "Color", "StartAngle", "EndAngle", "Normal", "Radius" };
                    break;
                case EntityKind.Circle:
                    properties = new[] { "Center", "Color", "Normal", "Radius" };
                    break;
                case EntityKind.Ellipse:
                    properties = new[] { "Center", "Color", "Normal", "MajorAxis", "MinorAxisRatio", "StartAngle", "EndAngle" };
                    break;
                case EntityKind.Line:
                    properties = new[] { "Color", "P1", "P2" };
                    break;
                case EntityKind.Polyline:
                    properties = new[] { "Color", "Points" };
                    break;
                case EntityKind.Text:
                    properties = new[] { "Color", "Height", "Location", "Normal", "Rotation", "Value" };
                    break;
                default:
                    throw new ArgumentException("Entity.Kind");
            }

            var details = DetailsFromProperties(entity.Value.Entity, properties);
            workspace.OutputService.WriteLine(details);

            return true;
        }

        private static string DetailsFromProperties(Entity entity, IEnumerable<string> properties)
        {
            var type = entity.GetType();
            var details = new[] { "Kind", "Id" }.Concat(properties).Select(prop => string.Format("{0}: {1}", prop, entity.GetProperty(prop)));
            return string.Join("\n", details);
        }
    }
}
