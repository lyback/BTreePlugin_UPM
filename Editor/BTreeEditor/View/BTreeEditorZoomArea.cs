
using UnityEngine;

namespace BTree.Editor
{
    public class BTreeEditorZoomArea
    {
        private static Matrix4x4 _prevGuiMatrix;
        private static Rect groupRect = new Rect();

        public static Rect Begin(Rect screenCoordsArea, float zoomScale)
        {
            GUI.EndGroup();
            Rect rect = new Rect(screenCoordsArea.x, screenCoordsArea.y, screenCoordsArea.width/zoomScale, screenCoordsArea.height/zoomScale);
            rect.y = rect.y + BTreeEditorUtility.EditorWindowTabHeight;
            GUI.BeginGroup(rect);
            _prevGuiMatrix = GUI.matrix;
            Matrix4x4 matrixx = Matrix4x4.TRS(rect.TopLeft(), Quaternion.identity, Vector3.one);
            Vector3 vector = Vector3.one;
            vector.x = vector.y = zoomScale;
            Matrix4x4 matrixx2 = Matrix4x4.Scale(vector);
            GUI.matrix = (((matrixx * matrixx2) * matrixx.inverse) * GUI.matrix);
            return rect;
        }

        public static void End()
        {
            GUI.matrix = _prevGuiMatrix;
            GUI.EndGroup();
            groupRect.y = BTreeEditorUtility.EditorWindowTabHeight;
            groupRect.width = (float)Screen.width;
            groupRect.height = (float)Screen.height;
            GUI.BeginGroup(groupRect);
        }
    }
}
