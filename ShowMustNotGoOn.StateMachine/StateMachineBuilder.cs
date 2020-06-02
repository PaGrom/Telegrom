using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Autofac;
using Autofac.Core;
using ShowMustNotGoOn.Core.Extensions;
using ShowMustNotGoOn.StateMachine.Builder;

namespace ShowMustNotGoOn.StateMachine
{
    public class StateMachineBuilder
    {
        private readonly ContainerBuilder _builder;
        private StateNode _initStateNode;
        private StateNode _defaultStateNode;

        private int _statesCount;

        public string InitStateName => _initStateNode?.GeneratedTypeName;
        public string DefaultStateName => _defaultStateNode?.GeneratedTypeName;

        public StateMachineBuilder(ContainerBuilder builder)
        {
            _builder = builder;
        }

        public StateNode AddInit<TInit>() where TInit: IState
        {
            _initStateNode = new StateNode(typeof(TInit));
            _defaultStateNode = _initStateNode;

            return _initStateNode;
        }

        public void SetDefaultStateNode(StateNode stateNode)
        {
            _defaultStateNode = stateNode;
        }

        public void Build()
        {
            Register(_initStateNode);
#if DEBUG
            GraphvizGenerate();
#endif
        }

        private void Register(StateNode node)
        {
            if (node == null || node.GeneratedTypeName != null)
            {
                return;
            }

            node.GeneratedTypeName = $"{nameof(GeneratedState)}<{node.StateType.Name}" +
                                     $"{(node.IfStates.Any() ? "->If:" : "")}{string.Join(';', node.IfStates.Select(s => s.StateNode.StateType.Name))}" +
                                     $"{(node.ElseState == null ? "" : $"->Else:{node.ElseState.StateNode.StateType.Name}")}>" +
                                     $"{_statesCount++}";

            _builder.RegisterType<GeneratedState>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType.IsAssignableFrom(typeof(IState)),
                        (pi, ctx) => ctx.ResolveNamed<IState>(node.StateType.Name)))
                .WithParameter(new TypedParameter(typeof(StateNode), node))
                .Named<IState>(node.GeneratedTypeName)
                .InstancePerUpdate();

            foreach (var ifState in node.IfStates)
            {
                Register(ifState.StateNode);
            }

            Register(node.ElseState?.StateNode);
        }

        private void GraphvizGenerate()
        {
            var rgx = new Regex("[^a-zA-Z0-9]");
            var generatedNodes = new HashSet<string>();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("digraph {");
            GenerateNode(_initStateNode);
            stringBuilder.AppendLine("}");

            var digraph = stringBuilder.ToString();

            Console.WriteLine(digraph);

            Console.WriteLine(GenerateAscii(digraph));

            void GenerateNode(StateNode node)
            {
                var generatedTypeName = rgx.Replace(node.GeneratedTypeName, "");

                if (generatedNodes.Contains(generatedTypeName))
                {
                    return;
                }

                generatedNodes.Add(generatedTypeName);

                stringBuilder.AppendLine($"\t{generatedTypeName} [ label = \"{node.StateType.Name}\" ]");

                var ifCount = 0;
                foreach (var ifState in node.IfStates)
                {
                    var nextNodeGeneratedTypeName = rgx.Replace(ifState.StateNode.GeneratedTypeName, "");
                    stringBuilder.AppendLine($"\t{generatedTypeName} -> {nextNodeGeneratedTypeName} [ label = \"{node.NextStateKind} If{(ifCount == 0 ? "" : $"{ifCount}")}\" ]");
                    ifCount++;
                    GenerateNode(ifState.StateNode);
                }

                if (node.ElseState != null)
                {
                    var nextNodeGeneratedTypeName = rgx.Replace(node.ElseState.StateNode.GeneratedTypeName, "");
                    stringBuilder.AppendLine($"\t{generatedTypeName} -> {nextNodeGeneratedTypeName} [ label = \"{node.NextStateKind}{(node.IfStates.Any() ? " Else" : "")}\" ]");
                    GenerateNode(node.ElseState.StateNode);
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
