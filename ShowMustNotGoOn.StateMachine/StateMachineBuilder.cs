using System;
using System.Collections.Generic;
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
                                     $"{(node.NextStateKind == null ? "" : $"->{node.NextStateKind}:{node.NextStateNodeIfTrue.StateType.Name}{(node.NextStateNodeElse == null ? "" : $"||{node.NextStateNodeElse.StateType.Name}")}")}>" +
                                     $"{_statesCount++}";

            _builder.RegisterType<GeneratedState>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.ParameterType.IsAssignableFrom(typeof(IState)),
                        (pi, ctx) => ctx.ResolveNamed<IState>(node.StateType.Name)))
                .WithParameter(new TypedParameter(typeof(StateNode), this))
                .Named<IState>(node.GeneratedTypeName)
                .InstancePerUpdate();

            Register(node.NextStateNodeIfTrue);
            Register(node.NextStateNodeElse);
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

                if (node.NextStateNodeIfTrue != null)
                {
                    var nextNodeGeneratedTypeName = rgx.Replace(node.NextStateNodeIfTrue.GeneratedTypeName, "");
                    stringBuilder.AppendLine($"\t{generatedTypeName} -> {nextNodeGeneratedTypeName} [ label = \"{node.NextStateKind} {(node.NextStateCondition == null ? "" : "If")}\" ]");
                    GenerateNode(node.NextStateNodeIfTrue);
                }

                if (node.NextStateNodeElse != null)
                {
                    var nextNodeGeneratedTypeName = rgx.Replace(node.NextStateNodeElse.GeneratedTypeName, "");
                    stringBuilder.AppendLine($"\t{generatedTypeName} -> {nextNodeGeneratedTypeName} [ label = \"{node.NextStateKind} Else\" ]");
                    GenerateNode(node.NextStateNodeElse);
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
