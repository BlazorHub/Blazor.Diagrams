﻿using Blazor.Diagrams.Core.Default;
using Blazor.Diagrams.Core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Blazor.Diagrams")]
namespace Blazor.Diagrams.Core
{
    public class DiagramManager
    {
        private readonly List<NodeModel> _nodes;
        private readonly List<DiagramSubManager> _subManagers;
        private readonly Dictionary<Type, Type> _componentByModelMapping;

        public event Action<Model, MouseEventArgs> MouseDown;
        public event Action<Model, MouseEventArgs> MouseMove;
        public event Action<Model, MouseEventArgs> MouseUp;

        public event Action<NodeModel> NodeAdded;
        public event Action<NodeModel> NodeRemoved;
        public event Action<NodeModel, bool> NodeSelectionChanged;
        public event Action<LinkModel> LinkAdded;
        public event Action<LinkModel> LinkAttached;
        public event Action<LinkModel> LinkRemoved;

        public DiagramManager()
        {
            _nodes = new List<NodeModel>();
            _subManagers = new List<DiagramSubManager>();
            _componentByModelMapping = new Dictionary<Type, Type>();

            RegisterSubManager<DragNodeSubManager>();
            RegisterSubManager<SelectionSubManager>();
            RegisterSubManager<DragNewLinkSubManager>();
        }

        public ReadOnlyCollection<NodeModel> Nodes => _nodes.AsReadOnly();
        public IEnumerable<LinkModel> AllLinks => _nodes.SelectMany(n => n.Ports.SelectMany(p => p.Links)).Distinct();
        public NodeModel SelectedNode { get; private set; }

        public void AddNode(NodeModel node)
        {
            _nodes.Add(node);
            NodeAdded?.Invoke(node);
        }

        public void RemoveNode(NodeModel node)
        {
            if (_nodes.Remove(node))
            {
                NodeRemoved?.Invoke(node);
            }
        }

        public LinkModel AddLink(PortModel source, PortModel? target = null)
        {
            var link = new LinkModel(source, target);
            source.AddLink(link);
            target?.AddLink(link);

            if (target == null)
            {
                link.OnGoingPosition = Point.Zero;
            }

            LinkAdded?.Invoke(link);
            return link;
        }

        public void AttachLink(LinkModel link, PortModel targetPort)
        {
            if (link.IsAttached)
                throw new Exception("Link already attached.");

            link.SetTargetPort(targetPort);
            targetPort.AddLink(link);
            link.Refresh();
            LinkAttached?.Invoke(link);
        }

        public void RemoveLink(LinkModel link)
        {
            link.SourcePort.RemoveLink(link);
            link.TargetPort?.RemoveLink(link);
            LinkRemoved?.Invoke(link);
        }

        public void SelectNode(NodeModel node)
        {
            if (SelectedNode == node)
                return;

            UnselectNode();
            SelectedNode = node;
            SelectedNode.Selected = true;
            NodeSelectionChanged?.Invoke(SelectedNode, true);
        }

        public void UnselectNode()
        {
            if (SelectedNode == null)
                return;

            var node = SelectedNode;
            node.Selected = false;
            node.Refresh();
            SelectedNode = null;
            NodeSelectionChanged?.Invoke(node, false);
        }

        public void RegisterSubManager<T>() where T : DiagramSubManager
        {
            var type = typeof(T);
            if (_subManagers.Any(sm => sm.GetType() == type))
                throw new Exception($"SubManager '{type.Name}' already registered.");

            var instance = (DiagramSubManager)Activator.CreateInstance(type, this);
            _subManagers.Add(instance);
        }

        public void UnregisterSubManager<T>() where T : DiagramSubManager
        {
            var subManager = _subManagers.FirstOrDefault(sm => sm.GetType() == typeof(T));
            if (subManager == null)
                return;

            subManager.Dispose();
            _subManagers.Remove(subManager);
        }

        public void RegisterModelComponent<M, C>() where M : Model where C : ComponentBase
        {
            var modelType = typeof(M);
            if (_componentByModelMapping.ContainsKey(modelType))
                throw new Exception($"Component already registered for model '{modelType.Name}'.");

            _componentByModelMapping.Add(modelType, typeof(C));
        }

        public Type GetComponentForModel<M>(M model) where M : Model
        {
            var modelType = model.GetType();
            return _componentByModelMapping.ContainsKey(modelType) ? _componentByModelMapping[modelType] : null;
        }

        internal void OnMouseDown(Model model, MouseEventArgs e) => MouseDown?.Invoke(model, e);

        internal void OnMouseMove(Model model, MouseEventArgs e) => MouseMove?.Invoke(model, e);

        internal void OnMouseUp(Model model, MouseEventArgs e) => MouseUp?.Invoke(model, e);
    }
}
