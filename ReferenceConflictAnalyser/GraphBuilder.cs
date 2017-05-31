using ReferenceConflictAnalyser.DataStructures;
using ReferenceConflictAnalyser.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
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

            return _doc;
        }

        #region private members



        private const string XmlNamespace = "http://schemas.microsoft.com/vs/2009/dgml";

        private readonly Dictionary<Category, Color> _categories = new Dictionary<Category, Color>()
        {
            { Category.EntryPoint, Color.LightGreen },
            { Category.Normal , Color.White },
            { Category.ConflictResolved, Color.Khaki },
            { Category.Conflicted, Color.LightSalmon },
            { Category.Missed, Color.Crimson }
        };

        private ReferenceList _referenceList;
        private XmlDocument _doc;


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
            var uniqueAssemblies = new Dictionary<string, Category>();
            foreach (var assembly in _referenceList.Assemblies)
            {
                if (!uniqueAssemblies.ContainsKey(assembly.Key.Name))
                    uniqueAssemblies.Add(assembly.Key.Name, assembly.Value);
            }

            var nodesElement = parent.AppendChild(_doc.CreateElement("Nodes", XmlNamespace));
            foreach (var item in uniqueAssemblies)
                nodesElement.AppendChild(CreateXmlElement("Node", new Dictionary<string, string>
                {
                    { "Id", item.Key.ToLower()},
                    { "Label", item.Key},
                    { "Category", item.Value.ToString()}
                }));
        }

        private void AddLinks(XmlNode parent)
        {
            var linksElement = parent.AppendChild(_doc.CreateElement("Links", XmlNamespace));
            foreach (var reference in _referenceList.References)
                linksElement.AppendChild(CreateXmlElement("Link", new Dictionary<string, string>
                {
                    { "Source", reference.Assembly.Name.ToLower()},
                    { "Target", reference.ReferencedAssembly.Name.ToLower()},
                    { "Label", reference.ReferencedAssembly.Version.ToString()}
                }));
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

        private void AddStyles(XmlNode parent)
        {
            var stylesElement = parent.AppendChild(_doc.CreateElement("Styles", XmlNamespace));
            foreach (var category in _categories)
            {
                stylesElement.AppendChild(CreateStyleElement("Node", 
                    EnumHelper.GetDescription(category.Key), 
                    $"HasCategory('{category.Key}')", 
                    new Dictionary<string, string>
                                {
                                        { "Background", ColorTranslator.ToHtml(category.Value) }
                                }));
            }

            stylesElement.AppendChild(CreateStyleElement("Link",
                      "Link to conflicted reference",
                      $"Target.HasCategory('{Category.Conflicted}')",
                      new Dictionary<string, string>
                      {
                            { "Stroke", ColorTranslator.ToHtml(_categories[Category.Conflicted]) },
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
