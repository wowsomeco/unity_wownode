using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  namespace Node {
    public delegate void Ev();
    public delegate void EvWithParam<E>(E param);

    public interface INodeEditor {
      EvWithParam<WowNode> OnRemoveNode { get; set; }
    }

    public class WowNodeEditor<T> : EditorWindow, INodeEditor where T : WowNode {
      public delegate T AddNode(Vector2 pos);

      List<WowNode> _nodes = new List<WowNode>();

      Vector2 _offset;
      Vector2 _drag;

      public AddNode OnAddNode { get; set; }
      public EvWithParam<WowNode> OnRemoveNode { get; set; }

      protected virtual void OnEnable() {
        OnRemoveNode += node => _nodes.Remove(node);
      }

      void OnGUI() {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);
        DrawNodes();
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);
        if (GUI.changed) Repaint();
      }

      void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor) {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        _offset += _drag * 0.5f;
        Vector3 newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++) {
          Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++) {
          Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
      }

      void DrawNodes() {
        for (int i = 0; i < _nodes.Count; i++) {
          _nodes[i].Draw();
        }
      }

      void ProcessEvents(Event e) {
        _drag = Vector2.zero;

        switch (e.type) {
          case EventType.MouseDown:
            // right click
            if (e.button == 1) {
              OnRightClick(e.mousePosition);
            }
            break;

          case EventType.MouseDrag:
            if (e.button == 0) {
              OnDrag(e.delta);
            }
            break;
        }
      }

      void ProcessNodeEvents(Event e) {
        foreach (var node in _nodes) {
          bool guiChanged = node.ProcessEvents(e);
          if (guiChanged) {
            GUI.changed = true;
          }
        }
      }

      void OnRightClick(Vector2 mousePosition) {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add node"), false, () => {
          T node = OnAddNode?.Invoke(mousePosition);
          _nodes.Add(node);
        });
        genericMenu.ShowAsContext();
      }

      void OnDrag(Vector2 delta) {
        _drag = delta;
        _nodes.ForEach(node => node.Drag(delta));
        GUI.changed = true;
      }
    }
  }
}

