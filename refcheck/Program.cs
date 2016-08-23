using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace refcheck
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                System.Environment.Exit(1);
            }

            var checker = new ReferenceChecker(args[0]);
            checker.Check();

            //Console.ReadLine();
        }

        static void usage()
        {
            Console.Out.WriteLine("usage: refcheck (dir)");
        }
    }

    public class ReferenceChecker
    {
        private string directory;


        public ReferenceChecker(string directory)
        {
            this.directory = directory;
        }

        public void Check()
        {
            Console.WriteLine("## project references {0}", directory);
            Console.WriteLine("");

            var references = new List<ReferenceNode>();
            var files = System.IO.Directory.GetFiles(directory, "*.csproj", System.IO.SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var node = ReadProject(file);
                references.Add(node);
            }
            Console.WriteLine("## references check {0}", directory);
            Console.WriteLine("");

            foreach (var reference in references)
            {
                WalkReferences(references, reference, 0, reference.ProjectName);
            }
            
        }

        private ReferenceNode ReadProject(string projectFileName)
        {
            var project = new Microsoft.Build.Evaluation.Project(projectFileName);
            var node = new ReferenceNode(System.IO.Path.GetFileNameWithoutExtension(projectFileName), "project");
            Console.WriteLine("### project {0}", node.ProjectName);
            Console.WriteLine("");

            node.References.AddRange(ReadAssemblyReference(project));
            node.References.AddRange(ReadProjectReference(project));

            Console.WriteLine("");

            return node;
        }
        private List<ReferenceNode> ReadAssemblyReference(Microsoft.Build.Evaluation.Project project)
        {
            var list = new List<ReferenceNode>();
            var references = project.GetItems("Reference");
            foreach (var i in references)
            {
                var include = i.EvaluatedInclude;
                var assemblyNameComponents = include.Split(',');
                var node = new ReferenceNode(assemblyNameComponents[0], "assembly");
                list.Add(node);
                Console.WriteLine("* assembly: {0}", node.ProjectName);

            }
            return list;
        }
        private List<ReferenceNode> ReadProjectReference(Microsoft.Build.Evaluation.Project project)
        {
            var list = new List<ReferenceNode>();
            var references = project.GetItems("ProjectReference");
            foreach (var i in references)
            {
                var include = i.EvaluatedInclude;
                var projectName = System.IO.Path.GetFileNameWithoutExtension(include);
                
                var node = new ReferenceNode(projectName, "assembly");
                list.Add(node);

                Console.WriteLine("* project: {0}", node.ProjectName);
            }
            return list;
        }
        private void WalkReferences(List<ReferenceNode> nodeList, ReferenceNode currentNode, int depth, string path)
        {
            Console.Write(new string('\t', depth));
            Console.WriteLine("* {0}", currentNode.ProjectName);

            foreach (var node in currentNode.References)
            {
                var reference = nodeList.FirstOrDefault(x => x.ProjectName == node.ProjectName);
                if (reference == null)
                {
                    continue;
                }
                if (path.Contains(reference.ProjectName + "/"))
                {
                    Console.Write(new string('\t', depth + 1));
                    Console.WriteLine("* {0} __circular reference__", reference.ProjectName);
                    continue;
                }
                if (node.References.Count == 0)
                {
                    node.References.AddRange(reference.References);
                }
                
                WalkReferences(nodeList, node, depth + 1, path + "/" + node.ProjectName);
            }
        }
    }
    public class ReferenceNode
    {
        public string ProjectName { get; set; }
        public string ReferenceType { get; set; }

        public List<ReferenceNode> References { get; set; }

        public ReferenceNode(string name, string type)
        {
            ProjectName = name;
            ReferenceType = type;
            References = new List<ReferenceNode>();
        }
    }
}
