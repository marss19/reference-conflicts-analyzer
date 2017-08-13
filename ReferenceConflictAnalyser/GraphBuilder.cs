using ReferenceConflictAnalyser.DataStructures;
using ReferenceConflictAnalyser.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReferenceConflictAnalyser
{
    public class GraphBuilder
    {

        public XmlDocument BuildDgml(ReferenceList referenceList)
        {
            _referenceList = referenceList;

            _doc = new XmlDocument();

            var root = AddRootElement();
            AddNodes(root);
            AddLinks(root);
            AddCategories(root);
            AddStyles(root);
            AddProperties(root);

            return _doc;
        }

        #region private members



        private const string XmlNamespace = "http://schemas.microsoft.com/vs/2009/dgml";

        private readonly Dictionary<Category, Color> _categories = new Dictionary<Category, Color>()
        {
            { Category.EntryPoint, Color.LightGreen },
            { Category.Normal , Color.MintCream },
            { Category.VersionsConflicted, Color.LightSalmon },
            { Category.OtherConflict, Color.Coral },
            { Category.VersionsConflictResolved, Color.Khaki },
            { Category.Missed, Color.Crimson },
            { Category.Comment, Color.White },
            { Category.UnusedAssembly, Color.Gray }
        };
        private Color PlatformTargetMismatchBorder = Color.DarkRed;
        private ReferenceList _referenceList;
        private XmlDocument _doc;

        private const string UnusedGroupNodeId = "UnusedGroupNodeId";
        private const string UnusedGroupNodeCommentId = "UnusedGroupNodeCommentId";


        private XmlNode AddRootElement()
        {
            var root = _doc.AppendChild(CreateXmlElement("DirectedGraph", new Dictionary<string, string>
            {
                { "GraphDirection", "BottomToTop"},
                { "Layout", "Sugiyama"}
            }));
            return root;
        }

        private void AddNodes(XmlNode parent)
        {
            var nodesElement = parent.AppendChild(_doc.CreateElement("Nodes", XmlNamespace));
            foreach (var referencedAssembly in _referenceList.Assemblies)
            {
                var nodeId = referencedAssembly.Name.ToLower();

                var attributes = new Dictionary<string, string>
                {
                    { "Id", nodeId},
                    { "Label", referencedAssembly.Name},
                    { "Category", referencedAssembly.Category.ToString()},
                    { "Icon", "CodeSchema_Assembly"}
                };

                if (referencedAssembly.ProcessorArchitecture != ProcessorArchitecture.None)
                {
                    attributes.Add(ExtraNodeProperty.ProcessorArchitecture.ToString(), referencedAssembly.ProcessorArchitecture.ToString());
                }
                nodesElement.AppendChild(CreateXmlElement("Node", attributes));

                if (HasCommentNode(referencedAssembly))
                {
                    nodesElement.AppendChild(CreateXmlElement("Node", new Dictionary<string, string>
                    {
                        { "Id", GetCommentNodeId(nodeId)},
                        { "Label", BuildComment(referencedAssembly)},
                        { "Category", Category.Comment.ToString()}
                    }));
                }
            }

            // add container for unused assemblies and comment for it
            if (_referenceList.Assemblies.Any(x => x.Category == Category.UnusedAssembly))
            {
                nodesElement.AppendChild(CreateXmlElement("Node", new Dictionary<string, string>
                    {
                        { "Id", UnusedGroupNodeId },
                        { "Label", "Unused assemblies?" },
                        { "Group", "Expanded" }
                    }));
                nodesElement.AppendChild(CreateXmlElement("Node", new Dictionary<string, string>
                    {
                        { "Id", UnusedGroupNodeCommentId },
                        { "Label", "These assemblies are not referended by any assembly from the graph explicitly. However, they can be loaded in runtime by Assembly and AppDomain methods." },
                        { "Category", Category.Comment.ToString() }
                    }));
            }
        }

        private string BuildComment(ReferencedAssembly referencedAssembly)
        {
            var comment = new StringBuilder();

            if (referencedAssembly.LoadingError != null)
            {
                comment.AppendLine($"Error message: {referencedAssembly.LoadingError.Message}");
                comment.AppendLine($"Error type: {referencedAssembly.LoadingError.GetType().Name}.");
            }

            if (referencedAssembly.PossibleLoadingErrorCauses != null && referencedAssembly.PossibleLoadingErrorCauses.Any())
            {
                comment.AppendLine($"Details: {string.Join("; ", referencedAssembly.PossibleLoadingErrorCauses)}");
            }

            return comment.ToString();
        }

        private bool HasCommentNode(ReferencedAssembly referencedAssembly)
        {
            return referencedAssembly.LoadingError != null || referencedAssembly.PossibleLoadingErrorCauses.Any();
        }

        private string GetCommentNodeId(string assemblyNodeId)
        {
            return $"{assemblyNodeId}..comment";
        }

        private void AddLinks(XmlNode parent)
        {
            var linksElement = parent.AppendChild(_doc.CreateElement("Links", XmlNamespace));

            //link assemblies
            foreach (var reference in _referenceList.References)
                linksElement.AppendChild(CreateXmlElement("Link", new Dictionary<string, string>
                {
                    { "Source", reference.Assembly.Name.ToLower()},
                    { "Target", reference.ReferencedAssembly.Name.ToLower()},
                    { "Label", reference.ReferencedAssembly.Version.ToString()},
                    { ExtraNodeProperty.SourceNodeDetails.ToString(), reference.Assembly.FullName },
                    { ExtraNodeProperty.TargetNodeDetails.ToString(), reference.ReferencedAssembly.FullName }
                }));

            //link comments to assemblies
            var assembliesWithComments = _referenceList.Assemblies.Where(HasCommentNode);
            foreach (var referencedAssembly in assembliesWithComments)
            {
                var nodeId = referencedAssembly.Name.ToLower();
                linksElement.AppendChild(CreateXmlElement("Link", new Dictionary<string, string>
                {
                    { "Source", nodeId},
                    { "Target", GetCommentNodeId(nodeId)}
                }));
            }

            //group unused assemblies
            var unusedAssemblies = _referenceList.Assemblies.Where(x => x.Category == Category.UnusedAssembly);
            if (unusedAssemblies.Any())
            {
                linksElement.AppendChild(CreateXmlElement("Link", new Dictionary<string, string>
                {
                    { "Source", UnusedGroupNodeId},
                    { "Target", UnusedGroupNodeCommentId}
                }));

                foreach (var assembly in unusedAssemblies)
                {
                    linksElement.AppendChild(CreateXmlElement("Link", new Dictionary<string, string>
                    {
                        { "Source", UnusedGroupNodeId},
                        { "Target", assembly.Name.ToLower()},
                        { "Category", "Contains" }
                    }));

                    if (HasCommentNode(assembly))
                    {
                        linksElement.AppendChild(CreateXmlElement("Link", new Dictionary<string, string>
                        {
                                { "Source", UnusedGroupNodeId},
                                { "Target", GetCommentNodeId(assembly.Name.ToLower())},
                                { "Category", "Contains" }
                        }));
                    }
                }
            }
        }

        private void AddCategories(XmlNode parent)
        {
            var categoriesElement = parent.AppendChild(_doc.CreateElement("Categories", XmlNamespace));
            foreach (var category in _categories)
                categoriesElement.AppendChild(CreateXmlElement("Category", new Dictionary<string, string>
                {
                    { "Id", category.Key.ToString() },
                    { "Label", EnumHelper.GetDescription(category.Key)}
                }));
        }

        private void AddProperties(XmlNode parent)
        {
            var propertiesElement = parent.AppendChild(_doc.CreateElement("Properties", XmlNamespace));

            var properties = EnumHelper.GetValuesWithDescriptions<ExtraNodeProperty>();
            foreach(var property in properties)
            {
                propertiesElement.AppendChild(CreateXmlElement("Property", new Dictionary<string, string>
                {
                    { "Id", property.Key.ToString() },
                    { "DataType", "System.String" },
                    { "Label", property.Value }
                }));
            }
        }

        private void AddStyles(XmlNode parent)
        {
            var stylesElement = parent.AppendChild(_doc.CreateElement("Styles", XmlNamespace));
            foreach (var category in _categories)
            {
                var properties = new Dictionary<string, string>
                {
                     { "Background", ColorTranslator.ToHtml(category.Value) }
                };
                if (category.Key == Category.Comment)
                {
                    properties.Add("MaxWidth", "300");
                    properties.Add("NodeRadius", "15");
                    properties.Add("Foreground", ColorTranslator.ToHtml(Color.Gray));
                }
                    

                stylesElement.AppendChild(CreateStyleElement("Node", 
                    EnumHelper.GetDescription(category.Key), 
                    $"HasCategory('{category.Key}')", 
                    properties
                    ));
            }

            stylesElement.AppendChild(CreateStyleElement("Link",
                      "Link to conflicted reference",
                      $"Target.HasCategory('{Category.VersionsConflicted}')",
                      new Dictionary<string, string>
                      {
                            { "Stroke", ColorTranslator.ToHtml(_categories[Category.VersionsConflicted]) },
                            { "StrokeThickness", "3" }
                      }));

            stylesElement.AppendChild(CreateStyleElement("Link",
                      "Link to missed reference",
                      $"Target.HasCategory('{Category.Missed}')",
                      new Dictionary<string, string>
                      {
                            { "Stroke", ColorTranslator.ToHtml(_categories[Category.Missed]) },
                            { "StrokeThickness", "3" }
                      }));
            stylesElement.AppendChild(CreateStyleElement("Link",
                      "Link to detailed information",
                      $"Target.HasCategory('{Category.Comment}')",
                      new Dictionary<string, string>
                      {
                          { "StrokeDashArray", "2 2" }
                      }));

        }

        private XmlElement CreateXmlElement(string elementName, Dictionary<string, string> attributes)
        {
            var elem = _doc.CreateElement(elementName, XmlNamespace);
            foreach (var attibute in attributes)
                elem.Attributes.Append(CreateXmlAtribute(attibute.Key, attibute.Value));
            return elem;
        }

        private XmlAttribute CreateXmlAtribute(string name, string value)
        {
            var attribute = _doc.CreateAttribute(name);
            attribute.Value = value;
            return attribute;
        }

        private XmlElement CreateStyleElement(string targetType, string groupLabel, string condition, Dictionary<string, string> properties)
        {
            var styleElement = _doc.CreateElement("Style", XmlNamespace);
            styleElement.Attributes.Append(CreateXmlAtribute("TargetType", targetType));
            styleElement.Attributes.Append(CreateXmlAtribute("GroupLabel", groupLabel));

            var conditionElement = _doc.CreateElement("Condition", XmlNamespace);
            conditionElement.Attributes.Append(CreateXmlAtribute("Expression", condition));
            styleElement.AppendChild(conditionElement);

            foreach (var property in properties)
                styleElement.AppendChild(CreateXmlElement("Setter", new Dictionary<string, string>
                {
                     { "Property", property.Key },
                     { "Value", property.Value }
                }));

            return styleElement;
        }

        #endregion
    }
}
