using System;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  namespace Node {
    public class NodeConfig {
      public GUIStyle NodeStyle;
      public GUIStyle SelectedNodeStyle;
      public GUIStyle InPointStyle;
      public GUIStyle OutPointStyle;

      public NodeConfig() {
        var nodeBorder = new RectOffset(12, 12, 12, 12);

        NodeStyle = new GUIStyle();
        NodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        NodeStyle.border = nodeBorder;

        SelectedNodeStyle = new GUIStyle();
        SelectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        SelectedNodeStyle.border = nodeBorder;

        InPointStyle = new GUIStyle();
        InPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        InPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        InPointStyle.border = new RectOffset(4, 4, 12, 12);

        OutPointStyle = new GUIStyle();
        OutPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        OutPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        OutPointStyle.border = new RectOffset(4, 4, 12, 12);
      }
    }

    public abstract class WowNode {
      public Rect rect;
      public string title;
      public bool isDragged;
      public bool isSelected;

      protected INodeEditor _controller;
      protected Vector2 _size = new Vector2(200f, 200f);

      public WowNode(INodeEditor controller, Vector2 position) {
        _controller = controller;
        rect = new Rect(position.x, position.y, _size.x, _size.y);
      }

      public void Drag(Vector2 delta) {
        rect.position += delta;
      }

      public void Draw() {
        GUI.BeginGroup(rect);
        GUILayout.BeginVertical("box", GUILayout.Height(200f));
        DrawContent();
        GUILayout.EndVertical();
        GUI.EndGroup();
      }

      public abstract void DrawContent();

      public bool ProcessEvents(Event e) {
        switch (e.type) {
          case EventType.MouseDown:
            if (e.button == 0) {
              if (rect.Contains(e.mousePosition)) {
                isDragged = true;
                GUI.changed = true;
                isSelected = true;
              } else {
                GUI.changed = true;
                isSelected = false;
              }
            }

            if (e.button == 1 && isSelected && rect.Contains(e.mousePosition)) {
              ProcessContextMenu();
              e.Use();
            }
            break;

          case EventType.MouseUp:
            isDragged = false;
            break;

          case EventType.MouseDrag:
            if (e.button == 0 && isDragged) {
              Drag(e.delta);
              e.Use();
              return true;
            }
            break;
        }

        return false;
      }

      void ProcessContextMenu() {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove node"), false, () => _controller.OnRemoveNode?.Invoke(this));
        genericMenu.ShowAsContext();
      }
    }

    public enum ConnectionPointType { In, Out }

    public class NodeConnectionPoint {
      public Rect rect;

      public ConnectionPointType type;

      public WowNode node;

      public GUIStyle style;

      public Action<NodeConnectionPoint> OnClickConnectionPoint;

      public NodeConnectionPoint(WowNode node, ConnectionPointType type, GUIStyle style, Action<NodeConnectionPoint> OnClickConnectionPoint) {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
      }

      public void Draw() {
        rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

        switch (type) {
          case ConnectionPointType.In:
            rect.x = node.rect.x - rect.width + 8f;
            break;

          case ConnectionPointType.Out:
            rect.x = node.rect.x + node.rect.width - 8f;
            break;
        }

        if (GUI.Button(rect, "", style)) {
          if (OnClickConnectionPoint != null) {
            OnClickConnectionPoint(this);
          }
        }
      }
    }

    public class NodeConnection {
      public NodeConnectionPoint inPoint;
      public NodeConnectionPoint outPoint;
      public Action<NodeConnection> OnClickRemoveConnection;

      public NodeConnection(NodeConnectionPoint inPoint, NodeConnectionPoint outPoint, Action<NodeConnection> OnClickRemoveConnection) {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
      }

      public void Draw() {
        Handles.DrawBezier(
            inPoint.rect.center,
            outPoint.rect.center,
            inPoint.rect.center + Vector2.left * 50f,
            outPoint.rect.center - Vector2.left * 50f,
            Color.white,
            null,
            2f
        );

        if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap)) {
          if (OnClickRemoveConnection != null) {
            OnClickRemoveConnection(this);
          }
        }
      }
    }
  }
}

