﻿using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Core;
using Microsoft.AspNetCore.Components;

namespace SharedDemo.Demos
{
    public class PerformanceCompoent : ComponentBase
    {
        protected readonly DiagramManager diagramManager = new DiagramManager();

        protected override void OnInitialized()
        {
            base.OnInitialized();

            for (int r = 0; r < 10; r++)
            {
                for (int c = 0; c < 10; c += 2)
                {
                    var node1 = new NodeModel(new Point(10 + c * 10 + c * 120, 10 + r * 100));
                    var node2 = new NodeModel(new Point(10 + (c + 1) * 130, 10 + r * 100));

                    var sourcePort = node1.AddPort(PortAlignment.Right);
                    var targetPort = node2.AddPort(PortAlignment.Left);

                    diagramManager.AddNodes(node1, node2);
                    diagramManager.AddLink(sourcePort, targetPort);
                }
            }
        }

        private NodeModel NewNode(double x, double y)
        {
            var node = new NodeModel(new Point(x, y));
            node.AddPort(PortAlignment.Bottom);
            node.AddPort(PortAlignment.Top);
            node.AddPort(PortAlignment.Left);
            node.AddPort(PortAlignment.Right);
            return node;
        }
    }
}
