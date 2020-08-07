using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Autofac;
using Autofac.Core;
using Telegrom.Core.Extensions;
using Telegrom.StateMachine.Builder;

[assembly: InternalsVisibleTo("Telegrom")]

namespace Telegrom.StateMachine
{
    public class StateMachineBuilder
    {
        private ContainerBuilder _builder;
        private StateNode _initStateNode;
        private StateNode _defaultStateNode;

        private readonly HashSet<string> _stateNames = new HashSet<string>();

        public string InitStateName => _initStateNode?.StateName;
        public string DefaultStateName => _defaultStateNode?.StateName;

        private static StateMachineBuilder _current;

        public static StateMachineBuilder Current
        {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }

        public StateNode AddInit<TInit>() where TInit: IState
        {
            _initStateNode = new StateNode(typeof(TInit), "init");
            _defaultStateNode = _initStateNode;

            return _initStateNode;
        }

        public void SetDefaultStateNode(StateNode stateNode)
        {
            _defaultStateNode = stateNode;
        }

        internal void Build(ContainerBuilder builder)
        {
            _builder = builder;

            Register(_initStateNode);
#if DEBUG
            var (digraph, ascii) = GraphvizGenerate();

            var digraphLog = "digraph.log";
            File.WriteAllText(digraphLog, digraph);
            File.AppendAllText(digraphLog, ascii);
#endif
        }

        private void Register(StateNode node)
        {
            if (node == null || node.Built)
            {
                return;
            }

            if (_stateNames.Contains(node.StateName))
            {
                throw new Exception($"Can't build state machine graph with not unique state names. Found several nodes with name {node.StateName}");
            }

            _builder.RegisterType<GeneratedState>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType.IsAssignableFrom(typeof(IState)),
                        (pi, ctx) => ctx.ResolveNamed<IState>(node.StateType.Name)))
                .WithParameter(new TypedParameter(typeof(StateNode), node))
                .Named<IState>(node.StateName)
                .InstancePerUpdate();

            node.Built = true;
            _stateNames.Add(node.StateName);

            foreach (var ifState in node.IfStates)
            {
                Register(ifState.StateNode);
            }

            Register(node.DefaultState?.StateNode);
        }

        private (string Digraph, string ascii) GraphvizGenerate()
        {
            var rgx = new Regex("[^a-zA-Z0-9]");
            var generatedNodes = new HashSet<string>();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("digraph {");
            GenerateNode(_initStateNode);
            stringBuilder.AppendLine("}");

            var digraph = stringBuilder.ToString();

            return (digraph, GenerateAscii(digraph));

            void GenerateNode(StateNode node)
            {
                var generatedTypeName = rgx.Replace(node.StateName, "");

                if (generatedNodes.Contains(generatedTypeName))
                {
                    return;
                }

                generatedNodes.Add(generatedTypeName);

                stringBuilder.AppendLine($"\t{generatedTypeName} [ label = \"{node.StateType.Name}\" ]");

                var ifCount = 0;
                foreach (var ifState in node.IfStates)
                {
                    var nextNodeGeneratedTypeName = rgx.Replace(ifState.StateNode.StateName, "");
                    stringBuilder.AppendLine($"\t{generatedTypeName} -> {nextNodeGeneratedTypeName} [ label = \"{node.NextStateKind} If{(ifCount == 0 ? "" : $"{ifCount}")}\" ]");
                    ifCount++;
                    GenerateNode(ifState.StateNode);
                }

                if (node.DefaultState != null)
                {
                    var nextNodeGeneratedTypeName = rgx.Replace(node.DefaultState.StateNode.StateName, "");
                    stringBuilder.AppendLine($"\t{generatedTypeName} -> {nextNodeGeneratedTypeName} [ label = \"{node.NextStateKind}{(node.IfStates.Any() ? " Else" : "")}\" ]");
                    GenerateNode(node.DefaultState.StateNode);
                }
            }
        }

        private string GenerateAscii(string digraph)
        {
            var httpClient = new HttpClient();

            var result =  httpClient.GetAsync($"https://dot-to-ascii.ggerganov.com/dot-to-ascii.php?src={System.Web.HttpUtility.UrlEncode(digraph)}")
                .GetAwaiter().GetResult()
                .Content.ReadAsStringAsync()
                .GetAwaiter().GetResult();
            return result.Replace("&lt;", "<").Replace("&gt;", ">");
        }
    }
}
