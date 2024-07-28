using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Better.BuildInfo.Internal
{
    public sealed class MultilineHeaderAttribute : PropertyAttribute
    {
        public MultilineHeaderAttribute(string text)
        {
            header = text;
        }

        public string header { get; private set; }
    }

    [CustomPropertyDrawer(typeof(MultilineHeaderAttribute))]
    internal sealed class MultilineHeaderDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            position.y += 8;
            position.height -= 10;
            position = EditorGUI.IndentedRect(position);

            GUI.Label(position, (attribute as MultilineHeaderAttribute).header, EditorStyles.boldLabel);
        }

        public override float GetHeight()
        {
            var header = (this.attribute as MultilineHeaderAttribute);
            var size = EditorStyles.boldLabel.CalcSize(new GUIContent(header.header));
            return size.y + 10;
        }
    }
}
